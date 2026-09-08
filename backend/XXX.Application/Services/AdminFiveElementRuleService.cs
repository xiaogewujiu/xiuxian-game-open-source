#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminFiveElementRuleService : IAdminFiveElementRuleService
    {
        private readonly IRepository<FiveElementLevelConfigEntity> _levelRepository;
        private readonly IRepository<FiveElementBranchUpgradeConfigEntity> _branchRepository;
        private readonly IRepository<FiveElementBranchRuleRangeEntity> _branchRangeRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminFiveElementRuleService(
            IRepository<FiveElementLevelConfigEntity> levelRepository,
            IRepository<FiveElementBranchUpgradeConfigEntity> branchRepository,
            IRepository<FiveElementBranchRuleRangeEntity> branchRangeRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _levelRepository = levelRepository;
            _branchRepository = branchRepository;
            _branchRangeRepository = branchRangeRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminFiveElementLevelRuleListItemDto>> GetLevelRulesAsync()
        {
            var rules = await _levelRepository.Db.Queryable<FiveElementLevelConfigEntity>()
                .OrderBy(item => item.ArrayLevel)
                .ToListAsync();

            return rules.Select(item => new AdminFiveElementLevelRuleListItemDto
            {
                ArrayLevel = item.ArrayLevel,
                UpgradeGoldCost = item.UpgradeGoldCost,
                UpgradeSpiritStoneCost = item.UpgradeSpiritStoneCost,
                SpiritFieldYieldBonusPercent = item.SpiritFieldYieldBonusPercent,
                BattleExpBonusPercent = item.BattleExpBonusPercent,
                ProfessionLevelCap = item.ProfessionLevelCap,
                IsEnabled = item.IsEnabled,
                IsBuiltIn = item.IsBuiltIn,
                BuiltInVersion = item.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminFiveElementLevelRuleDetailDto?> GetLevelRuleDetailAsync(int arrayLevel)
        {
            if (arrayLevel <= 0)
            {
                return null;
            }

            var entity = await _levelRepository.GetByIdAsync(arrayLevel);
            if (entity == null)
            {
                return null;
            }

            return MapLevelDetail(entity);
        }

        public async Task<AdminFiveElementLevelRuleDetailDto> SaveLevelRuleAsync(AdminFiveElementLevelRuleDetailDto request)
        {
            if (request.ArrayLevel <= 0)
            {
                throw new InvalidOperationException("阵等级必须大于 0。");
            }

            var existing = await _levelRepository.GetByIdAsync(request.ArrayLevel);
            if (existing == null)
            {
                existing = new FiveElementLevelConfigEntity { ArrayLevel = request.ArrayLevel };
                await _levelRepository.AddAsync(ApplyLevelRule(existing, request));
            }
            else
            {
                ApplyLevelRule(existing, request);
                await _levelRepository.UpdateAsync(existing);
            }

            await _runtimeRefreshService.ReloadFiveElementRuleCacheAsync();
            return (await GetLevelRuleDetailAsync(request.ArrayLevel))!;
        }

        public async Task<bool> DeleteLevelRuleAsync(int arrayLevel)
        {
            if (arrayLevel <= 0)
            {
                return false;
            }

            var rows = await _levelRepository.DeleteAsync(arrayLevel);
            if (rows > 0)
            {
                await _runtimeRefreshService.ReloadFiveElementRuleCacheAsync();
            }

            return rows > 0;
        }

        /// <summary>
        /// 获取五行等级区间规则列表。
        /// </summary>
        public async Task<List<AdminFiveElementBranchRuleRangeListItemDto>> GetBranchRuleRangesAsync(string? elementType = null)
        {
            var normalizedElementType = (elementType ?? string.Empty).Trim().ToLowerInvariant();
            var query = _branchRangeRepository.Db.Queryable<FiveElementBranchRuleRangeEntity>();
            if (!string.IsNullOrWhiteSpace(normalizedElementType))
            {
                query = query.Where(item => item.ElementType == normalizedElementType);
            }

            var rules = await query.OrderBy(item => item.ElementType).OrderBy(item => item.MinLevel).ToListAsync();
            return rules.Select(item => new AdminFiveElementBranchRuleRangeListItemDto
            {
                GID = item.GID,
                ElementType = item.ElementType,
                MinLevel = item.MinLevel,
                MaxLevel = item.MaxLevel,
                AttributeType = item.AttributeType,
                BonusPerLevel = item.BonusPerLevel,
                GoldCost = item.GoldCost,
                SpiritStoneCost = item.SpiritStoneCost,
                IsEnabled = item.IsEnabled,
                IsBuiltIn = item.IsBuiltIn,
                BuiltInVersion = item.BuiltInVersion
            }).ToList();
        }

        /// <summary>
        /// 获取指定五行等级区间规则详情。
        /// </summary>
        public async Task<AdminFiveElementBranchRuleRangeDetailDto?> GetBranchRuleRangeDetailAsync(string gid)
        {
            var entity = await _branchRangeRepository.GetByIdAsync((gid ?? string.Empty).Trim());
            return entity == null ? null : MapBranchRangeDetail(entity);
        }

        /// <summary>
        /// 保存五行等级区间规则并校验启用规则完整性。
        /// </summary>
        public async Task<AdminFiveElementBranchRuleRangeDetailDto> SaveBranchRuleRangeAsync(AdminFiveElementBranchRuleRangeDetailDto request)
        {
            var normalizedElementType = NormalizeElementType(request.ElementType);
            var candidate = new FiveElementBranchRuleRangeEntity
            {
                GID = string.IsNullOrWhiteSpace(request.GID) ? Guid.NewGuid().ToString("N") : request.GID.Trim(),
                ElementType = normalizedElementType,
                MinLevel = request.MinLevel,
                MaxLevel = request.MaxLevel,
                AttributeType = request.AttributeType?.Trim() ?? string.Empty,
                BonusPerLevel = request.BonusPerLevel,
                GoldCost = request.GoldCost,
                SpiritStoneCost = request.SpiritStoneCost,
                MaterialsJson = request.MaterialsJson?.Trim() ?? string.Empty,
                SortOrder = request.SortOrder,
                IsEnabled = request.IsEnabled,
                LastUpdateTime = DateTime.Now
            };

            var allRules = await _branchRangeRepository.Db.Queryable<FiveElementBranchRuleRangeEntity>().ToListAsync();
            var existing = allRules.FirstOrDefault(item => item.GID == candidate.GID);
            if (existing != null)
            {
                allRules.Remove(existing);
            }
            allRules.Add(candidate);
            // 即使本次请求要停用或删除某条规则，也必须校验保存后的完整启用集合，
            // 防止数据库先写入不完整配置、运行时刷新时才失败，留下不可用状态。
            FiveElementProgressionRules.ValidateBranchRangeConfigs(allRules);

            if (existing == null)
            {
                await _branchRangeRepository.AddAsync(candidate);
            }
            else
            {
                existing.ElementType = candidate.ElementType;
                existing.MinLevel = candidate.MinLevel;
                existing.MaxLevel = candidate.MaxLevel;
                existing.AttributeType = candidate.AttributeType;
                existing.BonusPerLevel = candidate.BonusPerLevel;
                existing.GoldCost = candidate.GoldCost;
                existing.SpiritStoneCost = candidate.SpiritStoneCost;
                existing.MaterialsJson = candidate.MaterialsJson;
                existing.SortOrder = candidate.SortOrder;
                existing.IsEnabled = candidate.IsEnabled;
                existing.IsBuiltIn = false;
                existing.SeedKey = null;
                existing.BuiltInVersion = null;
                existing.LastUpdateTime = candidate.LastUpdateTime;
                await _branchRangeRepository.UpdateAsync(existing);
                candidate = existing;
            }

            await _runtimeRefreshService.ReloadFiveElementRuleCacheAsync();
            return (await GetBranchRuleRangeDetailAsync(candidate.GID))!;
        }

        /// <summary>
        /// 删除五行等级区间规则。
        /// </summary>
        public async Task<bool> DeleteBranchRuleRangeAsync(string gid)
        {
            var normalizedGid = (gid ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedGid)) return false;
            var allRules = await _branchRangeRepository.Db.Queryable<FiveElementBranchRuleRangeEntity>().ToListAsync();
            var target = allRules.FirstOrDefault(item => item.GID == normalizedGid);
            if (target == null) return false;
            allRules.Remove(target);
            FiveElementProgressionRules.ValidateBranchRangeConfigs(allRules);
            var rows = await _branchRangeRepository.DeleteAsync(normalizedGid);
            if (rows > 0) await _runtimeRefreshService.ReloadFiveElementRuleCacheAsync();
            return rows > 0;
        }

        /// <summary>
        /// 获取五行分支升级规则列表（旧逐级规则兼容接口）。
        /// </summary>
        public async Task<List<AdminFiveElementBranchRuleListItemDto>> GetBranchRulesAsync(string? elementType = null)
        {
            var query = _branchRepository.Db.Queryable<FiveElementBranchUpgradeConfigEntity>();
            var normalizedElementType = (elementType ?? string.Empty).Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(normalizedElementType))
            {
                query = query.Where(item => item.ElementType == normalizedElementType);
            }

            var rules = await query
                .OrderBy(item => item.ElementType)
                .OrderBy(item => item.TargetLevel)
                .ToListAsync();

            return rules.Select(item => new AdminFiveElementBranchRuleListItemDto
            {
                GID = item.GID,
                ElementType = item.ElementType,
                TargetLevel = item.TargetLevel,
                GoldCost = item.GoldCost,
                SpiritStoneCost = item.SpiritStoneCost,
                IsEnabled = item.IsEnabled,
                IsBuiltIn = item.IsBuiltIn,
                BuiltInVersion = item.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminFiveElementBranchRuleDetailDto?> GetBranchRuleDetailAsync(string gid)
        {
            if (string.IsNullOrWhiteSpace(gid))
            {
                return null;
            }

            var entity = await _branchRepository.GetByIdAsync(gid.Trim());
            if (entity == null)
            {
                return null;
            }

            return MapBranchDetail(entity);
        }

        public async Task<AdminFiveElementBranchRuleDetailDto> SaveBranchRuleAsync(AdminFiveElementBranchRuleDetailDto request)
        {
            var normalizedElementType = NormalizeElementType(request.ElementType);
            if (request.TargetLevel <= 0)
            {
                throw new InvalidOperationException("目标等级必须大于 0。");
            }

            FiveElementBranchUpgradeConfigEntity? existing = null;
            if (!string.IsNullOrWhiteSpace(request.GID))
            {
                existing = await _branchRepository.GetByIdAsync(request.GID.Trim());
            }

            if (existing == null)
            {
                existing = await _branchRepository.Db.Queryable<FiveElementBranchUpgradeConfigEntity>()
                    .FirstAsync(item => item.ElementType == normalizedElementType && item.TargetLevel == request.TargetLevel);
            }

            if (existing == null)
            {
                existing = new FiveElementBranchUpgradeConfigEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    ElementType = normalizedElementType,
                    TargetLevel = request.TargetLevel
                };
                await _branchRepository.AddAsync(ApplyBranchRule(existing, request, normalizedElementType));
            }
            else
            {
                ApplyBranchRule(existing, request, normalizedElementType);
                await _branchRepository.UpdateAsync(existing);
            }

            await _runtimeRefreshService.ReloadFiveElementRuleCacheAsync();
            return (await GetBranchRuleDetailAsync(existing.GID))!;
        }

        public async Task<bool> DeleteBranchRuleAsync(string gid)
        {
            var normalizedGid = (gid ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedGid))
            {
                return false;
            }

            var rows = await _branchRepository.DeleteAsync(normalizedGid);
            if (rows > 0)
            {
                await _runtimeRefreshService.ReloadFiveElementRuleCacheAsync();
            }

            return rows > 0;
        }

        private static FiveElementLevelConfigEntity ApplyLevelRule(FiveElementLevelConfigEntity entity, AdminFiveElementLevelRuleDetailDto request)
        {
            entity.UpgradeGoldCost = Math.Max(0, request.UpgradeGoldCost);
            entity.UpgradeSpiritStoneCost = Math.Max(0, request.UpgradeSpiritStoneCost);
            entity.UpgradeMaterialsJson = string.IsNullOrWhiteSpace(request.UpgradeMaterialsJson) ? "[]" : request.UpgradeMaterialsJson.Trim();
            entity.SpiritFieldYieldBonusPercent = Math.Max(0, request.SpiritFieldYieldBonusPercent);
            entity.BattleExpBonusPercent = Math.Max(0, request.BattleExpBonusPercent);
            entity.ProfessionLevelCap = Math.Max(1, request.ProfessionLevelCap);
            entity.IsEnabled = request.IsEnabled;
            entity.IsBuiltIn = false;
            entity.SeedKey = null;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static FiveElementBranchUpgradeConfigEntity ApplyBranchRule(FiveElementBranchUpgradeConfigEntity entity, AdminFiveElementBranchRuleDetailDto request, string normalizedElementType)
        {
            entity.ElementType = normalizedElementType;
            entity.TargetLevel = request.TargetLevel;
            entity.GoldCost = Math.Max(0, request.GoldCost);
            entity.SpiritStoneCost = Math.Max(0, request.SpiritStoneCost);
            entity.MaterialsJson = string.IsNullOrWhiteSpace(request.MaterialsJson) ? "[]" : request.MaterialsJson.Trim();
            entity.SortOrder = request.SortOrder;
            entity.IsEnabled = request.IsEnabled;
            entity.IsBuiltIn = false;
            entity.SeedKey = null;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static AdminFiveElementLevelRuleDetailDto MapLevelDetail(FiveElementLevelConfigEntity entity)
        {
            return new AdminFiveElementLevelRuleDetailDto
            {
                ArrayLevel = entity.ArrayLevel,
                UpgradeGoldCost = entity.UpgradeGoldCost,
                UpgradeSpiritStoneCost = entity.UpgradeSpiritStoneCost,
                UpgradeMaterialsJson = string.IsNullOrWhiteSpace(entity.UpgradeMaterialsJson) ? "[]" : entity.UpgradeMaterialsJson,
                SpiritFieldYieldBonusPercent = entity.SpiritFieldYieldBonusPercent,
                BattleExpBonusPercent = entity.BattleExpBonusPercent,
                ProfessionLevelCap = entity.ProfessionLevelCap,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey,
                BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        private static AdminFiveElementBranchRuleRangeDetailDto MapBranchRangeDetail(FiveElementBranchRuleRangeEntity entity)
        {
            return new AdminFiveElementBranchRuleRangeDetailDto
            {
                GID = entity.GID,
                ElementType = entity.ElementType,
                MinLevel = entity.MinLevel,
                MaxLevel = entity.MaxLevel,
                AttributeType = entity.AttributeType,
                BonusPerLevel = entity.BonusPerLevel,
                GoldCost = entity.GoldCost,
                SpiritStoneCost = entity.SpiritStoneCost,
                MaterialsJson = string.IsNullOrWhiteSpace(entity.MaterialsJson) ? "[]" : entity.MaterialsJson,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey,
                BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        private static AdminFiveElementBranchRuleDetailDto MapBranchDetail(FiveElementBranchUpgradeConfigEntity entity)
        {
            return new AdminFiveElementBranchRuleDetailDto
            {
                GID = entity.GID,
                ElementType = entity.ElementType,
                TargetLevel = entity.TargetLevel,
                GoldCost = entity.GoldCost,
                SpiritStoneCost = entity.SpiritStoneCost,
                MaterialsJson = string.IsNullOrWhiteSpace(entity.MaterialsJson) ? "[]" : entity.MaterialsJson,
                SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled,
                IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey,
                BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
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
                _ => throw new InvalidOperationException("五行类型无效。")
            };
        }
    }
}
#pragma warning restore CS1591
