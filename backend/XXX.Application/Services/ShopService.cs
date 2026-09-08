using Microsoft.Extensions.Logging;
using SqlSugar;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Player;
using XXX.Shop;

namespace XXX.Application.Services
{
    /// <summary>
    /// 商店服务实现。
    /// </summary>
    /// <remarks>
    /// 该实现承担三类职责：
    /// 1. 把数据库里的商店配置加载成可运行时使用的内存缓存。
    /// 2. 负责购买、出售、刷新等会改动库存与玩家资产的真实事务操作。
    /// 3. 负责把领域对象拼装成前端真正可直接消费的商店 DTO，减少前后端字段错位。
    /// </remarks>
    public class ShopService : IShopService
    {
        private readonly ISqlSugarClient _db;
        private readonly ILogger<ShopService> _logger;
        private readonly IGameSyncService _gameSyncService;
        private static readonly Dictionary<string, ShopConfig> SharedShops = [];
        private static readonly Dictionary<string, List<ShopItem>> SharedShopItems = [];
        private static readonly System.Threading.SemaphoreSlim InitializationLock = new(1, 1);
        private readonly Dictionary<string, ShopConfig> _shops = SharedShops;
        private readonly Dictionary<string, List<ShopItem>> _shopItems = SharedShopItems;
        private static DateTime _lastDailyReset = DateTime.Today;
        private static bool _isInitialized;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        /// <param name="logger">日志记录器。</param>
        /// <param name="gameSyncService">排行榜同步服务。</param>
        public ShopService(
            DbContext dbContext,
            ILogger<ShopService> logger,
            IGameSyncService gameSyncService)
        {
            _db = dbContext.Db;
            _logger = logger;
            _gameSyncService = gameSyncService;
        }

        /// <summary>
        /// 初始化商店缓存。
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized && _shops.Count > 0)
            {
                return;
            }

