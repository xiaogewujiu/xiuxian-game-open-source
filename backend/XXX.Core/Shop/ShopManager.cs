using XXX.Entity;
using XXX.Player;

namespace XXX.Shop
{
    /// <summary>
    /// 商店管理器
    /// 负责商店的初始化、购买、出售、刷新等核心功能
    ///
    /// 使用说明：
    /// 1. 游戏启动时调用 Initialize() 初始化所有商店
    /// 2. 使用 BuyItem/BuyEquipment 购买商品
    /// 3. 使用 SellItem/SellEquipment 出售商品
    /// 4. 定期调用 RefreshShop 刷新商店
    /// </summary>
    public static class ShopManager
    {
        /// <summary>
        /// 所有商店的配置
        /// Key: ShopId, Value: ShopConfig
        /// </summary>
        private static Dictionary<string, ShopConfig> shops = [];

        /// <summary>
        /// 玩家每日购买记录
        /// Key: PlayerId, Value: Dictionary<ItemId, TodayPurchasedCount>
        /// </summary>
        private static Dictionary<string, Dictionary<string, int>> playerDailyRecords = [];

        /// <summary>
        /// 上次重置每日记录的日期
        /// </summary>
        private static DateTime lastDailyReset = DateTime.Today;

        #region 初始化

        /// <summary>
        /// 初始化商店系统
        /// 创建所有预配置的商店
        /// </summary>
        public static void Initialize()
        {
            shops.Clear();

            // 创建杂货铺
            CreateGeneralShop();

            // 创建铁匠铺
            CreateBlacksmithShop();

            // 创建魔法商店
            CreateMagicShop();

            // 创建黑市商人
            CreateBlackMarketShop();

            // 创建限时商店
            CreateLimitedShop();
        }

        /// <summary>
        /// 创建杂货铺
        /// 出售基础材料和道具
        /// </summary>
        private static void CreateGeneralShop()
        {
            var shop = new ShopConfig
            {
                ShopId = "shop_general",
                ShopName = "杂货铺",
                ShopType = ShopType.General,
                Description = "出售基础道具和材料",
                RequiredLevel = 1,
                RequiredVipLevel = 0,
                Discount = 1.0f,
                IsOpen = true,
                AutoRefresh = false,
                Icon = "Icons/Shops/General.png",
                SortOrder = 1
            };

            // 添加商品（示例数据，实际应从配置表读取）
            shop.Items.Add(new ShopItem
            {
                ItemId = "material_iron",
                ItemType = ShopItemType.Item,
                BasePrice = 50,
                CurrentPrice = 50,
                Stock = 100,
                InitialStock = 100,
                DailyLimit = -1
            });

            shop.Items.Add(new ShopItem
            {
                ItemId = "material_wood",
                ItemType = ShopItemType.Item,
                BasePrice = 30,
                CurrentPrice = 30,
                Stock = 100,
                InitialStock = 100,
                DailyLimit = -1
            });

            shops[shop.ShopId] = shop;
        }

        /// <summary>
        /// 创建铁匠铺
        /// 出售装备强化材料
        /// </summary>
        private static void CreateBlacksmithShop()
        {
            var shop = new ShopConfig
            {
                ShopId = "shop_blacksmith",
                ShopName = "铁匠铺",
                ShopType = ShopType.Blacksmith,
                Description = "出售装备强化材料",
                RequiredLevel = 10,
                RequiredVipLevel = 0,
                Discount = 1.0f,
                IsOpen = true,
                AutoRefresh = true,
                RefreshIntervalHours = 24,
                Icon = "Icons/Shops/Blacksmith.png",
                SortOrder = 2
            };

            // 强化石
            shop.Items.Add(new ShopItem
            {
                ItemId = "enhance_stone_1",
                ItemType = ShopItemType.Item,
                BasePrice = 100,
                CurrentPrice = 100,
                Stock = 50,
                InitialStock = 50,
                DailyLimit = 10
            });

            // 保护石
            shop.Items.Add(new ShopItem
            {
                ItemId = "protect_stone",
                ItemType = ShopItemType.Item,
                BasePrice = 500,
                CurrentPrice = 500,
                Stock = 20,
                InitialStock = 20,
                DailyLimit = 3
            });

            shops[shop.ShopId] = shop;
        }

