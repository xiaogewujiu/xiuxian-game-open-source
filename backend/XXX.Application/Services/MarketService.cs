using Microsoft.Extensions.Logging;
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class MarketService : IMarketService
    {
        private const string STATUS_LISTED = "Listed";
        private const string STATUS_SOLD = "Sold";
        private const string STATUS_CANCELLED = "Cancelled";

        private readonly DbContext _dbContext;
        private readonly IRepository<MarketListingEntity> _listingRepository;
        private readonly IRepository<InventoryItemEntity> _inventoryRepository;
        private readonly IRepository<EquipmentInstanceEntity> _equipmentRepository;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<MarketService> _logger;

        public MarketService(
            DbContext dbContext,
            IRepository<MarketListingEntity> listingRepository,
            IRepository<InventoryItemEntity> inventoryRepository,
            IRepository<EquipmentInstanceEntity> equipmentRepository,
            IRepository<UserEntity> userRepository,
            IInventoryService inventoryService,
            ILogger<MarketService> logger)
        {
            _dbContext = dbContext;
            _listingRepository = listingRepository;
            _inventoryRepository = inventoryRepository;
            _equipmentRepository = equipmentRepository;
            _userRepository = userRepository;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        public async Task<List<MarketListingDto>> BrowseAsync(string? itemType, string? currencyType,
            string? keyword, string? sortBy, string? sortOrder, int page, int pageSize)
        {
            var query = _dbContext.Db.Queryable<MarketListingEntity>()
                .Where(l => l.Status == STATUS_LISTED && l.ExpireAt > DateTime.Now);

            if (!string.IsNullOrEmpty(itemType))
                query = query.Where(l => l.ItemType == itemType);
            if (!string.IsNullOrEmpty(currencyType))
                query = query.Where(l => l.CurrencyType == currencyType);
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(l => (l.ItemId != null && l.ItemId.Contains(keyword)) || l.SellerName.Contains(keyword));

            // 排序
            var isAsc = sortOrder?.ToLower() == "asc";
            query = sortBy?.ToLower() switch
            {
                "price" => isAsc ? query.OrderBy(l => l.Price) : query.OrderByDescending(l => l.Price),
                "time" => isAsc ? query.OrderBy(l => l.CreatedAt) : query.OrderByDescending(l => l.CreatedAt),
                _ => query.OrderByDescending(l => l.CreatedAt)
            };

            var listings = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return listings.Select(MapToListingDto).ToList();
        }

        public async Task<List<MarketListingDto>> GetMyListingsAsync(string playerId)
        {
            var listings = await _dbContext.Db.Queryable<MarketListingEntity>()
                .Where(l => l.SellerId == playerId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return listings.Select(MapToListingDto).ToList();
        }

        public async Task<long> ListAsync(string playerId, MarketListDto dto)
        {
            var config = await GetConfigAsync();
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null) throw new InvalidOperationException("玩家不存在。");

            if (dto.Price < config.MinPrice)
                throw new InvalidOperationException($"价格不能低于 {config.MinPrice}。");

            if (dto.CurrencyType != "Gold" && dto.CurrencyType != "SpiritStone")
                throw new InvalidOperationException("不支持的货币类型。");

            // 检查上架数量
            var activeCount = await _dbContext.Db.Queryable<MarketListingEntity>()
                .Where(l => l.SellerId == playerId && l.Status == STATUS_LISTED)
                .CountAsync();
            if (activeCount >= config.MaxListingsPerPlayer)
                throw new InvalidOperationException($"上架数量已达上限 ({config.MaxListingsPerPlayer})。");

            var expireAt = DateTime.Now.AddDays(config.ListingDurationDays);

            _dbContext.BeginTransaction();
            try
            {
                var listing = new MarketListingEntity
                {
                    SellerId = playerId,
                    SellerName = user.Name,
                    Price = dto.Price,
                    CurrencyType = dto.CurrencyType,
                    Status = STATUS_LISTED,
                    CreatedAt = DateTime.Now,
                    ExpireAt = expireAt
                };

                if (dto.ItemType == "Equipment")
                {
                    if (string.IsNullOrEmpty(dto.EquipmentInstanceId))
                        throw new InvalidOperationException("装备实例ID不能为空。");

                    var equipment = await _equipmentRepository.GetFirstAsync(
                        e => e.PlayerId == playerId && e.InstanceId == dto.EquipmentInstanceId);
                    if (equipment == null) throw new InvalidOperationException("装备不存在。");
                    if (equipment.IsEquipped) throw new InvalidOperationException("请先卸下装备。");
                    if (equipment.IsLocked) throw new InvalidOperationException("已锁定装备不可交易。");

                    var alreadyListed = await _dbContext.Db.Queryable<MarketListingEntity>()
                        .AnyAsync(entry => entry.EquipmentInstanceId == equipment.InstanceId && entry.Status == STATUS_LISTED);
                    if (alreadyListed) throw new InvalidOperationException("该装备已上架，不能重复操作。");

                    // 检查装备模板是否可交易
                    var eqTemplate = _dbContext.Db.Queryable<EquipmentTemplateEntity>()
                        .Where(t => t.EquipmentId.ToString() == equipment.TemplateId).First();
                    if (eqTemplate != null && !eqTemplate.IsTradeable)
                        throw new InvalidOperationException("该装备不可交易。");

                    listing.ItemType = "Equipment";
                    listing.EquipmentInstanceId = equipment.InstanceId;
                    listing.ItemId = equipment.TemplateId;
                    listing.Quantity = 1;

                    // 标记装备为已上架（通过设置 IsLocked=true 暂时锁定）
                    await _equipmentRepository.UpdateAsync(equipment);
                }
                else
                {
                    if (!dto.InventoryItemId.HasValue)
                        throw new InvalidOperationException("物品ID不能为空。");

                    var invItem = await _inventoryRepository.GetFirstAsync(
                        i => i.Id == dto.InventoryItemId.Value && i.PlayerId == playerId);
                    if (invItem == null) throw new InvalidOperationException("物品不存在。");
                    if (invItem.IsLocked) throw new InvalidOperationException("已锁定物品不可交易。");
                    if (invItem.Quantity < dto.Quantity) throw new InvalidOperationException("数量不足。");

                    // 检查物品模板是否可交易
                    var itemTemplate = _dbContext.Db.Queryable<ItemTemplateEntity>()
                        .Where(t => t.ItemId == invItem.ItemId).First();
                    if (itemTemplate != null && !itemTemplate.IsTradeable)
                        throw new InvalidOperationException("该物品不可交易。");

                    listing.ItemType = "Item";
                    listing.ItemId = invItem.ItemId;
                    listing.Quantity = dto.Quantity;

                    // 从背包扣除
                    if (invItem.Quantity <= dto.Quantity)
                        await _inventoryRepository.DeleteAsync(invItem.Id);
                    else
                    {
                        invItem.Quantity -= dto.Quantity;
                        await _inventoryRepository.UpdateAsync(invItem);
                    }
                }

                await _listingRepository.AddAsync(listing);
                _dbContext.CommitTransaction();
                return listing.Id;
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }

        public async Task CancelAsync(string playerId, long listingId)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null) throw new InvalidOperationException("商品不存在。");
            if (listing.SellerId != playerId) throw new InvalidOperationException("只能下架自己的商品。");
            if (listing.Status != STATUS_LISTED) throw new InvalidOperationException("商品状态不正确。");

            _dbContext.BeginTransaction();
            try
            {
                // 恢复物品到背包/装备列表
                if (listing.ItemType == "Equipment" && !string.IsNullOrEmpty(listing.EquipmentInstanceId))
                {
                    var equipment = await _equipmentRepository.GetFirstAsync(
                        e => e.InstanceId == listing.EquipmentInstanceId);
                    if (equipment != null)
                    {
                        await _equipmentRepository.UpdateAsync(equipment);
                    }
                }
                else if (!string.IsNullOrEmpty(listing.ItemId))
                {
                    await _inventoryService.AddItemAsync(playerId, new AddItemRequestDto
                    {
                        ItemId = listing.ItemId,
                        Quantity = listing.Quantity,
                        Source = "MarketCancel"
                    });
                }

                listing.Status = STATUS_CANCELLED;
                await _listingRepository.UpdateAsync(listing);
                _dbContext.CommitTransaction();
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }

        public async Task BuyAsync(string playerId, long listingId)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null) throw new InvalidOperationException("商品不存在。");
            if (listing.Status != STATUS_LISTED) throw new InvalidOperationException("商品已售出或已下架。");
            if (listing.ExpireAt <= DateTime.Now) throw new InvalidOperationException("商品已过期。");
            if (listing.SellerId == playerId) throw new InvalidOperationException("不能购买自己的商品。");

            var buyer = await _userRepository.GetByIdAsync(playerId);
            if (buyer == null) throw new InvalidOperationException("买家不存在。");

            var seller = await _userRepository.GetByIdAsync(listing.SellerId);
            if (seller == null) throw new InvalidOperationException("卖家不存在。");

            var totalPrice = listing.Price * listing.Quantity;
            var config = await GetConfigAsync();
            var fee = (long)(totalPrice * config.FeePercent / 100.0);
            var sellerReceives = totalPrice - fee;

            // 检查买家余额
            if (listing.CurrencyType == "Gold" && buyer.Gold < totalPrice)
                throw new InvalidOperationException("金币不足。");
            if (listing.CurrencyType == "SpiritStone" && buyer.SpiritStone < totalPrice)
                throw new InvalidOperationException("灵石不足。");

            if (listing.ItemType == "Equipment" &&
                !await InventoryCapacityRules.HasEquipmentSlotsAsync(_dbContext.Db, playerId))
            {
                throw new InvalidOperationException("装备背包已满，无法购买该装备。");
            }

            _dbContext.BeginTransaction();
            try
            {
                // 扣除买家货币
                if (listing.CurrencyType == "Gold")
                    buyer.Gold -= totalPrice;
                else
                    buyer.SpiritStone -= totalPrice;
                await _userRepository.UpdateAsync(buyer);

                // 卖家收到货币
                if (listing.CurrencyType == "Gold")
                    seller.Gold += sellerReceives;
                else
                    seller.SpiritStone += sellerReceives;
                await _userRepository.UpdateAsync(seller);

                // 转移物品
                if (listing.ItemType == "Equipment" && !string.IsNullOrEmpty(listing.EquipmentInstanceId))
                {
                    var equipment = await _equipmentRepository.GetFirstAsync(
                        e => e.InstanceId == listing.EquipmentInstanceId);
                    if (equipment != null)
                    {
                        equipment.PlayerId = playerId;
                        equipment.LastUpdateTime = DateTime.Now;
                        await _equipmentRepository.UpdateAsync(equipment);
                    }
                }
                else if (!string.IsNullOrEmpty(listing.ItemId))
                {
                    await _inventoryService.AddItemAsync(playerId, new AddItemRequestDto
                    {
                        ItemId = listing.ItemId,
                        Quantity = listing.Quantity,
                        Source = "MarketBuy"
                    });
                }

                listing.Status = STATUS_SOLD;
                listing.BuyerId = playerId;
                listing.SoldAt = DateTime.Now;
                await _listingRepository.UpdateAsync(listing);

                _dbContext.CommitTransaction();
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }

        public async Task<List<MarketHistoryDto>> GetHistoryAsync(string playerId, int page, int pageSize)
        {
            var listings = await _dbContext.Db.Queryable<MarketListingEntity>()
                .Where(l => (l.SellerId == playerId || l.BuyerId == playerId)
                    && (l.Status == STATUS_SOLD || l.Status == STATUS_CANCELLED))
                .OrderByDescending(l => l.SoldAt)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .ToListAsync();

            var config = await GetConfigAsync();

            return listings.Select(l =>
            {
                var totalPrice = l.Price * l.Quantity;
                var fee = l.Status == STATUS_SOLD ? (long)(totalPrice * config.FeePercent / 100.0) : 0;
                return new MarketHistoryDto
                {
                    Id = l.Id,
                    ItemName = l.ItemId ?? l.EquipmentInstanceId ?? "",
                    ItemType = l.ItemType,
                    Quantity = l.Quantity,
                    Price = l.Price,
                    CurrencyType = l.CurrencyType,
                    Fee = fee,
                    SellerName = l.SellerName,
                    BuyerId = l.BuyerId,
                    SoldAt = l.SoldAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsSeller = l.SellerId == playerId
                };
            }).ToList();
        }

        private async Task<AdminMarketConfigDto> GetConfigAsync()
        {
            var configs = await _dbContext.Db.Queryable<MarketConfigEntity>().ToListAsync();
            var dict = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue);

            return new AdminMarketConfigDto
            {
                FeePercent = dict.TryGetValue("FeePercent", out var fp) && int.TryParse(fp, out var fpv) ? fpv : 5,
                MaxListingsPerPlayer = dict.TryGetValue("MaxListingsPerPlayer", out var ml) && int.TryParse(ml, out var mlv) ? mlv : 10,
                ListingDurationDays = dict.TryGetValue("ListingDurationDays", out var ld) && int.TryParse(ld, out var ldv) ? ldv : 7,
                AllowedCurrencies = dict.TryGetValue("AllowedCurrencies", out var ac) ? ac : "Gold,SpiritStone",
                MinPrice = dict.TryGetValue("MinPrice", out var mp) && long.TryParse(mp, out var mpv) ? mpv : 1
            };
        }

        private static MarketListingDto MapToListingDto(MarketListingEntity l) => new()
        {
            Id = l.Id,
            SellerId = l.SellerId,
            SellerName = l.SellerName,
            ItemType = l.ItemType,
            ItemId = l.ItemId,
            EquipmentInstanceId = l.EquipmentInstanceId,
            ItemName = l.ItemId ?? l.EquipmentInstanceId ?? "",
            Quantity = l.Quantity,
            Price = l.Price,
            CurrencyType = l.CurrencyType,
            Status = l.Status,
            CreatedAt = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            ExpireAt = l.ExpireAt.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    public class AdminMarketService : IAdminMarketService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<MarketListingEntity> _listingRepository;
        private readonly ILogger<AdminMarketService> _logger;

        public AdminMarketService(
            DbContext dbContext,
            IRepository<MarketListingEntity> listingRepository,
            ILogger<AdminMarketService> logger)
        {
            _dbContext = dbContext;
            _listingRepository = listingRepository;
            _logger = logger;
        }

        public async Task<AdminMarketConfigDto> GetConfigAsync()
        {
            var configs = await _dbContext.Db.Queryable<MarketConfigEntity>().ToListAsync();
            var dict = configs.ToDictionary(c => c.ConfigKey, c => c.ConfigValue);

            return new AdminMarketConfigDto
            {
                FeePercent = dict.TryGetValue("FeePercent", out var fp) && int.TryParse(fp, out var fpv) ? fpv : 5,
                MaxListingsPerPlayer = dict.TryGetValue("MaxListingsPerPlayer", out var ml) && int.TryParse(ml, out var mlv) ? mlv : 10,
                ListingDurationDays = dict.TryGetValue("ListingDurationDays", out var ld) && int.TryParse(ld, out var ldv) ? ldv : 7,
                AllowedCurrencies = dict.TryGetValue("AllowedCurrencies", out var ac) ? ac : "Gold,SpiritStone",
                MinPrice = dict.TryGetValue("MinPrice", out var mp) && long.TryParse(mp, out var mpv) ? mpv : 1
            };
        }

        public async Task UpdateConfigAsync(AdminMarketConfigDto dto)
        {
            var pairs = new Dictionary<string, string>
            {
                ["FeePercent"] = dto.FeePercent.ToString(),
                ["MaxListingsPerPlayer"] = dto.MaxListingsPerPlayer.ToString(),
                ["ListingDurationDays"] = dto.ListingDurationDays.ToString(),
                ["AllowedCurrencies"] = dto.AllowedCurrencies,
                ["MinPrice"] = dto.MinPrice.ToString()
            };

            foreach (var (key, value) in pairs)
            {
                var existing = await _dbContext.Db.Queryable<MarketConfigEntity>()
                    .Where(c => c.ConfigKey == key).FirstAsync();
                if (existing != null)
                {
                    existing.ConfigValue = value;
                    await _dbContext.Db.Updateable(existing).ExecuteCommandAsync();
                }
                else
                {
                    await _dbContext.Db.Insertable(new MarketConfigEntity
                    {
                        ConfigKey = key,
                        ConfigValue = value
                    }).ExecuteCommandAsync();
                }
            }
        }

        public async Task<List<AdminMarketListingDto>> GetListingsAsync(string? status, string? keyword, int take)
        {
            var query = _dbContext.Db.Queryable<MarketListingEntity>();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(l => l.Status == status);
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(l => l.SellerName.Contains(keyword) || (l.ItemId != null && l.ItemId.Contains(keyword)));

            var listings = await query.OrderByDescending(l => l.CreatedAt).Take(take).ToListAsync();

            return listings.Select(l => new AdminMarketListingDto
            {
                Id = l.Id,
                SellerId = l.SellerId,
                SellerName = l.SellerName,
                ItemType = l.ItemType,
                ItemName = l.ItemId ?? l.EquipmentInstanceId ?? "",
                Quantity = l.Quantity,
                Price = l.Price,
                CurrencyType = l.CurrencyType,
                Status = l.Status,
                CreatedAt = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                ExpireAt = l.ExpireAt.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();
        }

        public async Task ForceCancelAsync(long listingId)
        {
            var listing = await _listingRepository.GetByIdAsync(listingId);
            if (listing == null) throw new InvalidOperationException("商品不存在。");
            if (listing.Status != "Listed") throw new InvalidOperationException("商品状态不正确。");

            _dbContext.BeginTransaction();
            try
            {
                // 恢复物品到卖家
                if (listing.ItemType == "Equipment" && !string.IsNullOrEmpty(listing.EquipmentInstanceId))
                {
                    var equipment = await _dbContext.Db.Queryable<EquipmentInstanceEntity>()
                        .Where(e => e.InstanceId == listing.EquipmentInstanceId).FirstAsync();
                    if (equipment != null)
                    {
                        await _dbContext.Db.Updateable(equipment).ExecuteCommandAsync();
                    }
                }
                else if (!string.IsNullOrEmpty(listing.ItemId) && !string.IsNullOrEmpty(listing.SellerId))
                {
                    await InventoryItemGrantHelper.AddOrMergeAsync(
                        _dbContext.Db,
                        listing.SellerId,
                        listing.ItemId,
                        listing.Quantity,
                        "卖家道具背包已满，无法恢复该道具。");
                }

                listing.Status = "Cancelled";
                await _listingRepository.UpdateAsync(listing);
                _dbContext.CommitTransaction();
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }

        public async Task<List<AdminMarketTransactionDto>> GetTransactionsAsync(string? keyword, int take)
        {
            var query = _dbContext.Db.Queryable<MarketListingEntity>()
                .Where(l => l.Status == "Sold");

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(l => l.SellerName.Contains(keyword) || (l.BuyerId != null && l.BuyerId.Contains(keyword)));

            var listings = await query.OrderByDescending(l => l.SoldAt).Take(take).ToListAsync();

            var config = await GetConfigAsync();

            return listings.Select(l =>
            {
                var totalPrice = l.Price * l.Quantity;
                var fee = (long)(totalPrice * config.FeePercent / 100.0);
                return new AdminMarketTransactionDto
                {
                    Id = l.Id,
                    SellerName = l.SellerName,
                    BuyerId = l.BuyerId,
                    ItemType = l.ItemType,
                    ItemName = l.ItemId ?? l.EquipmentInstanceId ?? "",
                    Quantity = l.Quantity,
                    Price = l.Price,
                    CurrencyType = l.CurrencyType,
                    Fee = fee,
                    SoldAt = l.SoldAt?.ToString("yyyy-MM-dd HH:mm:ss")
                };
            }).ToList();
        }
    }
}