            await ReloadCacheAsync();
        }

        /// <summary>
        /// 重新从数据库装载商店缓存。
        /// </summary>
        public async Task ReloadCacheAsync()
        {
            await InitializationLock.WaitAsync();
            try
            {
                await EnsureSeedDataAsync();
                await LoadCacheFromDatabaseAsync();
                _isInitialized = true;

                _logger.LogInformation("Shop system cache reloaded with {Count} shops", _shops.Count);
            }
            finally
            {
                InitializationLock.Release();
            }
        }

        /// <summary>
        /// 获取当前玩家可访问商店的前端视图。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <returns>玩家可见商店列表。</returns>
        public async Task<List<ShopDto>> GetAccessibleShopViewsAsync(UserEntity player)
        {
            await EnsureInitializedAsync();
            var shops = await GetAccessibleShopsAsync(player);
            var results = new List<ShopDto>(shops.Count);

            foreach (var shop in shops)
            {
                results.Add(await BuildShopDtoAsync(shop, player));
            }

            return results;
        }

        /// <summary>
        /// 获取单个商店的前端视图。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="shopId">商店编号。</param>
        /// <returns>命中的商店视图；不可访问或不存在时返回空。</returns>
        public async Task<ShopDto?> GetShopViewAsync(UserEntity player, string shopId)
        {
            await EnsureInitializedAsync();
            var shop = await GetShopAsync(shopId);
            if (shop == null || !shop.IsAccessible(player))
            {
                return null;
            }

            return await BuildShopDtoAsync(shop, player);
        }

        /// <summary>
        /// 获取全部商店配置。
        /// </summary>
        /// <returns>按排序字段升序返回的商店配置列表。</returns>
        public async Task<List<ShopConfig>> GetAllShopsAsync()
        {
            await EnsureInitializedAsync();
            return _shops.Values.OrderBy(s => s.SortOrder).ToList();
        }

        /// <summary>
        /// 获取单个商店配置。
        /// </summary>
        /// <param name="shopId">商店编号。</param>
        /// <returns>命中的商店配置；不存在时返回空。</returns>
        public async Task<ShopConfig?> GetShopAsync(string shopId)
        {
            await EnsureInitializedAsync();
            return _shops.TryGetValue(shopId, out var config) ? config : null;
        }

        /// <summary>
        /// 获取玩家当前可访问的商店配置列表。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <returns>满足访问条件的商店集合。</returns>
        public async Task<List<ShopConfig>> GetAccessibleShopsAsync(UserEntity player)
        {
            await EnsureInitializedAsync();
            return _shops.Values
                .Where(s => s.IsAccessible(player))
                .OrderBy(s => s.SortOrder)
                .ToList();
        }

        /// <summary>
        /// 购买普通道具。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="shopId">商店编号。</param>
        /// <param name="itemId">道具编号。</param>
        /// <param name="count">购买数量。</param>
        /// <returns>购买结果。</returns>
        public async Task<ShopBuyResult> BuyItemAsync(UserEntity player, string shopId, string itemId, int count = 1)
        {
            await EnsureInitializedAsync();
            if (player == null)
            {
                return new ShopBuyResult { Success = false, Message = "玩家对象不能为空" };
            }

            if (count <= 0)
            {
                return new ShopBuyResult { Success = false, Message = "购买数量必须大于 0" };
            }

            var shop = await GetShopAsync(shopId);
            if (shop == null)
            {
                return new ShopBuyResult { Success = false, Message = "商店不存在" };
            }

            if (!shop.IsAccessible(player))
            {
                return new ShopBuyResult { Success = false, Message = "当前等级不足，暂时无法访问该商店" };
            }

            var shopItem = _shopItems.TryGetValue(shopId, out var items)
                ? items.FirstOrDefault(i => i.ItemId == itemId && i.ItemType == ShopItemType.Item)
                : null;

            if (shopItem == null)
            {
                return new ShopBuyResult { Success = false, Message = "商品不存在" };
            }

            if (!GameData.Items.TryGetValue(itemId, out var itemTemplate))
            {
                return new ShopBuyResult { Success = false, Message = "商品模板不存在，请联系开发检查配置" };
            }

            if (player.Level < itemTemplate.UseLevel)
            {
                return new ShopBuyResult { Success = false, Message = $"需要达到 {itemTemplate.UseLevel} 级后才能购买该道具" };
            }

            if (!shopItem.CanPurchase)
            {
                return new ShopBuyResult { Success = false, Message = "商品已售罄或今日限购已满" };
            }

            if (shopItem.Stock != -1 && shopItem.Stock < count)
            {
                return new ShopBuyResult { Success = false, Message = $"库存不足，当前剩余 {shopItem.Stock}" };
            }

            var todayPurchased = await GetPlayerTodayPurchasedAsync(player.GID, itemId);
            var remainingDaily = shopItem.DailyLimit - todayPurchased;
            if (shopItem.DailyLimit != -1 && remainingDaily < count)
            {
                return new ShopBuyResult { Success = false, Message = $"今日剩余可购买数量为 {remainingDaily}" };
            }

            var totalPrice = (long)shopItem.GetBuyPrice(shop.Discount, 0) * count;
            if (!PlayerManager.HasEnoughGold(player, totalPrice))
            {
                return new ShopBuyResult { Success = false, Message = $"金币不足，需要 {totalPrice}" };
            }

            var now = DateTime.Now;
            try
            {
                // 中文注释：扣钱、扣库存、发放道具、写入限购记录必须放在同一事务中。
                // 这样即便中途任意一步失败，也不会出现“钱扣了但东西没到账”或“东西到账了但库存没扣”的脏数据。
                _db.Ado.BeginTran();

                var affectedRows = await _db.Updateable<UserEntity>()
                    .SetColumns(u => u.Gold == u.Gold - totalPrice)
                    .SetColumns(u => u.TotalGoldSpent == u.TotalGoldSpent + totalPrice)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == player.GID && !u.IsDeleted && u.Gold >= totalPrice)
                    .ExecuteCommandAsync();

                if (affectedRows == 0)
                {
                    throw new InvalidOperationException("金币不足或玩家状态异常");
                }

                if (shopItem.Stock != -1)
                {
                    var stockRows = await _db.Updateable<ShopItemEntity>()
                        .SetColumns(s => s.Stock == s.Stock - count)
                        .Where(s =>
                            s.ShopId == shopId &&
                            s.ItemId == itemId &&
                            s.ItemType == (int)ShopItemType.Item &&
                            s.IsEnabled &&
                            s.Stock >= count)
                        .ExecuteCommandAsync();

                    if (stockRows == 0)
                    {
                        throw new InvalidOperationException("商品库存不足，请刷新后重试");
                    }
                }

                await InventoryItemGrantHelper.AddOrMergeAsync(
                    _db,
                    player.GID,
                    itemId,
                    count,
                    "道具背包已满，请先出售或丢弃道具");

                await UpdatePlayerTodayPurchasedAsync(player.GID, itemId, count, shopId, now);
                _db.Ado.CommitTran();
            }
            catch (InvalidOperationException ex)
            {
                _db.Ado.RollbackTran();
                _logger.LogWarning(
                    ex,
                    "Player {PlayerId} buy item failed. shopId={ShopId}, itemId={ItemId}, count={Count}",
                    player.GID,
                    shopId,
                    itemId,
                    count);
                return new ShopBuyResult { Success = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                _db.Ado.RollbackTran();
                _logger.LogError(
                    ex,
                    "Player {PlayerId} buy item exception. shopId={ShopId}, itemId={ItemId}, count={Count}",
                    player.GID,
                    shopId,
                    itemId,
                    count);
                return new ShopBuyResult { Success = false, Message = "购买失败，请稍后重试" };
            }

            // 中文注释：事务提交成功后再同步内存对象，确保本次接口返回的数据与数据库保持一致。
            player.Gold -= totalPrice;
            player.TotalGoldSpent += totalPrice;
            player.LastUpdateTime = now;
            if (shopItem.Stock != -1)
            {
                shopItem.Stock -= count;
            }

            await _gameSyncService.SyncPlayerAsync(player.GID);

            return new ShopBuyResult
            {
                Success = true,
                Message = $"购买成功，消耗 {totalPrice} 金币",
                TotalCost = totalPrice,
                ItemId = itemId ?? string.Empty,
                Count = count
            };
        }

        /// <summary>
        /// 购买装备商品。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="shopId">商店编号。</param>
        /// <param name="equipmentTemplateId">装备模板编号。</param>
        /// <returns>购买结果。</returns>
        public async Task<ShopBuyResult> BuyEquipmentAsync(UserEntity player, string shopId, int equipmentTemplateId)
        {
            await EnsureInitializedAsync();
            if (player == null)
            {
                return new ShopBuyResult { Success = false, Message = "玩家对象不能为空" };
            }

            var shop = await GetShopAsync(shopId);
            if (shop == null)
            {
                return new ShopBuyResult { Success = false, Message = "商店不存在" };
            }

            if (!shop.IsAccessible(player))
            {
                return new ShopBuyResult { Success = false, Message = "当前等级不足，暂时无法访问该商店" };
            }

            var equipmentId = equipmentTemplateId.ToString();
            var shopItem = _shopItems.TryGetValue(shopId, out var items)
                ? items.FirstOrDefault(i => i.ItemId == equipmentId && i.ItemType == ShopItemType.Equipment)
                : null;

            if (shopItem == null)
            {
                return new ShopBuyResult { Success = false, Message = "装备不存在" };
            }

            if (!shopItem.CanPurchase)
            {
                return new ShopBuyResult { Success = false, Message = "装备已售罄或今日限购已满" };
            }

            if (shopItem.Stock == 0)
            {
                return new ShopBuyResult { Success = false, Message = "装备已售罄" };
            }

            if (!GameData.EquipmentTemplates.TryGetValue(equipmentTemplateId, out var template))
            {
                return new ShopBuyResult { Success = false, Message = "装备模板不存在，请联系开发检查配置" };
            }

            if (player.Level < template.Level)
            {
                return new ShopBuyResult { Success = false, Message = $"需要达到 {template.Level} 级后才能购买该装备" };
            }

            var totalPrice = (long)shopItem.GetBuyPrice(shop.Discount, 0);
            if (!PlayerManager.HasEnoughGold(player, totalPrice))
            {
                return new ShopBuyResult { Success = false, Message = $"金币不足，需要 {totalPrice}" };
            }

            if (!await InventoryCapacityRules.HasEquipmentSlotsAsync(_db, player.GID))
            {
                return new ShopBuyResult { Success = false, Message = "装备背包已满，请先出售或丢弃装备" };
            }

            // 中文注释：购买装备时直接基于模板生成一件真实可落库的装备实例。
            // 这样装备接口拿到的就是“玩家拥有的实例”，而不是还需要二次补全的半成品对象。
            var equipment = CreateEquipmentInstance(template);
            var equipmentEntity = MapToEquipmentEntity(player.GID, equipment);
            var now = DateTime.Now;

            try
            {
                // 中文注释：扣金币、扣库存、写装备实例、写限购记录仍然必须走单事务。
                // 这是保证“装备购买成功”语义真实成立的关键，不允许中途只成功一半。
                _db.Ado.BeginTran();

                var affectedRows = await _db.Updateable<UserEntity>()
                    .SetColumns(u => u.Gold == u.Gold - totalPrice)
                    .SetColumns(u => u.TotalGoldSpent == u.TotalGoldSpent + totalPrice)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == player.GID && !u.IsDeleted && u.Gold >= totalPrice)
                    .ExecuteCommandAsync();

                if (affectedRows == 0)
                {
                    throw new InvalidOperationException("金币不足或玩家状态异常");
                }

                if (shopItem.Stock != -1)
                {
                    var stockRows = await _db.Updateable<ShopItemEntity>()
                        .SetColumns(s => s.Stock == s.Stock - 1)
                        .Where(s =>
                            s.ShopId == shopId &&
                            s.ItemId == equipmentId &&
                            s.ItemType == (int)ShopItemType.Equipment &&
                            s.IsEnabled &&
                            s.Stock >= 1)
                        .ExecuteCommandAsync();

                    if (stockRows == 0)
                    {
                        throw new InvalidOperationException("装备库存不足，请刷新后重试");
                    }
                }

                var insertRows = await _db.Insertable(equipmentEntity).ExecuteCommandAsync();
                if (insertRows == 0)
                {
                    throw new InvalidOperationException("装备入库失败");
                }

                await UpdatePlayerTodayPurchasedAsync(player.GID, equipmentId, 1, shopId, now);
                _db.Ado.CommitTran();
            }
            catch (InvalidOperationException ex)
            {
                _db.Ado.RollbackTran();
                _logger.LogWarning(
                    ex,
                    "Player {PlayerId} buy equipment failed. shopId={ShopId}, templateId={TemplateId}",
                    player.GID,
                    shopId,
                    equipmentTemplateId);
                return new ShopBuyResult { Success = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                _db.Ado.RollbackTran();
                _logger.LogError(
                    ex,
                    "Player {PlayerId} buy equipment exception. shopId={ShopId}, templateId={TemplateId}",
                    player.GID,
                    shopId,
                    equipmentTemplateId);
                return new ShopBuyResult { Success = false, Message = "购买失败，请稍后重试" };
            }

            player.Gold -= totalPrice;
            player.TotalGoldSpent += totalPrice;
            player.LastUpdateTime = now;
            if (shopItem.Stock != -1)
            {
                shopItem.Stock--;
            }

            await _gameSyncService.SyncPlayerAsync(player.GID);

            return new ShopBuyResult
            {
                Success = true,
                Message = $"购买成功，消耗 {totalPrice} 金币",
                TotalCost = totalPrice,
                Equipment = equipment
            };
        }

        /// <summary>
        /// 出售背包道具。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="itemId">道具编号。</param>
        /// <param name="count">出售数量。</param>
        /// <returns>出售结果。</returns>
        public async Task<ShopSellResult> SellItemAsync(UserEntity player, string itemId, int count = 1)
        {
            await EnsureInitializedAsync();
            if (player == null)
            {
                return new ShopSellResult { Success = false, Message = "玩家对象不能为空" };
            }

            if (count <= 0)
            {
                return new ShopSellResult { Success = false, Message = "出售数量必须大于 0" };
            }

            if (!GameData.Items.ContainsKey(itemId))
            {
                return new ShopSellResult { Success = false, Message = "道具不存在" };
            }

            var inventoryItems = await _db.Queryable<InventoryItemEntity>()
                .Where(i => i.PlayerId == player.GID && i.ItemId == itemId)
                .OrderBy(i => i.IsLocked)
                .OrderBy(i => i.Id)
                .ToListAsync();

            var totalQuantity = inventoryItems.Sum(i => i.Quantity);
            if (totalQuantity < count)
            {
                return new ShopSellResult { Success = false, Message = "背包道具数量不足" };
            }

            var item = GameData.Items[itemId];
            var basePrice = item.Quality * 10;
            var totalGold = (int)(basePrice * 0.5f) * count;
            var now = DateTime.Now;

            try
            {
                // 中文注释：出售逻辑必须先扣掉背包道具，再发放金币，避免出现只加钱不扣物品。
                _db.Ado.BeginTran();

                var remainingToSell = count;
                foreach (var inventoryItem in inventoryItems)
                {
                    if (remainingToSell <= 0)
                    {
                        break;
                    }

                    var deductCount = Math.Min(inventoryItem.Quantity, remainingToSell);
                    await DeductInventoryItemWithConcurrencyCheckAsync(inventoryItem, itemId, deductCount);
                    remainingToSell -= deductCount;
                }

                if (remainingToSell > 0)
                {
                    throw new InvalidOperationException("出售失败：道具数量已变化，请重试");
                }

                var affectedRows = await _db.Updateable<UserEntity>()
                    .SetColumns(u => u.Gold == u.Gold + totalGold)
                    .SetColumns(u => u.TotalGoldEarned == u.TotalGoldEarned + totalGold)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == player.GID && !u.IsDeleted)
                    .ExecuteCommandAsync();

                if (affectedRows == 0)
                {
                    throw new InvalidOperationException("玩家状态异常，出售失败");
                }

                _db.Ado.CommitTran();
            }
            catch (InvalidOperationException ex)
            {
                _db.Ado.RollbackTran();
                _logger.LogWarning(
                    ex,
                    "Player {PlayerId} sell item failed. itemId={ItemId}, count={Count}, reason={Reason}",
                    player.GID,
                    itemId,
                    count,
                    ex.Message);
                return new ShopSellResult { Success = false, Message = ex.Message, ItemId = itemId, Count = count };
            }
            catch (Exception ex)
            {
                _db.Ado.RollbackTran();
                _logger.LogError(
                    ex,
                    "Player {PlayerId} sell item exception. itemId={ItemId}, count={Count}",
                    player.GID,
                    itemId,
                    count);
                return new ShopSellResult { Success = false, Message = "出售失败，请稍后重试", ItemId = itemId, Count = count };
            }

            player.Gold += totalGold;
            player.TotalGoldEarned += totalGold;
            player.LastUpdateTime = now;
            await _gameSyncService.SyncPlayerAsync(player.GID);

            _logger.LogInformation(
                "Player {PlayerId} sold item {ItemId} x{Count}, gained gold {Gold}",
                player.GID,
                itemId,
                count,
                totalGold);

            return new ShopSellResult
            {
                Success = true,
                Message = $"出售成功，获得 {totalGold} 金币",
                GoldEarned = totalGold,
                ItemId = itemId ?? string.Empty,
                Count = count
            };
        }

        /// <summary>
        /// 在一个请求、一个事务内批量出售背包道具。
        /// </summary>
        public async Task<BatchSellInventoryItemsResultDto> SellItemsAsync(UserEntity player, BatchSellInventoryItemsRequestDto request)
        {
            var result = new BatchSellInventoryItemsResultDto();
            if (player == null)
            {
                return result;
            }

            var entries = (request?.Items ?? [])
                .Where(item => !string.IsNullOrWhiteSpace(item.ItemId) && item.Count > 0)
                .GroupBy(item => item.ItemId, StringComparer.OrdinalIgnoreCase)
                .Select(group => new BatchSellInventoryItemEntryDto
                {
                    ItemId = group.Key,
                    Count = group.Sum(item => item.Count)
                })
                .ToList();
            if (entries.Count == 0)
            {
                return result;
            }

            var itemIds = entries.Select(item => item.ItemId).ToList();
            var inventoryItems = await _db.Queryable<InventoryItemEntity>()
                .Where(item => item.PlayerId == player.GID && !item.IsLocked && itemIds.Contains(item.ItemId))
                .OrderBy(item => item.ItemId)
                .OrderBy(item => item.Id)
                .ToListAsync();

            var validEntries = new List<BatchSellInventoryItemEntryDto>();
            foreach (var entry in entries)
            {
                if (!GameData.Items.TryGetValue(entry.ItemId, out var itemTemplate))
                {
                    result.FailedItemCount++;
                    continue;
                }

                var available = inventoryItems
                    .Where(item => string.Equals(item.ItemId, entry.ItemId, StringComparison.OrdinalIgnoreCase))
                    .Sum(item => item.Quantity);
                if (available < entry.Count)
                {
                    result.FailedItemCount++;
                    continue;
                }

                validEntries.Add(entry);
                result.GoldEarned += (int)(itemTemplate.Quality * 10 * 0.5f) * entry.Count;
            }

            if (validEntries.Count == 0)
            {
                return result;
            }

            var now = DateTime.Now;
            try
            {
                _db.Ado.BeginTran();
                foreach (var entry in validEntries)
                {
                    var remaining = entry.Count;
                    foreach (var inventoryItem in inventoryItems.Where(item =>
                        string.Equals(item.ItemId, entry.ItemId, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (remaining <= 0) break;
                        var deduct = Math.Min(inventoryItem.Quantity, remaining);
                        await DeductInventoryItemWithConcurrencyCheckAsync(inventoryItem, entry.ItemId, deduct);
                        remaining -= deduct;
                    }

                    if (remaining > 0)
                    {
                        throw new InvalidOperationException("出售失败：背包数据已变化，请重试");
                    }

                    result.SoldItemCount++;
                    result.SoldQuantity += entry.Count;
                }

                var updatedRows = await _db.Updateable<UserEntity>()
                    .SetColumns(user => user.Gold == user.Gold + result.GoldEarned)
                    .SetColumns(user => user.TotalGoldEarned == user.TotalGoldEarned + result.GoldEarned)
                    .SetColumns(user => user.LastUpdateTime == now)
                    .Where(user => user.GID == player.GID && !user.IsDeleted)
                    .ExecuteCommandAsync();
                if (updatedRows == 0)
                {
                    throw new InvalidOperationException("玩家状态异常，出售失败");
                }

                _db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} batch sell items failed", player.GID);
                return new BatchSellInventoryItemsResultDto { FailedItemCount = entries.Count };
            }

            player.Gold += result.GoldEarned;
            player.TotalGoldEarned += result.GoldEarned;
            player.LastUpdateTime = now;
            await _gameSyncService.SyncPlayerAsync(player.GID);
            _logger.LogInformation("Player {PlayerId} batch sold {Count} item type(s), quantity={Quantity}, gained gold {Gold}", player.GID, result.SoldItemCount, result.SoldQuantity, result.GoldEarned);
            return result;
        }

        private async Task DeductInventoryItemWithConcurrencyCheckAsync(
            InventoryItemEntity inventoryItem,
            string itemId,
            int deductCount)
        {
            if (deductCount <= 0)
            {
                return;
            }

            if (deductCount == inventoryItem.Quantity)
            {
                var deleteRows = await _db.Deleteable<InventoryItemEntity>()
                    .Where(i =>
                        i.Id == inventoryItem.Id &&
                        i.PlayerId == inventoryItem.PlayerId &&
                        i.ItemId == itemId &&
                        i.Quantity == inventoryItem.Quantity)
                    .ExecuteCommandAsync();

                if (deleteRows == 0)
                {
                    throw new InvalidOperationException("出售失败：背包数据发生并发冲突，请重试");
                }

                return;
            }

            var updateRows = await _db.Updateable<InventoryItemEntity>()
                .SetColumns(i => i.Quantity == i.Quantity - deductCount)
                .Where(i =>
                    i.Id == inventoryItem.Id &&
                    i.PlayerId == inventoryItem.PlayerId &&
                    i.ItemId == itemId &&
                    i.Quantity >= deductCount)
                .ExecuteCommandAsync();

            if (updateRows == 0)
            {
                throw new InvalidOperationException("出售失败：背包数据发生并发冲突，请重试");
            }
        }

        /// <summary>
        /// 出售单件装备。
        /// </summary>
        /// <param name="player">当前玩家实体。</param>
        /// <param name="equipment">待出售装备实例。</param>
        /// <returns>出售结果。</returns>
        public async Task<ShopSellResult> SellEquipmentAsync(UserEntity player, EquipmentInstance equipment)
        {
            await EnsureInitializedAsync();
            if (player == null)
            {
                return new ShopSellResult { Success = false, Message = "玩家对象不能为空" };
            }

            if (equipment == null || string.IsNullOrWhiteSpace(equipment.InstanceId))
            {
                return new ShopSellResult { Success = false, Message = "装备不存在" };
            }

            var equipmentEntity = await _db.Queryable<EquipmentInstanceEntity>()
                .Where(e => e.PlayerId == player.GID && e.InstanceId == equipment.InstanceId)
                .FirstAsync();
            if (equipmentEntity == null)
            {
                return new ShopSellResult { Success = false, Message = "装备不存在" };
            }

            if (equipmentEntity.IsEquipped)
            {
                return new ShopSellResult { Success = false, Message = "已装备的装备无法出售，请先卸下" };
            }

            if (equipmentEntity.IsLocked)
            {
                return new ShopSellResult { Success = false, Message = "绑定装备无法出售" };
            }

            if (!int.TryParse(equipmentEntity.TemplateId, out var templateId) ||
                !GameData.EquipmentTemplates.ContainsKey(templateId))
            {
                return new ShopSellResult { Success = false, Message = "装备模板不存在" };
            }

            var template = GameData.EquipmentTemplates[templateId];
            var totalGold = GetEquipmentSellPrice(template, equipmentEntity.EnhanceLevel);
            if (totalGold <= 0)
            {
                return new ShopSellResult { Success = false, Message = "装备价格异常，无法出售" };
            }

            var now = DateTime.Now;
            try
            {
                // 中文注释：删除装备与增加金币必须同事务提交，防止出现装备没了但钱没到账的异常状态。
                _db.Ado.BeginTran();

                var deleteRows = await _db.Deleteable<EquipmentInstanceEntity>()
                    .Where(e =>
                        e.InstanceId == equipmentEntity.InstanceId &&
                        e.PlayerId == player.GID &&
                        !e.IsEquipped &&
                        !e.IsLocked)
                    .ExecuteCommandAsync();

                if (deleteRows == 0)
                {
                    _db.Ado.RollbackTran();
                    return new ShopSellResult { Success = false, Message = "出售失败：装备状态已变化，请重试" };
                }

                var userRows = await _db.Updateable<UserEntity>()
                    .SetColumns(u => u.Gold == u.Gold + totalGold)
                    .SetColumns(u => u.TotalGoldEarned == u.TotalGoldEarned + totalGold)
                    .SetColumns(u => u.LastUpdateTime == now)
                    .Where(u => u.GID == player.GID && !u.IsDeleted)
                    .ExecuteCommandAsync();

                if (userRows == 0)
                {
                    _db.Ado.RollbackTran();
                    return new ShopSellResult { Success = false, Message = "出售失败：玩家状态异常" };
                }

                _db.Ado.CommitTran();
            }
            catch (Exception ex)
            {
                _db.Ado.RollbackTran();
                _logger.LogError(ex, "Player {PlayerId} sell equipment exception. instanceId={InstanceId}", player.GID, equipment.InstanceId);
                return new ShopSellResult { Success = false, Message = "出售失败，请稍后重试" };
            }

            player.Gold += totalGold;
            player.TotalGoldEarned += totalGold;
            player.LastUpdateTime = now;
            await _gameSyncService.SyncPlayerAsync(player.GID);

            return new ShopSellResult
            {
                Success = true,
                Message = $"出售成功，获得 {totalGold} 金币",
                GoldEarned = totalGold,
                InstanceId = equipmentEntity.InstanceId
            };
        }

        /// <summary>
        /// 刷新单个商店库存。
        /// </summary>
        /// <param name="shopId">商店编号。</param>
        /// <returns>刷新成功返回真。</returns>
        public async Task<bool> RefreshShopAsync(string shopId)
        {
            await EnsureInitializedAsync();
            var shop = await GetShopAsync(shopId);
            if (shop == null)
            {
                return false;
            }

            var itemEntities = await _db.Queryable<ShopItemEntity>()
                .Where(i => i.ShopId == shopId && i.IsEnabled)
                .ToListAsync();

            foreach (var itemEntity in itemEntities)
            {
                itemEntity.Stock = itemEntity.InitialStock < 0 ? -1 : itemEntity.InitialStock;
            }

            if (itemEntities.Count > 0)
            {
                await _db.Updateable(itemEntities).ExecuteCommandAsync();
            }

            shop.LastRefreshTime = DateTime.Now;
            if (_shopItems.TryGetValue(shopId, out var cachedItems))
            {
                foreach (var cachedItem in cachedItems)
                {
                    var entity = itemEntities.FirstOrDefault(i =>
                        i.ItemId == cachedItem.ItemId &&
                        i.ItemType == (int)cachedItem.ItemType);

                    if (entity != null)
                    {
                        cachedItem.Stock = entity.Stock;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 刷新全部支持自动刷新的商店。
        /// </summary>
        public async Task RefreshAllShopsAsync()
        {
            await EnsureInitializedAsync();
            foreach (var shop in _shops.Values.OrderBy(s => s.SortOrder))
            {
                if (shop.NeedsRefresh())
                {
                    await RefreshShopAsync(shop.ShopId);
                }
            }
        }

        /// <summary>
        /// 中文注释：
        /// 商店服务当前使用 Scoped 生命周期，请求到来时会重新创建实例。
        /// 如果只在程序启动时初始化一次，那么后续请求拿到的新实例缓存仍然是空的，
        /// 前端就会看到“接口成功但商店列表为空”的假象。
        /// 这里统一做按需初始化兜底，确保任何入口方法被调用时，商店缓存都已经装载完成。
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized && _shops.Count > 0)
            {
                return;
            }

            await InitializeAsync();
        }

        private async Task<int> GetPlayerTodayPurchasedAsync(string playerId, string itemId)
        {
            CheckDailyReset();

            var record = await _db.Queryable<ShopDailyRecordEntity>()
                .Where(r => r.PlayerId == playerId && r.ItemId == itemId && r.RecordDate == DateTime.Today)
                .FirstAsync();

            return record?.PurchasedCount ?? 0;
        }

        private async Task UpdatePlayerTodayPurchasedAsync(
            string playerId,
            string itemId,
            int count,
            string shopId = "",
            DateTime? updateTime = null)
        {
            var now = updateTime ?? DateTime.Now;
            var recordDate = now.Date;
            var record = await _db.Queryable<ShopDailyRecordEntity>()
                .Where(r => r.PlayerId == playerId && r.ItemId == itemId && r.RecordDate == recordDate)
                .FirstAsync();

            if (record == null)
            {
                record = new ShopDailyRecordEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    PlayerId = playerId,
                    ShopId = shopId,
                    ItemId = itemId,
                    PurchasedCount = count,
                    RecordDate = recordDate,
                    LastUpdateTime = now
                };
                await _db.Insertable(record).ExecuteCommandAsync();
                return;
            }

            record.PurchasedCount += count;
            if (!string.IsNullOrWhiteSpace(shopId))
            {
                record.ShopId = shopId;
            }
            record.LastUpdateTime = now;
            await _db.Updateable(record).ExecuteCommandAsync();
        }

        private void CheckDailyReset()
        {
            if (DateTime.Today > _lastDailyReset)
            {
                // 中文注释：这里不直接删数据库记录，因为每日限购本来就是按 RecordDate 隔离查询。
                // 只需要更新进程内日期即可，避免服务长时间运行时出现“昨天的日期还被继续使用”的问题。
                _lastDailyReset = DateTime.Today;
            }
        }

        private int GetEquipmentBasePrice(EquipmentTemplate template)
        {
            var slotBasePrice = template.Slot switch
            {
                EquipmentSlot.Weapon => 760,
                EquipmentSlot.Armor => 650,
                EquipmentSlot.Helmet => 560,
                EquipmentSlot.Pants => 560,
                EquipmentSlot.Boots => 530,
                EquipmentSlot.Necklace => 520,
                EquipmentSlot.Ring => 470,
                _ => 420
            };
            var levelPrice = template.Level * 120;
            var qualityPrice = (int)template.Quality * 160;
            var stylePrice = template.CombatStyle == CombatStyle.Magic ? 30 : 0;
            return slotBasePrice + levelPrice + qualityPrice + stylePrice;
        }

        private int GetEquipmentSellPrice(EquipmentTemplate template, int enhanceLevel)
        {
            var basePrice = GetEquipmentBasePrice(template);
            var qualityBonus = template.QualityMultiplier;
            var enhanceBonus = 1.0f + enhanceLevel * 0.1f;
            return (int)(basePrice * qualityBonus * enhanceBonus * 0.5f);
        }

        private ShopConfig MapToConfig(ShopConfigEntity entity)
        {
            return new ShopConfig
            {
                ShopId = entity.ShopId,
                ShopName = entity.ShopName,
                ShopType = (ShopType)entity.ShopType,
                Description = entity.Description ?? string.Empty,
                RequiredLevel = entity.RequiredLevel,
                RequiredVipLevel = entity.RequiredVipLevel,
                Discount = entity.Discount,
                IsOpen = entity.IsOpen,
                AutoRefresh = entity.AutoRefresh,
                RefreshIntervalHours = entity.RefreshIntervalHours,
                Icon = entity.Icon ?? string.Empty,
                SortOrder = entity.SortOrder
            };
        }

        private ShopItem MapToShopItem(ShopItemEntity entity)
        {
            return new ShopItem
            {
                ItemId = entity.ItemId,
                ItemType = (ShopItemType)entity.ItemType,
                BasePrice = entity.BasePrice,
                CurrentPrice = entity.CurrentPrice,
                Stock = entity.Stock,
                InitialStock = entity.InitialStock,
                DailyLimit = entity.DailyLimit
            };
        }

        /// <summary>
        /// 确保数据库中的商店种子数据与当前 GameData 一致。
        /// </summary>
        private async Task EnsureSeedDataAsync()
        {
            var shopCount = await _db.Queryable<ShopConfigEntity>().CountAsync();
            var itemCount = await _db.Queryable<ShopItemEntity>().CountAsync();

            if (shopCount > 0 && itemCount > 0)
            {
                return;
            }

            _logger.LogError(
                "Shop configs are incomplete. ShopCount={ShopCount}, ItemCount={ItemCount}. Runtime shop seeding has been disabled; database configs are required.",
                shopCount,
                itemCount);
            throw new InvalidOperationException(
                $"Shop configs are incomplete. ShopCount={shopCount}, ItemCount={itemCount}. Please run startup seed sync before using shops.");
        }

        /// <summary>
        /// 从数据库重新装载商店缓存。
        /// </summary>
        private async Task LoadCacheFromDatabaseAsync()
        {
            var shopEntities = await _db.Queryable<ShopConfigEntity>()
                .Where(s => s.IsEnabled)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();
            var itemEntities = await _db.Queryable<ShopItemEntity>()
                .Where(i => i.IsEnabled)
                .OrderBy(i => i.ShopId)
                .OrderBy(i => i.SortOrder)
                .ToListAsync();

            _shops.Clear();
            _shopItems.Clear();

            foreach (var entity in shopEntities)
            {
                _shops[entity.ShopId] = MapToConfig(entity);
            }

            foreach (var itemEntity in itemEntities)
            {
                if (!_shopItems.TryGetValue(itemEntity.ShopId, out var items))
                {
                    items = [];
                    _shopItems[itemEntity.ShopId] = items;
                }

                items.Add(MapToShopItem(itemEntity));
            }

            foreach (var shop in _shops.Values)
            {
                if (!_shopItems.TryGetValue(shop.ShopId, out var items))
                {
                    items = [];
                    _shopItems[shop.ShopId] = items;
                }

                shop.Items = items;
            }
        }

        /// <summary>
        /// 把领域商店对象组装成前端可直接渲染的 DTO。
        /// </summary>
        private async Task<ShopDto> BuildShopDtoAsync(ShopConfig shop, UserEntity player)
        {
            var recordDate = DateTime.Today;
            var todayRecords = await _db.Queryable<ShopDailyRecordEntity>()
                .Where(r => r.PlayerId == player.GID && r.ShopId == shop.ShopId && r.RecordDate == recordDate)
                .ToListAsync();
            var purchasedTodayMap = todayRecords.ToDictionary(r => r.ItemId, r => r.PurchasedCount);

            var items = _shopItems.TryGetValue(shop.ShopId, out var shopItems)
                ? shopItems
                : [];

            return new ShopDto
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                ShopType = shop.ShopType.ToString(),
                Description = shop.Description,
                Icon = string.IsNullOrWhiteSpace(shop.Icon) ? $"shop-{shop.ShopType.ToString().ToLowerInvariant()}" : shop.Icon,
                IsOpen = shop.IsOpen,
                RequiredLevel = shop.RequiredLevel,
                Discount = shop.Discount,
                AutoRefresh = shop.AutoRefresh,
                RefreshIntervalHours = shop.RefreshIntervalHours,
                Items = items.Select(item => MapToShopItemDto(shop, item, player, purchasedTodayMap)).ToList()
            };
        }

        /// <summary>
        /// 将商店商品映射成前端 DTO。
        /// </summary>
        private ShopItemDto MapToShopItemDto(
            ShopConfig shop,
            ShopItem shopItem,
            UserEntity player,
            IReadOnlyDictionary<string, int> purchasedTodayMap)
        {
            var purchasedToday = purchasedTodayMap.TryGetValue(shopItem.ItemId, out var value) ? value : 0;
            var remainingDailyLimit = shopItem.DailyLimit < 0
                ? -1
                : Math.Max(0, shopItem.DailyLimit - purchasedToday);

            var dto = new ShopItemDto
            {
                ItemId = shopItem.ItemId,
                TemplateId = shopItem.ItemId,
                ItemType = shopItem.ItemType.ToString(),
                Price = BuildPriceDto(shop, shopItem),
                Stock = shopItem.Stock,
                DailyLimit = shopItem.DailyLimit,
                PurchasedToday = purchasedToday,
                RemainingDailyLimit = remainingDailyLimit,
                IsSoldOut = shopItem.Stock == 0,
                RequiredVipLevel = shop.RequiredVipLevel
            };

            if (shopItem.ItemType == ShopItemType.Item && GameData.Items.TryGetValue(shopItem.ItemId, out var itemTemplate))
            {
                dto.Name = itemTemplate.Name;
                dto.Description = itemTemplate.Description;
                dto.Icon = GetItemIcon(shopItem.ItemId, itemTemplate);
                dto.Quality = itemTemplate.Quality <= 0 ? 1 : itemTemplate.Quality;
                dto.QualityName = GetQualityName(dto.Quality);
                dto.RequiredLevel = Math.Max(shop.RequiredLevel, itemTemplate.UseLevel);
                dto.SellPrice = shopItem.GetSellPrice();
            }
            else if (shopItem.ItemType == ShopItemType.Equipment &&
                int.TryParse(shopItem.ItemId, out var templateId) &&
                GameData.EquipmentTemplates.TryGetValue(templateId, out var equipmentTemplate))
            {
                dto.Name = equipmentTemplate.Name;
                dto.Description = equipmentTemplate.Description;
                dto.Icon = GetEquipmentIcon(equipmentTemplate);
                dto.Quality = (int)equipmentTemplate.Quality;
                dto.QualityName = GetQualityName(dto.Quality);
                dto.RequiredLevel = Math.Max(shop.RequiredLevel, equipmentTemplate.Level);
                dto.SellPrice = GetEquipmentSellPrice(equipmentTemplate, 0);
                dto.Stats = BuildEquipmentShopStats(equipmentTemplate);
            }
            else
            {
                dto.Name = shopItem.ItemId;
                dto.Description = "未知商品";
                dto.Icon = "unknown-item";
                dto.Quality = 1;
                dto.QualityName = GetQualityName(1);
                dto.RequiredLevel = shop.RequiredLevel;
                dto.SellPrice = shopItem.GetSellPrice();
            }

            dto.CanPurchase =
                shop.IsOpen &&
                player.Level >= dto.RequiredLevel &&
                (shopItem.Stock != 0) &&
                (shopItem.DailyLimit < 0 || remainingDailyLimit > 0);

            return dto;
        }

        private ShopPriceDto BuildPriceDto(ShopConfig shop, ShopItem shopItem)
        {
            var amount = shopItem.GetBuyPrice(shop.Discount, 0);
            var hasDiscount = amount != shopItem.BasePrice;

            return new ShopPriceDto
            {
                CurrencyType = "Gold",
                Amount = amount,
                OriginalAmount = hasDiscount ? shopItem.BasePrice : null,
                DiscountPercent = hasDiscount ? (int)Math.Round(shop.Discount * 100) : null
            };
        }

        private static List<ShopItemStatDto> BuildEquipmentShopStats(EquipmentTemplate template)
        {
            var stats = new List<ShopItemStatDto>();

            AppendEquipmentStat(stats, "生命", template.MinType1, template.MaxType1);
            AppendEquipmentStat(stats, "法力", template.MinType2, template.MaxType2);
            AppendEquipmentStat(stats, "物攻", template.MinType3, template.MaxType3);
            AppendEquipmentStat(stats, "法攻", template.MinType4, template.MaxType4);
            AppendEquipmentStat(stats, "物防", template.MinType5, template.MaxType5);
            AppendEquipmentStat(stats, "法防", template.MinType6, template.MaxType6);
            AppendEquipmentStat(stats, "速度", template.MinType7, template.MaxType7);
            AppendEquipmentStat(stats, "命中", template.MinType8, template.MaxType8, true);
            AppendEquipmentStat(stats, "闪避", template.MinType9, template.MaxType9, true);
            AppendEquipmentStat(stats, "暴击", template.MinType10, template.MaxType10, true);
            AppendEquipmentStat(stats, "暴伤", template.MinType11, template.MaxType11, true);
            AppendEquipmentStat(stats, "连击", template.MinType12, template.MaxType12, true);
            AppendEquipmentStat(stats, "反击", template.MinType13, template.MaxType13, true);
            AppendEquipmentStat(stats, "破甲", template.MinType14, template.MaxType14, true);
            AppendEquipmentStat(stats, "附伤", template.MinType15, template.MaxType15, true);

            return stats;
        }

        private static void AppendEquipmentStat(
            ICollection<ShopItemStatDto> stats,
            string name,
            int? minValue,
            int? maxValue,
            bool isPercentage = false)
        {
            var resolvedMin = minValue ?? 0;
            var resolvedMax = maxValue ?? resolvedMin;
            if (resolvedMin <= 0 && resolvedMax <= 0)
            {
                return;
            }

            if (resolvedMax < resolvedMin)
            {
                resolvedMax = resolvedMin;
            }

            var suffix = isPercentage ? "%" : string.Empty;
            var valueText = resolvedMin == resolvedMax
                ? $"{resolvedMin}{suffix}"
                : $"{resolvedMin}-{resolvedMax}{suffix}";

            stats.Add(new ShopItemStatDto
            {
                Name = name,
                Value = valueText
            });
        }

        private static string GetItemIcon(string itemId, ItemTable itemTemplate)
        {
            if (!string.IsNullOrWhiteSpace(itemTemplate.IconPath))
            {
                return itemTemplate.IconPath;
            }

            if (string.Equals(itemId, "pet_egg", StringComparison.OrdinalIgnoreCase))
            {
                return "🥚";
            }

            return itemTemplate.Type switch
            {
                ItemType.Consumable => "item-consumable",
                ItemType.Pill => "item-consumable",
                ItemType.Material => "item-material",
                ItemType.Seed => "item-seed",
                ItemType.Chest => "item-chest",
                ItemType.SkillBook => "item-skillbook",
                ItemType.PetEgg => "item-petegg",
                ItemType.Quest => "item-quest",
                ItemType.Gem => "item-gem",
                _ => "item-default"
            };
        }

        private static string GetEquipmentIcon(EquipmentTemplate equipmentTemplate)
        {
            if (!string.IsNullOrWhiteSpace(equipmentTemplate.IconPath))
            {
                return equipmentTemplate.IconPath;
            }

            return $"equipment-{equipmentTemplate.Slot.ToString().ToLowerInvariant()}";
        }

        private static string GetQualityName(int quality)
        {
            return quality switch
            {
                1 => "普通",
                2 => "优秀",
                3 => "稀有",
                4 => "史诗",
                5 => "传说",
                _ => "未知"
            };
        }

        /// <summary>
        /// 根据装备模板生成一件真实装备实例。
        /// </summary>
        private static EquipmentInstance CreateEquipmentInstance(EquipmentTemplate template)
        {
            return new EquipmentInstance
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                Template = template,
                EnhanceLevel = 0,
                RerolledAttrs = [],
                RerollCount = 0,
                Type1 = EquipmentBalanceHelper.RandomRange(template.MinType1, template.MaxType1),
                Type2 = EquipmentBalanceHelper.RandomRange(template.MinType2, template.MaxType2),
                Type3 = EquipmentBalanceHelper.RandomRange(template.MinType3, template.MaxType3),
                Type4 = EquipmentBalanceHelper.RandomRange(template.MinType4, template.MaxType4),
                Type5 = EquipmentBalanceHelper.RandomRange(template.MinType5, template.MaxType5),
                Type6 = EquipmentBalanceHelper.RandomRange(template.MinType6, template.MaxType6),
                Type7 = EquipmentBalanceHelper.RandomRange(template.MinType7, template.MaxType7),
                Type8 = EquipmentBalanceHelper.RandomRange(template.MinType8, template.MaxType8) / 100f,
                Type9 = EquipmentBalanceHelper.RandomRange(template.MinType9, template.MaxType9) / 100f,
                Type10 = EquipmentBalanceHelper.RandomRange(template.MinType10, template.MaxType10) / 100f,
                Type11 = EquipmentBalanceHelper.RandomRange(template.MinType11, template.MaxType11) / 100f,
                Type12 = EquipmentBalanceHelper.RandomRange(template.MinType12, template.MaxType12) / 100f,
                Type13 = EquipmentBalanceHelper.RandomRange(template.MinType13, template.MaxType13) / 100f,
                Type14 = EquipmentBalanceHelper.RandomRange(template.MinType14, template.MaxType14) / 100f,
                Type15 = EquipmentBalanceHelper.RandomRange(template.MinType15, template.MaxType15) / 100f
            };
        }

        private static EquipmentInstanceEntity MapToEquipmentEntity(string playerId, EquipmentInstance equipment)
        {
            return EquipmentBalanceHelper.CreateEntity(playerId, equipment, false);
        }
    }
}