        /// <summary>
        /// 创建魔法商店
        /// 出售法术相关道具
        /// </summary>
        private static void CreateMagicShop()
        {
            var shop = new ShopConfig
            {
                ShopId = "shop_magic",
                ShopName = "魔法商店",
                ShopType = ShopType.Magic,
                Description = "出售法术相关道具",
                RequiredLevel = 15,
                RequiredVipLevel = 0,
                Discount = 1.0f,
                IsOpen = true,
                AutoRefresh = true,
                RefreshIntervalHours = 24,
                Icon = "Icons/Shops/Magic.png",
                SortOrder = 3
            };

            // 法力药水材料
            shop.Items.Add(new ShopItem
            {
                ItemId = "material_mana_crystal",
                ItemType = ShopItemType.Item,
                BasePrice = 200,
                CurrentPrice = 200,
                Stock = 30,
                InitialStock = 30,
                DailyLimit = 5
            });

            shops[shop.ShopId] = shop;
        }

        /// <summary>
        /// 创建黑市商人
        /// 出售稀有道具，价格昂贵
        /// </summary>
        private static void CreateBlackMarketShop()
        {
            var shop = new ShopConfig
            {
                ShopId = "shop_blackmarket",
                ShopName = "黑市商人",
                ShopType = ShopType.BlackMarket,
                Description = "出售稀有道具，价格昂贵",
                RequiredLevel = 20,
                RequiredVipLevel = 1,
                Discount = 1.0f,
                IsOpen = true,
                AutoRefresh = true,
                RefreshIntervalHours = 24,
                Icon = "Icons/Shops/BlackMarket.png",
                SortOrder = 4
            };

            // 稀有材料
            shop.Items.Add(new ShopItem
            {
                ItemId = "material_rare_crystal",
                ItemType = ShopItemType.Item,
                BasePrice = 2000,
                CurrentPrice = 2000,
                Stock = 5,
                InitialStock = 5,
                DailyLimit = 1
            });

            shops[shop.ShopId] = shop;
        }

        /// <summary>
        /// 创建限时商店
        /// 限时开放，特殊商品
        /// </summary>
        private static void CreateLimitedShop()
        {
            var shop = new ShopConfig
            {
                ShopId = "shop_limited",
                ShopName = "限时商店",
                ShopType = ShopType.Limited,
                Description = "限时开放，特殊商品",
                RequiredLevel = 1,
                RequiredVipLevel = 0,
                Discount = 0.8f, // 20%折扣
                IsOpen = true,
                AutoRefresh = true,
                RefreshIntervalHours = 12, // 12小时刷新一次
                Icon = "Icons/Shops/Limited.png",
                SortOrder = 5
            };

            // 限时特殊商品
            shop.Items.Add(new ShopItem
            {
                ItemId = "limited_gift_box",
                ItemType = ShopItemType.Item,
                BasePrice = 1000,
                CurrentPrice = 800,
                Stock = 10,
                InitialStock = 10,
                DailyLimit = 2
            });

            shops[shop.ShopId] = shop;
        }

        #endregion

        #region 商店查询

        /// <summary>
        /// 获取所有商店
        /// </summary>
        /// <returns>商店列表</returns>
        public static List<ShopConfig> GetAllShops()
        {
            return shops.Values.OrderBy(s => s.SortOrder).ToList();
        }

        /// <summary>
        /// 根据ID获取商店
        /// </summary>
        /// <param name="shopId">商店ID</param>
        /// <returns>商店配置</returns>
        public static ShopConfig? GetShop(string shopId)
        {
            return shops.ContainsKey(shopId) ? shops[shopId] : null;
        }

        /// <summary>
        /// 获取玩家可访问的商店
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <returns>可访问的商店列表</returns>
        public static List<ShopConfig> GetAccessibleShops(UserEntity player)
        {
            return shops.Values
                .Where(s => s.IsAccessible(player))
                .OrderBy(s => s.SortOrder)
                .ToList();
        }

        #endregion

        #region 购买功能

