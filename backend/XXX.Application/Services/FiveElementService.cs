using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 五行聚灵阵服务实现。
    /// </summary>
    public class FiveElementService : IFiveElementService
    {
        private static readonly string[] ElementTypes = ["metal", "wood", "water", "fire", "earth"];
        private const int MaxArrayLevel = 50;

        private readonly DbContext _dbContext;
        private readonly IRepository<FiveElementArrayEntity> _fiveElementRepository;
        private readonly IRepository<ItemTemplateEntity> _itemTemplateRepository;
        private readonly IPlayerAttributeService _playerAttributeService;

        /// <summary>
        /// 初始化五行聚灵阵服务。
        /// </summary>
        public FiveElementService(
            DbContext dbContext,
            IRepository<FiveElementArrayEntity> fiveElementRepository,
            IRepository<ItemTemplateEntity> itemTemplateRepository,
            IPlayerAttributeService playerAttributeService)
        {
            _dbContext = dbContext;
            _fiveElementRepository = fiveElementRepository;
            _itemTemplateRepository = itemTemplateRepository;
            _playerAttributeService = playerAttributeService;
        }

        /// <summary>
        /// 获取玩家五行阵信息。
        /// </summary>
        public async Task<FiveElementDto> GetFiveElementInfoAsync(string playerId)
        {
            var entity = await GetOrCreateSystemAsync(playerId);
            NormalizeStoredLevels(entity);
            RecalculateDerivedValues(entity);
            await _fiveElementRepository.UpdateAsync(entity);
            return await MapToDtoAsync(entity);
        }

        /// <summary>
        /// 升级聚灵阵主等级。
        /// </summary>
        public async Task<bool> UpgradeArrayAsync(string playerId)
        {
            var entity = await GetOrCreateSystemAsync(playerId);
            NormalizeStoredLevels(entity);
            if (entity.ArrayLevel >= MaxArrayLevel)
            {
                return false;
            }

            var cost = FiveElementProgressionRules.BuildArrayUpgradeCost(entity.ArrayLevel);
            var upgraded = await ExecuteUpgradeWithCostAsync(playerId, cost, () =>
            {
                entity.ArrayLevel += 1;
                entity.LastUpdateTime = DateTime.Now;
                RecalculateDerivedValues(entity);
                return _fiveElementRepository.UpdateAsync(entity);
            });

            if (upgraded)
            {
                await _playerAttributeService.RecalculateCombatAttributesAsync(playerId);
            }

            return upgraded;
        }

        /// <summary>
        /// 升级指定五行分支。
        /// </summary>
        public async Task<bool> UpgradeElementAsync(string playerId, UpgradeElementRequestDto request)
        {
            var entity = await GetOrCreateSystemAsync(playerId);
            NormalizeStoredLevels(entity);

            var elementType = NormalizeElementType(request.ElementType);
            var currentLevel = GetElementLevel(entity, elementType);
            if (currentLevel >= FiveElementProgressionRules.GetElementMaxLevel(entity.ArrayLevel))
            {
                return false;
            }

            var cost = FiveElementProgressionRules.BuildElementUpgradeCost(elementType, currentLevel);
            var upgraded = await ExecuteUpgradeWithCostAsync(playerId, cost, () =>
            {
                SetElementLevel(entity, elementType, currentLevel + 1);
                SetElementExp(entity, elementType, GetElementExp(entity, elementType) + cost.Materials.Sum(item => item.Count) + (int)cost.SpiritStoneCost);
                entity.LastUpdateTime = DateTime.Now;
                RecalculateDerivedValues(entity);
                return _fiveElementRepository.UpdateAsync(entity);
            });

            if (upgraded)
            {
                await _playerAttributeService.RecalculateCombatAttributesAsync(playerId);
            }

            return upgraded;
        }

        /// <summary>
        /// 聚灵阵已取消灵气收集玩法。
        /// </summary>
        public Task<CollectSpiritPowerResultDto> CollectSpiritPowerAsync(string playerId)
        {
            return Task.FromResult(new CollectSpiritPowerResultDto
            {
                Success = false,
                CollectedAmount = 0,
                TotalSpiritPower = 0,
                Message = "聚灵阵已改为消耗道具与货币升级，不再收集灵气。"
            });
        }

        /// <summary>
        /// 灵气产出已停用。
        /// </summary>
        public Task<int> CalculateSpiritPowerProductionAsync(string playerId)
        {
            return Task.FromResult(0);
        }

        private async Task<bool> ExecuteUpgradeWithCostAsync(string playerId, FiveElementUpgradeCostDto cost, Func<Task> onSuccess)
        {
            try
            {
                _dbContext.BeginTransaction();

                var userRows = await _dbContext.Db.Updateable<UserEntity>()
                    .SetColumns(user => user.Gold == user.Gold - cost.GoldCost)
                    .SetColumns(user => user.SpiritStone == user.SpiritStone - cost.SpiritStoneCost)
                    .SetColumns(user => user.TotalGoldSpent == user.TotalGoldSpent + cost.GoldCost)
                    .SetColumns(user => user.LastUpdateTime == DateTime.Now)
                    .Where(user => user.GID == playerId &&
                                   !user.IsDeleted &&
                                   user.Gold >= cost.GoldCost &&
                                   user.SpiritStone >= cost.SpiritStoneCost)
                    .ExecuteCommandAsync();

                if (userRows == 0)
                {
                    _dbContext.RollbackTransaction();
                    return false;
                }

                foreach (var material in cost.Materials)
                {
                    var deducted = await DeductInventoryItemAsync(playerId, material.ItemId, material.Count);
                    if (!deducted)
                    {
                        _dbContext.RollbackTransaction();
                        return false;
                    }
                }

                await onSuccess();
                _dbContext.CommitTransaction();
                return true;
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }

        private async Task<bool> DeductInventoryItemAsync(string playerId, string itemId, int quantity)
        {
            if (quantity <= 0)
            {
                return true;
            }

            var items = await _dbContext.Db.Queryable<InventoryItemEntity>()
                .Where(item => item.PlayerId == playerId && item.ItemId == itemId)
                .OrderBy(item => item.IsLocked)
                .OrderBy(item => item.Id)
                .ToListAsync();

            if (items.Sum(item => item.Quantity) < quantity)
            {
                return false;
            }

            var remaining = quantity;
            foreach (var item in items)
            {
                if (remaining <= 0)
                {
                    break;
                }

                var deductCount = Math.Min(item.Quantity, remaining);
                remaining -= deductCount;

                if (deductCount == item.Quantity)
                {
                    var deleteRows = await _dbContext.Db.Deleteable<InventoryItemEntity>()
                        .Where(entity => entity.Id == item.Id && entity.Quantity == item.Quantity)
                        .ExecuteCommandAsync();

                    if (deleteRows == 0)
                    {
                        throw new InvalidOperationException("Inventory item changed while deducting.");
                    }

                    continue;
                }

                var updateRows = await _dbContext.Db.Updateable<InventoryItemEntity>()
                    .SetColumns(entity => entity.Quantity == entity.Quantity - deductCount)
                    .Where(entity => entity.Id == item.Id && entity.Quantity >= deductCount)
                    .ExecuteCommandAsync();

                if (updateRows == 0)
                {
                    throw new InvalidOperationException("Inventory item changed while deducting.");
                }
            }

            return remaining == 0;
        }

        private async Task<FiveElementArrayEntity> GetOrCreateSystemAsync(string playerId)
        {
            var entity = await _fiveElementRepository.GetFirstAsync(f => f.PlayerId == playerId);
            if (entity != null)
            {
                return entity;
            }

            entity = new FiveElementArrayEntity
            {
                PlayerId = playerId,
                ArrayLevel = 1,
                MetalLevel = 1,
                WoodLevel = 1,
                WaterLevel = 1,
                FireLevel = 1,
                EarthLevel = 1,
                SpiritPowerRate = 0,
                LastCollectTime = DateTime.Now,
                DailyResetTime = DateTime.Now.Date,
                LastUpdateTime = DateTime.Now
            };

            entity.ActiveCombinations = BuildActiveCombinations(entity);
            await _fiveElementRepository.AddAsync(entity);
            return entity;
        }

        private static void NormalizeStoredLevels(FiveElementArrayEntity entity)
        {
            entity.ArrayLevel = Math.Clamp(entity.ArrayLevel <= 0 ? 1 : entity.ArrayLevel, 1, MaxArrayLevel);
            var maxElementLevel = FiveElementProgressionRules.GetElementMaxLevel(entity.ArrayLevel);
            entity.MetalLevel = Math.Clamp(entity.MetalLevel <= 0 ? 1 : entity.MetalLevel, 1, maxElementLevel);
            entity.WoodLevel = Math.Clamp(entity.WoodLevel <= 0 ? 1 : entity.WoodLevel, 1, maxElementLevel);
            entity.WaterLevel = Math.Clamp(entity.WaterLevel <= 0 ? 1 : entity.WaterLevel, 1, maxElementLevel);
            entity.FireLevel = Math.Clamp(entity.FireLevel <= 0 ? 1 : entity.FireLevel, 1, maxElementLevel);
            entity.EarthLevel = Math.Clamp(entity.EarthLevel <= 0 ? 1 : entity.EarthLevel, 1, maxElementLevel);
        }

        private static void RecalculateDerivedValues(FiveElementArrayEntity entity)
        {
            entity.SpiritPowerRate = 0;
            entity.TotalSpiritPower = 0;
            entity.ActiveCombinations = BuildActiveCombinations(entity);
        }

        private static List<string> BuildActiveCombinations(FiveElementArrayEntity entity)
        {
            var combinations = new List<string>();

            if (entity.MetalLevel >= 5 &&
                entity.WoodLevel >= 5 &&
                entity.WaterLevel >= 5 &&
                entity.FireLevel >= 5 &&
                entity.EarthLevel >= 5)
            {
                combinations.Add("五行轮转");
            }

            if (entity.MetalLevel == entity.WoodLevel &&
                entity.WoodLevel == entity.WaterLevel &&
                entity.WaterLevel == entity.FireLevel &&
                entity.FireLevel == entity.EarthLevel)
            {
                combinations.Add("五行均衡");
            }

            return combinations;
        }

        private static string NormalizeElementType(string? elementType)
        {
            return (elementType ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "metal" => "metal",
                "wood" => "wood",
                "water" => "water",
                "fire" => "fire",
                "earth" => "earth",
                _ => "metal"
            };
        }

        private static long GetElementExp(FiveElementArrayEntity entity, string elementType)
        {
            return elementType switch
            {
                "metal" => entity.MetalExp,
                "wood" => entity.WoodExp,
                "water" => entity.WaterExp,
                "fire" => entity.FireExp,
                "earth" => entity.EarthExp,
                _ => entity.MetalExp
            };
        }

        private static void SetElementExp(FiveElementArrayEntity entity, string elementType, long exp)
        {
            switch (elementType)
            {
                case "metal":
                    entity.MetalExp = exp;
                    break;
                case "wood":
                    entity.WoodExp = exp;
                    break;
                case "water":
                    entity.WaterExp = exp;
                    break;
                case "fire":
                    entity.FireExp = exp;
                    break;
                case "earth":
                    entity.EarthExp = exp;
                    break;
            }
        }

        private static int GetElementLevel(FiveElementArrayEntity entity, string elementType)
        {
            return elementType switch
            {
                "metal" => entity.MetalLevel,
                "wood" => entity.WoodLevel,
                "water" => entity.WaterLevel,
                "fire" => entity.FireLevel,
                "earth" => entity.EarthLevel,
                _ => entity.MetalLevel
            };
        }

        private static void SetElementLevel(FiveElementArrayEntity entity, string elementType, int level)
        {
            switch (elementType)
            {
                case "metal":
                    entity.MetalLevel = level;
                    break;
                case "wood":
                    entity.WoodLevel = level;
                    break;
                case "water":
                    entity.WaterLevel = level;
                    break;
                case "fire":
                    entity.FireLevel = level;
                    break;
                case "earth":
                    entity.EarthLevel = level;
                    break;
            }
        }

        private static FiveElementUpgradeCostDto BuildArrayUpgradeCost(int currentArrayLevel)
        {
            var nextLevel = Math.Clamp(currentArrayLevel + 1, 1, 50);
            var materialCount = 1 + (nextLevel - 1) / 5;

            return new FiveElementUpgradeCostDto
            {
                ElementType = "array",
                GoldCost = 400 + nextLevel * 180L,
                SpiritStoneCost = 3 + nextLevel * 2L,
                Materials =
                [
                    CreateUpgradeMaterial("ore_t1", "赤砂矿", materialCount),
                    CreateUpgradeMaterial("wood_t1", "青纹木", materialCount),
                    CreateUpgradeMaterial("spirit_water", "灵泉水", materialCount),
                    CreateUpgradeMaterial("crystal_t1", "聚灵晶", materialCount),
                    CreateUpgradeMaterial("hide_t1", "硬皮", materialCount)
                ]
            };
        }

        private static FiveElementUpgradeCostDto BuildElementUpgradeCost(string elementType, int currentLevel)
        {
            var nextLevel = Math.Clamp(currentLevel + 1, 1, 50);
            return new FiveElementUpgradeCostDto
            {
                ElementType = elementType,
                GoldCost = 160 + nextLevel * 120L,
                SpiritStoneCost = 2 + nextLevel,
                Materials =
                [
                    CreateUpgradeMaterial(
                        GetElementMaterialItemId(elementType, nextLevel),
                        GetElementMaterialItemName(elementType, nextLevel),
                        2 + (nextLevel - 1) / 4)
                ]
            };
        }

        private static string GetElementMaterialItemId(string elementType, int nextLevel)
        {
            return elementType switch
            {
                "metal" => GetTieredItemId("ore", nextLevel),
                "wood" => GetTieredItemId("wood", nextLevel),
                "water" => "spirit_water",
                "fire" => GetTieredItemId("crystal", nextLevel),
                "earth" => GetTieredItemId("hide", nextLevel),
                _ => "ore_t1"
            };
        }

        private static string GetElementMaterialItemName(string elementType, int nextLevel)
        {
            return elementType switch
            {
                "metal" => nextLevel >= 25 ? "陨星矿" : nextLevel >= 15 ? "玄铁矿" : "赤砂矿",
                "wood" => nextLevel >= 25 ? "雷击木" : nextLevel >= 15 ? "沉香木" : "青纹木",
                "water" => "灵泉水",
                "fire" => nextLevel >= 25 ? "天辉晶" : nextLevel >= 15 ? "曜灵晶" : "聚灵晶",
                "earth" => nextLevel >= 25 ? "龙鳞革" : nextLevel >= 15 ? "玄纹皮" : "硬皮",
                _ => "赤砂矿"
            };
        }

        private static string GetTieredItemId(string prefix, int nextLevel)
        {
            if (nextLevel >= 25)
            {
                return $"{prefix}_t3";
            }

            if (nextLevel >= 15)
            {
                return $"{prefix}_t2";
            }

            return $"{prefix}_t1";
        }

        private static FiveElementUpgradeMaterialDto CreateUpgradeMaterial(string itemId, string itemName, int count)
        {
            return new FiveElementUpgradeMaterialDto
            {
                ItemId = itemId,
                ItemName = itemName,
                Count = Math.Max(1, count)
            };
        }

        private async Task<FiveElementDto> MapToDtoAsync(FiveElementArrayEntity entity)
        {
            var itemNameResolver = await BuildItemNameResolverAsync();
            return new FiveElementDto
            {
                ArrayLevel = entity.ArrayLevel,
                MetalLevel = entity.MetalLevel,
                WoodLevel = entity.WoodLevel,
                WaterLevel = entity.WaterLevel,
                FireLevel = entity.FireLevel,
                EarthLevel = entity.EarthLevel,
                MetalExp = entity.MetalExp,
                WoodExp = entity.WoodExp,
                WaterExp = entity.WaterExp,
                FireExp = entity.FireExp,
                EarthExp = entity.EarthExp,
                TotalSpiritPower = 0,
                SpiritPowerRate = 0,
                TodayCollectCount = 0,
                TotalCollectedSpiritPower = 0,
                NextArrayUpgradeCost = entity.ArrayLevel >= 50 ? 0 : FiveElementProgressionRules.GetArrayUpgradeCost(entity.ArrayLevel),
                ArrayUpgradeCost = entity.ArrayLevel >= 50
                    ? new FiveElementUpgradeCostDto { ElementType = "array" }
                    : FiveElementProgressionRules.BuildArrayUpgradeCost(entity.ArrayLevel, itemNameResolver),
                ElementUpgradeCosts = ElementTypes
                    .Select(type => FiveElementProgressionRules.BuildElementUpgradeCost(type, GetElementLevel(entity, type), itemNameResolver))
                    .ToList(),
                ElementBonuses = ElementTypes
                    .Select(type =>
                    {
                        var level = GetElementLevel(entity, type);
                        var range = FiveElementProgressionRules.ResolveBranchRange(type, level);
                        return new FiveElementBonusDto
                        {
                            ElementType = type,
                            ElementName = GetElementName(type),
                            Level = level,
                            MaxLevel = FiveElementProgressionRules.GetElementMaxLevel(entity.ArrayLevel),
                            AttributeType = range.AttributeType,
                            AttributeName = FiveElementProgressionRules.GetAttributeName(range.AttributeType),
                            CurrentBonus = FiveElementProgressionRules.GetElementCurrentBonus(type, level),
                            BonusPerLevel = range.BonusPerLevel
                        };
                    })
                    .ToList(),
                SpiritFieldYieldBonusPercent = FiveElementProgressionRules.GetSpiritFieldYieldBonusPercent(entity.ArrayLevel),
                BattleExpBonusPercent = FiveElementProgressionRules.GetBattleExpBonusPercent(entity.ArrayLevel),
                ProfessionLevelCap = FiveElementProgressionRules.GetProfessionLevelCap(entity.ArrayLevel),
                ActiveCombinations = entity.ActiveCombinations
            };
        }

        /// <summary>
        /// 获取五行分支的中文名称，用于返回给游戏端显示。
        /// </summary>
        private static string GetElementName(string elementType)
        {
            return elementType switch
            {
                "metal" => "金",
                "wood" => "木",
                "water" => "水",
                "fire" => "火",
                "earth" => "土",
                _ => elementType
            };
        }

        private async Task<Func<string, string>> BuildItemNameResolverAsync()
        {
            var items = await _itemTemplateRepository.Db.Queryable<ItemTemplateEntity>()
                .Select(item => new ItemTemplateEntity { ItemId = item.ItemId, Name = item.Name })
                .ToListAsync();
            var itemMap = items.ToDictionary(item => item.ItemId, item => item.Name, StringComparer.OrdinalIgnoreCase);
            return itemId => itemMap.TryGetValue(itemId, out var name) ? name : itemId;
        }
    }
}
