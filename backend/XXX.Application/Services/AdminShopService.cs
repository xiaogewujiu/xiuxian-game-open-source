#pragma warning disable CS1591
using XXX;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;
using XXX.Shop;

namespace XXX.Application.Services
{
    public class AdminShopService : IAdminShopService
    {
        private readonly IRepository<ShopConfigEntity> _shopConfigRepository;
        private readonly IRepository<ShopItemEntity> _shopItemRepository;
        private readonly IRepository<ShopDailyRecordEntity> _shopDailyRecordRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminShopService(
            IRepository<ShopConfigEntity> shopConfigRepository,
            IRepository<ShopItemEntity> shopItemRepository,
            IRepository<ShopDailyRecordEntity> shopDailyRecordRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _shopConfigRepository = shopConfigRepository;
            _shopItemRepository = shopItemRepository;
            _shopDailyRecordRepository = shopDailyRecordRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminShopConfigListItemDto>> GetConfigsAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _shopConfigRepository.Db.Queryable<ShopConfigEntity>();
            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(shop => shop.ShopId.Contains(normalizedKeyword) || shop.ShopName.Contains(normalizedKeyword));
            }

            var shops = await query.OrderBy(shop => shop.SortOrder).ToListAsync();
            return shops.Select(shop => new AdminShopConfigListItemDto
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                ShopType = shop.ShopType,
                IsOpen = shop.IsOpen,
                IsEnabled = shop.IsEnabled,
                IsBuiltIn = shop.IsBuiltIn,
                BuiltInVersion = shop.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminShopConfigDetailDto?> GetConfigDetailAsync(string shopId)
        {
            if (string.IsNullOrWhiteSpace(shopId)) return null;
            var shop = await _shopConfigRepository.GetByIdAsync(shopId.Trim());
            if (shop == null) return null;
            return new AdminShopConfigDetailDto
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                ShopType = shop.ShopType,
                Description = shop.Description,
                RequiredLevel = shop.RequiredLevel,
                RequiredVipLevel = shop.RequiredVipLevel,
                Discount = shop.Discount,
                IsOpen = shop.IsOpen,
                AutoRefresh = shop.AutoRefresh,
                RefreshIntervalHours = shop.RefreshIntervalHours,
                Icon = shop.Icon,
                SortOrder = shop.SortOrder,
                IsEnabled = shop.IsEnabled,
                IsBuiltIn = shop.IsBuiltIn,
                SeedKey = shop.SeedKey,
                BuiltInVersion = shop.BuiltInVersion,
                LastUpdateTime = shop.LastUpdateTime
            };
        }

        public async Task<AdminShopConfigDetailDto> SaveConfigAsync(AdminShopConfigDetailDto request)
        {
            var shopId = (request.ShopId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(shopId)) throw new InvalidOperationException("商店编号不能为空。");
            if (string.IsNullOrWhiteSpace(request.ShopName)) throw new InvalidOperationException("商店名称不能为空。");
            if (request.Discount <= 0 || request.Discount > 1) throw new InvalidOperationException("商店折扣必须在 0 到 1 之间。");
            if (request.AutoRefresh && request.RefreshIntervalHours <= 0) throw new InvalidOperationException("启用自动刷新时，刷新间隔必须大于 0 小时。");

            var existing = await _shopConfigRepository.GetByIdAsync(shopId);
            if (existing == null)
            {
                existing = new ShopConfigEntity { ShopId = shopId };
                await _shopConfigRepository.AddAsync(ApplyConfig(existing, request));
                await _runtimeRefreshService.ReloadShopCacheAsync();
                return (await GetConfigDetailAsync(shopId))!;
            }

            ApplyConfig(existing, request);
            await _shopConfigRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadShopCacheAsync();
            return (await GetConfigDetailAsync(shopId))!;
        }

        public async Task<bool> DeleteConfigAsync(string shopId)
        {
            var normalizedShopId = (shopId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedShopId)) return false;