        /// <summary>
        /// 购买道具
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="shopId">商店ID</param>
        /// <param name="itemId">道具ID</param>
        /// <param name="count">购买数量</param>
        /// <returns>购买结果</returns>
        public static ShopBuyResult BuyItem(UserEntity player, string shopId, string itemId, int count = 1)
        {
            // 参数验证
            if (player == null)
            {
                return new ShopBuyResult { Success = false, Message = "玩家对象为空" };
            }

            if (count <= 0)
            {
                return new ShopBuyResult { Success = false, Message = "购买数量必须大于0" };
            }

            // 获取商店
            var shop = GetShop(shopId);
            if (shop == null)
            {
                return new ShopBuyResult { Success = false, Message = "商店不存在" };
            }

            // 检查商店是否可访问
            if (!shop.IsAccessible(player))
            {
                return new ShopBuyResult { Success = false, Message = "商店未开放或等级不足" };
            }

            // 查找商品
            var shopItem = shop.Items.FirstOrDefault(i => i.ItemId == itemId && i.ItemType == ShopItemType.Item);
            if (shopItem == null)
            {
                return new ShopBuyResult { Success = false, Message = "商品不存在" };
            }

            // 检查是否可购买
            if (!shopItem.CanPurchase)
            {
                return new ShopBuyResult { Success = false, Message = "商品已售罄或达到限购" };
            }

            // 检查库存
            if (shopItem.Stock != -1 && shopItem.Stock < count)
            {
                return new ShopBuyResult { Success = false, Message = $"库存不足，剩余{shopItem.Stock}" };
            }

            // 检查每日限购
            int todayPurchased = GetPlayerTodayPurchased(player.GID, itemId);
            int remainingDaily = shopItem.DailyLimit - todayPurchased;
            if (shopItem.DailyLimit != -1 && remainingDaily < count)
            {
                return new ShopBuyResult { Success = false, Message = $"今日限购剩余{remainingDaily}" };
            }

            // 计算价格
            int vipLevel = 0; // TODO: 从玩家获取VIP等级
            int totalPrice = shopItem.GetBuyPrice(shop.Discount, vipLevel) * count;

            // 检查金币
            if (!PlayerManager.HasEnoughGold(player, totalPrice))
            {
                return new ShopBuyResult { Success = false, Message = $"金币不足，需要{totalPrice}" };
            }

            // 检查背包空间
            // TODO: 需要InventoryManager检查背包空间
            // if (!player.InventoryManager.CanAddItem(itemId, count))
            // {
            //     return new ShopBuyResult { Success = false, Message = "背包空间不足" };
            // }

            // 扣除金币
            if (!PlayerManager.ConsumeGold(player, totalPrice, "购买道具"))
            {
                return new ShopBuyResult { Success = false, Message = "金币扣除失败" };
            }

            // 添加道具到背包
            // TODO: player.InventoryManager.AddItem(itemId, count);

            // 更新库存
            if (shopItem.Stock != -1)
            {
                shopItem.Stock -= count;
            }

            // 更新每日购买记录
            UpdatePlayerTodayPurchased(player.GID, itemId, count);

            return new ShopBuyResult
            {
                Success = true,
                Message = $"购买成功！消耗{totalPrice}金币",
                TotalCost = totalPrice,
                ItemId = itemId,
                Count = count
            };
        }

