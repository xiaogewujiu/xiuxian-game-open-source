using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台聚灵阵服务。
    /// </summary>
    public class AdminFiveElementService : IAdminFiveElementService
    {
        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<FiveElementArrayEntity> _fiveElementRepository;
        private readonly IPlayerAttributeService _playerAttributeService;

        /// <summary>
        /// 初始化聚灵阵服务。
        /// </summary>
        public AdminFiveElementService(
            IRepository<UserEntity> userRepository,
            IRepository<FiveElementArrayEntity> fiveElementRepository,
            IPlayerAttributeService playerAttributeService)
        {
            _userRepository = userRepository;
            _fiveElementRepository = fiveElementRepository;
            _playerAttributeService = playerAttributeService;
        }

        /// <summary>
        /// 获取聚灵阵列表。
        /// </summary>
        public async Task<List<AdminFiveElementListItemDto>> GetListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _userRepository.Db.Queryable<UserEntity>()
                .Where(user => !user.IsDeleted);

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(user =>
                    user.GID.Contains(normalizedKeyword) ||
                    user.Name.Contains(normalizedKeyword) ||
                    user.Account.Contains(normalizedKeyword));
            }

            var users = await query.OrderByDescending(user => user.Level).ToListAsync();
            if (users.Count == 0)
            {
                return [];
            }

            var playerIds = users.Select(user => user.GID).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var arrays = await _fiveElementRepository.Db.Queryable<FiveElementArrayEntity>()
                .Where(entity => playerIds.Contains(entity.PlayerId))
                .ToListAsync();
            var arrayMap = arrays.ToDictionary(entity => entity.PlayerId, entity => entity, StringComparer.OrdinalIgnoreCase);

            return users.Select(user => MapListItem(user, arrayMap.TryGetValue(user.GID, out var entity) ? entity : null)).ToList();
        }

        /// <summary>
        /// 获取聚灵阵详情。
        /// </summary>
        public async Task<AdminFiveElementDetailDto?> GetDetailAsync(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return null;
            }

            var normalizedPlayerId = playerId.Trim();
            var user = await _userRepository.GetByIdAsync(normalizedPlayerId);
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            var entity = await _fiveElementRepository.GetFirstAsync(item => item.PlayerId == normalizedPlayerId);
            return MapDetail(user, entity);
        }

        /// <summary>
        /// 保存聚灵阵数据。
        /// </summary>
        public async Task<AdminFiveElementDetailDto> SaveAsync(AdminFiveElementDetailDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PlayerId))
            {
                throw new InvalidOperationException("玩家编号不能为空。");
            }

            var normalizedPlayerId = request.PlayerId.Trim();
            var user = await _userRepository.GetByIdAsync(normalizedPlayerId);
            if (user == null || user.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在。");
            }

            var entity = await _fiveElementRepository.GetFirstAsync(item => item.PlayerId == normalizedPlayerId);
            if (entity == null)
            {
                entity = new FiveElementArrayEntity
                {
                    PlayerId = normalizedPlayerId,
                    LastCollectTime = DateTime.Now,
                    DailyResetTime = DateTime.Now.Date
                };
                ApplyArrayValues(entity, request);
                entity.LastUpdateTime = DateTime.Now;
                await _fiveElementRepository.AddAsync(entity);
            }
            else
            {
                ApplyArrayValues(entity, request);
                entity.LastUpdateTime = DateTime.Now;
                await _fiveElementRepository.UpdateAsync(entity);
            }

            await _playerAttributeService.RecalculateCombatAttributesAsync(normalizedPlayerId);
            return await GetDetailAsync(normalizedPlayerId) ?? throw new InvalidOperationException("玩家不存在。");
        }

        private static void ApplyArrayValues(FiveElementArrayEntity entity, AdminFiveElementDetailDto request)
        {
            entity.ArrayLevel = NormalizeArrayLevel(request.ArrayLevel);
            var maxElementLevel = FiveElementProgressionRules.GetElementMaxLevel(entity.ArrayLevel);
            entity.MetalLevel = NormalizeElementLevel(request.MetalLevel, maxElementLevel);
            entity.WoodLevel = NormalizeElementLevel(request.WoodLevel, maxElementLevel);
            entity.WaterLevel = NormalizeElementLevel(request.WaterLevel, maxElementLevel);
            entity.FireLevel = NormalizeElementLevel(request.FireLevel, maxElementLevel);
            entity.EarthLevel = NormalizeElementLevel(request.EarthLevel, maxElementLevel);
            entity.MetalExp = Math.Max(0, request.MetalExp);
            entity.WoodExp = Math.Max(0, request.WoodExp);
            entity.WaterExp = Math.Max(0, request.WaterExp);
            entity.FireExp = Math.Max(0, request.FireExp);
            entity.EarthExp = Math.Max(0, request.EarthExp);
            entity.ActiveCombinations = (request.ActiveCombinations ?? [])
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static AdminFiveElementListItemDto MapListItem(UserEntity user, FiveElementArrayEntity? entity)
        {
            var arrayLevel = NormalizeArrayLevel(entity?.ArrayLevel ?? 1);
            var maxElementLevel = FiveElementProgressionRules.GetElementMaxLevel(arrayLevel);
            return new AdminFiveElementListItemDto
            {
                PlayerId = user.GID,
                Name = user.Name,
                Account = user.Account,
                ArrayLevel = arrayLevel,
                MetalLevel = NormalizeElementLevel(entity?.MetalLevel ?? 1, maxElementLevel),
                WoodLevel = NormalizeElementLevel(entity?.WoodLevel ?? 1, maxElementLevel),
                WaterLevel = NormalizeElementLevel(entity?.WaterLevel ?? 1, maxElementLevel),
                FireLevel = NormalizeElementLevel(entity?.FireLevel ?? 1, maxElementLevel),
                EarthLevel = NormalizeElementLevel(entity?.EarthLevel ?? 1, maxElementLevel),
                ProfessionLevelCap = FiveElementProgressionRules.GetProfessionLevelCap(arrayLevel),
                SpiritFieldYieldBonusPercent = FiveElementProgressionRules.GetSpiritFieldYieldBonusPercent(arrayLevel),
                BattleExpBonusPercent = FiveElementProgressionRules.GetBattleExpBonusPercent(arrayLevel),
                MaxElementLevel = maxElementLevel,
                ElementBonuses = BuildElementBonuses(entity, maxElementLevel)
            };
        }

        private static AdminFiveElementDetailDto MapDetail(UserEntity user, FiveElementArrayEntity? entity)
        {
            var arrayLevel = NormalizeArrayLevel(entity?.ArrayLevel ?? 1);
            var maxElementLevel = FiveElementProgressionRules.GetElementMaxLevel(arrayLevel);
            return new AdminFiveElementDetailDto
            {
                PlayerId = user.GID,
                Name = user.Name,
                Account = user.Account,
                ArrayLevel = arrayLevel,
                MetalLevel = NormalizeElementLevel(entity?.MetalLevel ?? 1, maxElementLevel),
                WoodLevel = NormalizeElementLevel(entity?.WoodLevel ?? 1, maxElementLevel),
                WaterLevel = NormalizeElementLevel(entity?.WaterLevel ?? 1, maxElementLevel),
                FireLevel = NormalizeElementLevel(entity?.FireLevel ?? 1, maxElementLevel),
                EarthLevel = NormalizeElementLevel(entity?.EarthLevel ?? 1, maxElementLevel),
                MetalExp = Math.Max(0, entity?.MetalExp ?? 0),
                WoodExp = Math.Max(0, entity?.WoodExp ?? 0),
                WaterExp = Math.Max(0, entity?.WaterExp ?? 0),
                FireExp = Math.Max(0, entity?.FireExp ?? 0),
                EarthExp = Math.Max(0, entity?.EarthExp ?? 0),
                ProfessionLevelCap = FiveElementProgressionRules.GetProfessionLevelCap(arrayLevel),
                SpiritFieldYieldBonusPercent = FiveElementProgressionRules.GetSpiritFieldYieldBonusPercent(arrayLevel),
                BattleExpBonusPercent = FiveElementProgressionRules.GetBattleExpBonusPercent(arrayLevel),
                MaxElementLevel = FiveElementProgressionRules.GetElementMaxLevel(arrayLevel),
                ElementBonuses = BuildElementBonuses(entity, maxElementLevel),
                ActiveCombinations = entity?.ActiveCombinations ?? []
            };
        }

        /// <summary>
        /// 根据玩家五行等级构建管理端实际加成明细。
        /// </summary>
        private static List<FiveElementBonusDto> BuildElementBonuses(FiveElementArrayEntity? entity, int maxElementLevel)
        {
            var levels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["metal"] = NormalizeElementLevel(entity?.MetalLevel ?? 1, maxElementLevel),
                ["wood"] = NormalizeElementLevel(entity?.WoodLevel ?? 1, maxElementLevel),
                ["water"] = NormalizeElementLevel(entity?.WaterLevel ?? 1, maxElementLevel),
                ["fire"] = NormalizeElementLevel(entity?.FireLevel ?? 1, maxElementLevel),
                ["earth"] = NormalizeElementLevel(entity?.EarthLevel ?? 1, maxElementLevel)
            };

            return levels.Select(item =>
            {
                var range = FiveElementProgressionRules.ResolveBranchRange(item.Key, item.Value);
                return new FiveElementBonusDto
                {
                    ElementType = item.Key,
                    ElementName = item.Key switch { "metal" => "金", "wood" => "木", "water" => "水", "fire" => "火", "earth" => "土", _ => item.Key },
                    Level = item.Value,
                    MaxLevel = maxElementLevel,
                    AttributeType = range.AttributeType,
                    AttributeName = FiveElementProgressionRules.GetAttributeName(range.AttributeType),
                    CurrentBonus = FiveElementProgressionRules.GetElementCurrentBonus(item.Key, item.Value),
                    BonusPerLevel = range.BonusPerLevel
                };
            }).ToList();
        }

        private static int NormalizeArrayLevel(int level)
        {
            return Math.Clamp(level <= 0 ? 1 : level, 1, 50);
        }

        private static int NormalizeElementLevel(int level, int maxElementLevel)
        {
            return Math.Clamp(level <= 0 ? 1 : level, 1, Math.Clamp(maxElementLevel, 1, 500));
        }
    }
}