            if (await _shopItemRepository.Db.Queryable<ShopItemEntity>().Where(item => item.ShopId == normalizedShopId).AnyAsync())
            {
                throw new InvalidOperationException("当前商店下仍存在商品，请先删除商品后再删除商店配置。");
            }

            if (await _shopDailyRecordRepository.Db.Queryable<ShopDailyRecordEntity>().Where(record => record.ShopId == normalizedShopId).AnyAsync())
            {
                throw new InvalidOperationException("当前商店仍被历史购买记录引用，不能直接删除。");
            }

            var deleteRows = await _shopConfigRepository.DeleteAsync(normalizedShopId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadShopCacheAsync();
            }

            return deleteRows > 0;
        }

        public async Task<List<AdminShopItemListItemDto>> GetItemsAsync(string? shopId = null)
        {
            var normalizedShopId = (shopId ?? string.Empty).Trim();
            var query = _shopItemRepository.Db.Queryable<ShopItemEntity>();
            if (!string.IsNullOrWhiteSpace(normalizedShopId))
            {
                query = query.Where(item => item.ShopId == normalizedShopId);
            }

            var items = await query.OrderBy(item => item.SortOrder).ToListAsync();

            var shopIds = items.Select(i => i.ShopId).Distinct().ToList();
            var shopConfigs = await _shopItemRepository.Db.Queryable<ShopConfigEntity>()
                .Where(s => shopIds.Contains(s.ShopId))
                .ToListAsync();
            var shopNameMap = shopConfigs.ToDictionary(s => s.ShopId, s => s.ShopName);

            return items.Select(item => new AdminShopItemListItemDto
            {
                GID = item.GID,
                ShopId = item.ShopId,
                ShopName = shopNameMap.TryGetValue(item.ShopId, out var sn) ? sn : item.ShopId,
                ItemId = item.ItemId,
                Name = ResolveItemName(item.ItemId, item.ItemType),
                ItemType = item.ItemType,
                CurrentPrice = item.CurrentPrice,
                IsEnabled = item.IsEnabled,
                IsBuiltIn = item.IsBuiltIn,
                BuiltInVersion = item.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminShopItemDetailDto?> GetItemDetailAsync(string gid)
        {
            if (string.IsNullOrWhiteSpace(gid)) return null;
            var item = await _shopItemRepository.GetByIdAsync(gid.Trim());
            if (item == null) return null;
            return new AdminShopItemDetailDto
            {
                GID = item.GID,
                ShopId = item.ShopId,
                ItemId = item.ItemId,
                ItemType = item.ItemType,
                BasePrice = item.BasePrice,
                CurrentPrice = item.CurrentPrice,
                Stock = item.Stock,
                InitialStock = item.InitialStock,
                DailyLimit = item.DailyLimit,
                SortOrder = item.SortOrder,
                IsEnabled = item.IsEnabled,
                IsBuiltIn = item.IsBuiltIn,
                SeedKey = item.SeedKey,
                BuiltInVersion = item.BuiltInVersion,
                LastUpdateTime = item.LastUpdateTime
            };
        }

        public async Task<AdminShopItemDetailDto> SaveItemAsync(AdminShopItemDetailDto request)
        {
            var gid = (request.GID ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(gid))
            {
                gid = Guid.NewGuid().ToString("N");
                request.GID = gid;
            }

            var shopId = (request.ShopId ?? string.Empty).Trim();
            var itemId = (request.ItemId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(shopId) || string.IsNullOrWhiteSpace(itemId)) throw new InvalidOperationException("商店编号和商品编号不能为空。");
            if (request.ItemType is < 0 or > 1) throw new InvalidOperationException("商品类型仅支持 0=道具、1=装备。");
            if (request.Stock < -1 || request.InitialStock < -1 || request.DailyLimit < -1) throw new InvalidOperationException("库存、初始库存和每日限购只能填写 -1 或大于等于 0 的数值。");
            if (request.CurrentPrice < 0 || request.BasePrice < 0) throw new InvalidOperationException("商品价格不能小于 0。");
            if (!await _shopConfigRepository.ExistsAsync(shop => shop.ShopId == shopId)) throw new InvalidOperationException("所属商店不存在，请先创建商店配置。");

            var duplicatedItem = await _shopItemRepository.Db.Queryable<ShopItemEntity>()
                .Where(item => item.ShopId == shopId && item.ItemId == itemId && item.ItemType == request.ItemType && item.GID != gid)
                .AnyAsync();
            if (duplicatedItem) throw new InvalidOperationException("同一商店内已存在相同商品配置，请直接编辑原记录。");

            var existing = await _shopItemRepository.GetByIdAsync(gid);
            if (existing == null)
            {
                existing = new ShopItemEntity { GID = gid };
                await _shopItemRepository.AddAsync(ApplyItem(existing, request));
                await _runtimeRefreshService.ReloadShopCacheAsync();
                return (await GetItemDetailAsync(gid))!;
            }

            ApplyItem(existing, request);
            await _shopItemRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadShopCacheAsync();
            return (await GetItemDetailAsync(gid))!;
        }

        public async Task<bool> DeleteItemAsync(string gid)
        {
            var normalizedGid = (gid ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedGid)) return false;
            var item = await _shopItemRepository.GetByIdAsync(normalizedGid);
            if (item == null) return false;

            var hasDailyRecord = await _shopDailyRecordRepository.Db.Queryable<ShopDailyRecordEntity>()
                .Where(record => record.ShopId == item.ShopId && record.ItemId == item.ItemId)
                .AnyAsync();
            if (hasDailyRecord)
            {
                throw new InvalidOperationException("当前商店商品仍被历史购买记录引用，不能直接删除。");
            }

            var deleteRows = await _shopItemRepository.DeleteAsync(normalizedGid);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadShopCacheAsync();
            }

            return deleteRows > 0;
        }

        private static ShopConfigEntity ApplyConfig(ShopConfigEntity entity, AdminShopConfigDetailDto request)
        {
            entity.ShopName = request.ShopName.Trim();
            entity.ShopType = request.ShopType;
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.RequiredLevel = Math.Max(1, request.RequiredLevel);
            entity.RequiredVipLevel = Math.Max(0, request.RequiredVipLevel);
            entity.Discount = request.Discount;
            entity.IsOpen = request.IsOpen;
            entity.AutoRefresh = request.AutoRefresh;
            entity.RefreshIntervalHours = Math.Max(0, request.RefreshIntervalHours);
            entity.Icon = string.IsNullOrWhiteSpace(request.Icon) ? null : request.Icon.Trim();
            entity.SortOrder = request.SortOrder;
            entity.IsEnabled = request.IsEnabled;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static ShopItemEntity ApplyItem(ShopItemEntity entity, AdminShopItemDetailDto request)
        {
            entity.ShopId = request.ShopId.Trim();
            entity.ItemId = request.ItemId.Trim();
            entity.ItemType = request.ItemType;
            entity.BasePrice = Math.Max(0, request.BasePrice);
            entity.CurrentPrice = Math.Max(0, request.CurrentPrice);
            entity.Stock = request.Stock;
            entity.InitialStock = request.InitialStock;
            entity.DailyLimit = request.DailyLimit;
            entity.SortOrder = request.SortOrder;
            entity.IsEnabled = request.IsEnabled;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static string ResolveItemName(string itemId, int itemType)
        {
            if (itemType == (int)ShopItemType.Equipment && int.TryParse(itemId, out var templateId))
            {
                return GameData.EquipmentTemplates.TryGetValue(templateId, out var eq) ? eq.Name : itemId;
            }

            return GameData.Items.TryGetValue(itemId, out var item) ? item.Name : itemId;
        }
    }
}
#pragma warning restore CS1591