        /// <summary>
        /// 购买装备
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="shopId">商店ID</param>
        /// <param name="equipmentTemplateId">装备模板ID</param>
        /// <returns>购买结果</returns>
        public static ShopBuyResult BuyEquipment(UserEntity player, string shopId, int equipmentTemplateId)
        {
            // 参数验证
            if (player == null)
            {
                return new ShopBuyResult { Success = false, Message = "玩家对象为空" };
            }

            // 获取商店
            var shop = GetShop(shopId);
            if (shop == null)
            {
                return new ShopBuyResult { Success = false, Message = "商店不存在" };
            }

            // 检查商店是否可访问
            if (!shop.IsAccessible(player))
            {
                return new ShopBuyResult { Success = false, Message = "商店未开放或等级不足" };
            }

            // 查找商品
            string equipmentId = equipmentTemplateId.ToString();
            var shopItem = shop.Items.FirstOrDefault(i => i.ItemId == equipmentId && i.ItemType == ShopItemType.Equipment);
            if (shopItem == null)
            {
                return new ShopBuyResult { Success = false, Message = "装备不存在" };
            }

            // 检查是否可购买
            if (!shopItem.CanPurchase)
            {
                return new ShopBuyResult { Success = false, Message = "装备已售罄或达到限购" };
            }

            // 检查库存
            if (shopItem.Stock == 0)
            {
                return new ShopBuyResult { Success = false, Message = "装备已售罄" };
            }

            // 计算价格
            int vipLevel = 0; // TODO: 从玩家获取VIP等级
            int totalPrice = shopItem.GetBuyPrice(shop.Discount, vipLevel);

            // 检查金币
            if (!PlayerManager.HasEnoughGold(player, totalPrice))
            {
                return new ShopBuyResult { Success = false, Message = $"金币不足，需要{totalPrice}" };
            }

            // 检查背包空间
            // TODO: 需要InventoryManager检查背包空间

            // 扣除金币
            if (!PlayerManager.ConsumeGold(player, totalPrice, "购买装备"))
            {
                return new ShopBuyResult { Success = false, Message = "金币扣除失败" };
            }

            // 创建装备实例
            var template = GameData.EquipmentTemplates.ContainsKey(equipmentTemplateId)
                ? GameData.EquipmentTemplates[equipmentTemplateId]
                : null;

            if (template == null)
            {
                return new ShopBuyResult { Success = false, Message = "装备模板不存在" };
            }

            var equipment = new EquipmentInstance
            {
                InstanceId = Guid.NewGuid().ToString(),
                Template = template,
                EnhanceLevel = 0,
                RerolledAttrs = [],
                RerollCount = 0
            };

            // 添加装备到背包
            // TODO: player.InventoryManager.AddEquipment(equipment);

            // 更新库存
            if (shopItem.Stock != -1)
            {
                shopItem.Stock--;
            }

            // 更新每日购买记录
            UpdatePlayerTodayPurchased(player.GID, equipmentId, 1);

            return new ShopBuyResult
            {
                Success = true,
                Message = $"购买成功！消耗{totalPrice}金币",
                TotalCost = totalPrice,
                Equipment = equipment
            };
        }

        #endregion

        #region 出售功能

        /// <summary>
        /// 出售道具
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="itemId">道具ID</param>
        /// <param name="count">出售数量</param>
        /// <returns>出售结果</returns>
        public static ShopSellResult SellItem(UserEntity player, string itemId, int count = 1)
        {
            // 参数验证
            if (player == null)
            {
                return new ShopSellResult { Success = false, Message = "玩家对象为空" };
            }

            if (count <= 0)
            {
                return new ShopSellResult { Success = false, Message = "出售数量必须大于0" };
            }

            // 检查道具是否存在
            if (!GameData.Items.ContainsKey(itemId))
            {
                return new ShopSellResult { Success = false, Message = "道具不存在" };
            }

            // 检查背包是否有足够的道具
            // TODO: 需要InventoryManager检查
            // if (!player.InventoryManager.HasItem(itemId, count))
            // {
            //     return new ShopSellResult { Success = false, Message = "背包道具不足" };
            // }

            // 获取道具基础价格
            var item = GameData.Items[itemId];
            // 根据品质计算基础价格：品质 * 10
            int basePrice = item.Quality * 10;

            // 计算出售价格（原价的50%）
            int totalGold = (int)(basePrice * 0.5f) * count;

            // 移除道具
            // TODO: player.InventoryManager.RemoveItem(itemId, count);

            // 添加金币
            PlayerManager.AddGold(player, totalGold, "出售道具");

            return new ShopSellResult
            {
                Success = true,
                Message = $"出售成功！获得{totalGold}金币",
                GoldEarned = totalGold,
                ItemId = itemId,
                Count = count
            };
        }

        /// <summary>
        /// 出售装备
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="equipment">装备实例</param>
        /// <returns>出售结果</returns>
        public static ShopSellResult SellEquipment(UserEntity player, EquipmentInstance equipment)
        {
            if (player == null)
            {
                return new ShopSellResult { Success = false, Message = "玩家对象为空" };
            }

            if (equipment == null || equipment.Template == null)
            {
                return new ShopSellResult { Success = false, Message = "装备不存在" };
            }

            var template = equipment.Template;

            int basePrice = GetEquipmentBasePrice(template);
            float qualityBonus = template.QualityMultiplier;
            float enhanceBonus = 1.0f + equipment.EnhanceLevel * 0.1f;
            int totalGold = (int)(basePrice * qualityBonus * enhanceBonus * 0.5f);

            PlayerManager.AddGold(player, totalGold, "出售装备");

            return new ShopSellResult
            {
                Success = true,
                Message = $"出售成功！获得{totalGold}金币",
                GoldEarned = totalGold,
                InstanceId = equipment.InstanceId
            };
        }

        /// <summary>
        /// 获取装备基础价格
        /// </summary>
        /// <param name="template">装备模板</param>
        /// <returns>基础价格</returns>
        private static int GetEquipmentBasePrice(EquipmentTemplate template)
        {
            int levelPrice = template.Level * 50;
            int qualityPrice = (int)template.Quality * 100;
            return levelPrice + qualityPrice;
        }

        #endregion

        #region 刷新功能

        /// <summary>
        /// 刷新指定商店
        /// </summary>
        /// <param name="shopId">商店ID</param>
        /// <returns>是否成功</returns>
        public static bool RefreshShop(string shopId)
        {
            var shop = GetShop(shopId);
            if (shop == null)
            {
                return false;
            }

            shop.Refresh();
            return true;
        }

        /// <summary>
        /// 刷新所有需要刷新的商店
        /// </summary>
        public static void RefreshAllShops()
        {
            foreach (var shop in shops.Values)
            {
                if (shop.NeedsRefresh())
                {
                    shop.Refresh();
                }
            }
        }

        #endregion

        #region 每日记录管理

        /// <summary>
        /// 检查并重置每日记录
        /// </summary>
        private static void CheckDailyReset()
        {
            if (DateTime.Today > lastDailyReset)
            {
                // 新的一天，重置所有玩家的每日购买记录
                playerDailyRecords.Clear();
                lastDailyReset = DateTime.Today;

                // 重置所有商品的每日购买计数
                foreach (var shop in shops.Values)
                {
                    foreach (var item in shop.Items)
                    {
                        item.ResetDailyPurchase();
                    }
                }
            }
        }

        /// <summary>
        /// 获取玩家今日已购买数量
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="itemId">商品ID</param>
        /// <returns>今日已购买数量</returns>
        private static int GetPlayerTodayPurchased(string playerId, string itemId)
        {
            CheckDailyReset();

            if (!playerDailyRecords.ContainsKey(playerId))
            {
                return 0;
            }

            if (!playerDailyRecords[playerId].ContainsKey(itemId))
            {
                return 0;
            }

            return playerDailyRecords[playerId][itemId];
        }

        /// <summary>
        /// 更新玩家今日购买数量
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="itemId">商品ID</param>
        /// <param name="count">购买数量</param>
        private static void UpdatePlayerTodayPurchased(string playerId, string itemId, int count)
        {
            CheckDailyReset();

            if (!playerDailyRecords.ContainsKey(playerId))
            {
                playerDailyRecords[playerId] = [];
            }

            if (!playerDailyRecords[playerId].ContainsKey(itemId))
            {
                playerDailyRecords[playerId][itemId] = 0;
            }

            playerDailyRecords[playerId][itemId] += count;
        }

        #endregion
    }

    #region 结果类

    /// <summary>
    /// 购买结果类
    /// </summary>
    public class ShopBuyResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 总花费
        /// </summary>
        public long TotalCost { get; set; }

        /// <summary>
        /// 购买的道具ID
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 购买数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 购买的装备（仅购买装备时有值）
        /// </summary>
        public EquipmentInstance? Equipment { get; set; }
    }

    /// <summary>
    /// 出售结果类
    /// </summary>
    public class ShopSellResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获得的金币
        /// </summary>
        public int GoldEarned { get; set; }

        /// <summary>
        /// 出售的道具ID
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 出售数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 出售的装备实例ID
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;
    }

    #endregion
}
