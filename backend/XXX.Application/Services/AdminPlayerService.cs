using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台玩家服务。
    /// 除了基础资料维护，还补充玩家战斗与养成运行态。
    /// </summary>
    public class AdminPlayerService : IAdminPlayerService
    {
        private const int DefaultSpiritFieldLevel = 1;
        private const int DefaultSpiritFieldUnlockedPlots = 3;
        private const int DefaultSpiritFieldMaxPlots = 9;

        private readonly IRepository<UserEntity> _userRepository;
        private readonly IRepository<AlchemySystemEntity> _alchemyRepository;
        private readonly IRepository<ForgeSystemEntity> _forgeRepository;
        private readonly IRepository<FiveElementArrayEntity> _fiveElementRepository;
        private readonly IRepository<SpiritFieldSystemEntity> _spiritFieldRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IBattleService _battleService;

        /// <summary>
        /// 初始化玩家服务。
        /// </summary>
        public AdminPlayerService(
            IRepository<UserEntity> userRepository,
            IRepository<AlchemySystemEntity> alchemyRepository,
            IRepository<ForgeSystemEntity> forgeRepository,
            IRepository<FiveElementArrayEntity> fiveElementRepository,
            IRepository<SpiritFieldSystemEntity> spiritFieldRepository,
            IInventoryService inventoryService,
            IBattleService battleService)
        {
            _userRepository = userRepository;
            _alchemyRepository = alchemyRepository;
            _forgeRepository = forgeRepository;
            _fiveElementRepository = fiveElementRepository;
            _spiritFieldRepository = spiritFieldRepository;
            _inventoryService = inventoryService;
            _battleService = battleService;
        }

        /// <summary>
        /// 获取玩家列表。
        /// </summary>
        public async Task<List<AdminPlayerListItemDto>> GetListAsync(string? keyword = null, bool? isOfflineBattling = null)
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

            query = query.WhereIF(isOfflineBattling == true, user => user.BattleMode == BattleMode.OfflineAuto);
            query = query.WhereIF(isOfflineBattling == false, user => user.BattleMode != BattleMode.OfflineAuto);

            var users = await query
                .OrderByDescending(user => user.Level)
                .ToListAsync();

            if (users.Count == 0)
            {
                return [];
            }

            var playerIds = users.Select(user => user.GID).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var alchemyMap = await LoadAlchemyMapAsync(playerIds);
            var forgeMap = await LoadForgeMapAsync(playerIds);
            var fiveElementMap = await LoadFiveElementMapAsync(playerIds);
            var spiritFieldMap = await LoadSpiritFieldMapAsync(playerIds);

            return users.Select(user => MapListItem(
                user,
                alchemyMap.TryGetValue(user.GID, out var alchemy) ? alchemy : null,
                forgeMap.TryGetValue(user.GID, out var forge) ? forge : null,
                fiveElementMap.TryGetValue(user.GID, out var fiveElement) ? fiveElement : null,
                spiritFieldMap.TryGetValue(user.GID, out var spiritField) ? spiritField : null))
                .ToList();
        }

        /// <summary>
        /// 获取玩家详情。
        /// </summary>
        public async Task<AdminPlayerDetailDto?> GetDetailAsync(string playerId)
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

            var alchemy = await _alchemyRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);
            var forge = await _forgeRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);
            var fiveElement = await _fiveElementRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);
            var spiritField = await _spiritFieldRepository.GetFirstAsync(entity => entity.PlayerId == normalizedPlayerId);
            return MapDetail(user, alchemy, forge, fiveElement, spiritField);
        }

        /// <summary>
        /// 获取当前离线挂机玩家列表。
        /// </summary>
        public async Task<List<AdminOfflineBattleListItemDto>> GetOfflineBattlesAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _userRepository.Db.Queryable<UserEntity>()
                .Where(user => !user.IsDeleted && user.BattleMode == BattleMode.OfflineAuto);

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(user =>
                    user.GID.Contains(normalizedKeyword) ||
                    user.Name.Contains(normalizedKeyword) ||
                    user.Account.Contains(normalizedKeyword) ||
                    (user.OfflineBattleMapId ?? string.Empty).Contains(normalizedKeyword));
            }

            var users = await query
                .OrderBy(user => user.OfflineBattleStartedAtUtc)
                .OrderBy(user => user.Name)
                .ToListAsync();

            return users.Select(user =>
            {
                var summary = DeserializeOfflineBattleSummary(user);
                return new AdminOfflineBattleListItemDto
                {
                    PlayerId = user.GID,
                    Name = user.Name,
                    Account = user.Account,
                    MapId = string.IsNullOrWhiteSpace(user.OfflineBattleMapId) ? summary.MapId : user.OfflineBattleMapId ?? string.Empty,
                    MapName = ResolveMapName(user.OfflineBattleMapId ?? summary.MapId),
                    StartedAtUtc = user.OfflineBattleStartedAtUtc ?? summary.StartedAtUtc,
                    LastTickAtUtc = user.OfflineBattleLastTickAtUtc,
                    BattleCooldownUntilUtc = user.BattleCooldownUntilUtc,
                    TotalBattles = summary.TotalBattles,
                    WinBattles = summary.WinBattles,
                    ExpGained = summary.ExpGained,
                    GoldGained = summary.GoldGained
                };
            }).ToList();
        }

        /// <summary>
        /// 保存玩家基础信息。
        /// </summary>
        public async Task<AdminPlayerDetailDto> SaveAsync(AdminPlayerDetailDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PlayerId))
            {
                throw new InvalidOperationException("玩家编号不能为空。");
            }

            var user = await _userRepository.GetByIdAsync(request.PlayerId.Trim());
            if (user == null || user.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在。");
            }

            user.Name = (request.Name ?? string.Empty).Trim();
            user.CurrentTitle = string.IsNullOrWhiteSpace(request.CurrentTitle) ? null : request.CurrentTitle.Trim();
            user.Level = Math.Max(1, request.Level);
            user.Exp = Math.Max(0, request.Exp);
            user.XExp = Math.Max(1, request.XExp);
            user.Gold = Math.Max(0, request.Gold);
            user.Honor = Math.Max(0, request.Honor);
            user.GuildContribution = Math.Max(0, request.GuildContribution);
            user.SpiritStone = Math.Max(0, request.SpiritStone);
            user.Profession = PlayerProfessionCatalog.Normalize(request.Profession, PlayerProfessionCatalog.Normalize(user.Profession));
            user.LastUpdateTime = DateTime.Now;

            await _userRepository.UpdateAsync(user);
            return await GetDetailAsync(user.GID) ?? throw new InvalidOperationException("玩家不存在。");
        }

        /// <summary>
        /// 发放货币或经验。
        /// </summary>
        public async Task<AdminPlayerDetailDto> GrantCurrencyAsync(string playerId, AdminGrantCurrencyRequestDto request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("货币发放请求不能为空。");
            }

            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在。");
            }

            var resourceType = (request.ResourceType ?? string.Empty).Trim();
            var amount = request.Amount;
            if (string.IsNullOrWhiteSpace(resourceType) || amount <= 0)
            {
                throw new InvalidOperationException("资源类型和发放数量必须有效。");
            }

            switch (resourceType.ToLowerInvariant())
            {
                case "gold":
                    user.Gold += amount;
                    break;
                case "spiritstone":
                    user.SpiritStone += amount;
                    break;
                case "honor":
                    user.Honor += checked((int)amount);
                    break;
                case "guildcontribution":
                    user.GuildContribution += checked((int)amount);
                    break;
                case "exp":
                    user.Exp += amount;
                    break;
                default:
                    throw new InvalidOperationException("不支持的资源类型。");
            }

            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            return await GetDetailAsync(user.GID) ?? throw new InvalidOperationException("玩家不存在。");
        }

        /// <summary>
        /// 发放玩家道具。
        /// </summary>
        public async Task<InventoryItemDto> GrantItemAsync(string playerId, AdminGrantItemRequestDto request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("道具发放请求不能为空。");
            }

            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new InvalidOperationException("玩家编号不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.ItemId) || request.Quantity <= 0)
            {
                throw new InvalidOperationException("道具编号和发放数量必须有效。");
            }

            return await _inventoryService.AddItemAsync(playerId, new AddItemRequestDto
            {
                ItemId = request.ItemId.Trim(),
                Quantity = request.Quantity,
                Source = string.IsNullOrWhiteSpace(request.Reason) ? "AdminGrant" : request.Reason.Trim()
            });
        }

        /// <summary>
        /// 封禁玩家。
        /// </summary>
        public async Task<AdminPlayerDetailDto> BanAsync(string playerId, AdminBanPlayerRequestDto request)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在。");
            }

            user.IsBanned = true;
            user.BanReason = string.IsNullOrWhiteSpace(request.Reason) ? "后台封禁" : request.Reason.Trim();
            user.BanExpiresAt = request.Hours <= 0 ? null : DateTime.Now.AddHours(request.Hours);
            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            return await GetDetailAsync(user.GID) ?? throw new InvalidOperationException("玩家不存在。");
        }

        /// <summary>
        /// 解封玩家。
        /// </summary>
        public async Task<AdminPlayerDetailDto> UnbanAsync(string playerId)
        {
            var user = await _userRepository.GetByIdAsync(playerId);
            if (user == null || user.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在。");
            }

            user.IsBanned = false;
            user.BanReason = null;
            user.BanExpiresAt = null;
            user.LastUpdateTime = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            return await GetDetailAsync(user.GID) ?? throw new InvalidOperationException("玩家不存在。");
        }

        /// <summary>
        /// 后台强制停止某个玩家的离线挂机。
        /// </summary>
        public async Task<AdminOfflineBattleSummaryDto> StopOfflineBattleAsync(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new InvalidOperationException("玩家编号不能为空。");
            }

            var user = await _userRepository.GetByIdAsync(playerId.Trim());
            if (user == null || user.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在。");
            }

            if (user.BattleMode != BattleMode.OfflineAuto)
            {
                throw new InvalidOperationException("该玩家当前不在离线挂机中。");
            }

            var summary = await _battleService.StopOfflineBattleAsync(user.GID);
            return new AdminOfflineBattleSummaryDto
            {
                PlayerId = user.GID,
                Name = user.Name,
                Account = user.Account,
                MapId = summary.MapId,
                MapName = summary.MapName,
                StartedAtUtc = summary.StartedAtUtc,
                StoppedAtUtc = summary.StoppedAtUtc,
                DurationSeconds = summary.DurationSeconds,
                TotalBattles = summary.TotalBattles,
                WinBattles = summary.WinBattles,
                ExpGained = summary.ExpGained,
                GoldGained = summary.GoldGained,
                ItemDrops = summary.ItemDrops,
                EquipmentDrops = summary.EquipmentDrops
            };
        }

        private async Task<Dictionary<string, AlchemySystemEntity>> LoadAlchemyMapAsync(IReadOnlyCollection<string> playerIds)
        {
            var items = await _alchemyRepository.Db.Queryable<AlchemySystemEntity>()
                .Where(entity => playerIds.Contains(entity.PlayerId))
                .ToListAsync();
            return items.ToDictionary(item => item.PlayerId, item => item, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<Dictionary<string, ForgeSystemEntity>> LoadForgeMapAsync(IReadOnlyCollection<string> playerIds)
        {
            var items = await _forgeRepository.Db.Queryable<ForgeSystemEntity>()
                .Where(entity => playerIds.Contains(entity.PlayerId))
                .ToListAsync();
            return items.ToDictionary(item => item.PlayerId, item => item, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<Dictionary<string, FiveElementArrayEntity>> LoadFiveElementMapAsync(IReadOnlyCollection<string> playerIds)
        {
            var items = await _fiveElementRepository.Db.Queryable<FiveElementArrayEntity>()
                .Where(entity => playerIds.Contains(entity.PlayerId))
                .ToListAsync();
            return items.ToDictionary(item => item.PlayerId, item => item, StringComparer.OrdinalIgnoreCase);
        }

        private async Task<Dictionary<string, SpiritFieldSystemEntity>> LoadSpiritFieldMapAsync(IReadOnlyCollection<string> playerIds)
        {
            var items = await _spiritFieldRepository.Db.Queryable<SpiritFieldSystemEntity>()
                .Where(entity => playerIds.Contains(entity.PlayerId))
                .ToListAsync();
            return items.ToDictionary(item => item.PlayerId, item => item, StringComparer.OrdinalIgnoreCase);
        }

        private static AdminPlayerListItemDto MapListItem(
            UserEntity user,
            AlchemySystemEntity? alchemy,
            ForgeSystemEntity? forge,
            FiveElementArrayEntity? fiveElement,
            SpiritFieldSystemEntity? spiritField)
        {
            var summary = DeserializeOfflineBattleSummary(user);
            return new AdminPlayerListItemDto
            {
                PlayerId = user.GID,
                Name = user.Name,
                Account = user.Account,
                Level = user.Level,
                Profession = PlayerProfessionCatalog.Normalize(user.Profession),
                ProfessionName = PlayerProfessionCatalog.GetDisplayName(user.Profession),
                Gold = user.Gold,
                BattleMode = user.BattleMode.ToString(),
                IsOfflineBattling = user.BattleMode == BattleMode.OfflineAuto,
                OfflineBattleMapName = user.BattleMode == BattleMode.OfflineAuto
                    ? ResolveMapName(user.OfflineBattleMapId ?? summary.MapId)
                    : null,
                AlchemistLevel = Math.Max(1, alchemy?.AlchemistLevel ?? 1),
                BlacksmithLevel = Math.Max(1, forge?.BlacksmithLevel ?? 1),
                ArrayLevel = NormalizeArrayLevel(fiveElement?.ArrayLevel ?? 1),
                SpiritFieldLevel = Math.Max(DefaultSpiritFieldLevel, spiritField?.FieldLevel ?? DefaultSpiritFieldLevel)
            };
        }

        private static AdminPlayerDetailDto MapDetail(
            UserEntity user,
            AlchemySystemEntity? alchemy,
            ForgeSystemEntity? forge,
            FiveElementArrayEntity? fiveElement,
            SpiritFieldSystemEntity? spiritField)
        {
            var summary = DeserializeOfflineBattleSummary(user);
            var arrayLevel = NormalizeArrayLevel(fiveElement?.ArrayLevel ?? 1);
            var professionLevelCap = FiveElementProgressionRules.GetProfessionLevelCap(arrayLevel);
            var alchemyIsCrafting = !string.IsNullOrWhiteSpace(alchemy?.ActiveRecipeId);
            var forgeIsForging = !string.IsNullOrWhiteSpace(forge?.ActiveRecipeId);

            return new AdminPlayerDetailDto
            {
                PlayerId = user.GID,
                Name = user.Name,
                Account = user.Account,
                CurrentTitle = user.CurrentTitle,
                Level = user.Level,
                Profession = PlayerProfessionCatalog.Normalize(user.Profession),
                ProfessionName = PlayerProfessionCatalog.GetDisplayName(user.Profession),
                Exp = user.Exp,
                XExp = user.XExp,
                Gold = user.Gold,
                Honor = user.Honor,
                GuildContribution = user.GuildContribution,
                SpiritStone = user.SpiritStone,
                IsBanned = user.IsBanned,
                BanReason = user.BanReason,
                BanExpiresAt = user.BanExpiresAt,
                BattleCooldownUntilUtc = user.BattleCooldownUntilUtc,
                BattleMode = user.BattleMode.ToString(),
                IsOfflineBattling = user.BattleMode == BattleMode.OfflineAuto,
                OfflineBattleMapId = user.OfflineBattleMapId,
                OfflineBattleMapName = ResolveMapName(user.OfflineBattleMapId ?? summary.MapId),
                OfflineBattleStartedAtUtc = user.OfflineBattleStartedAtUtc ?? summary.StartedAtUtc,
                OfflineBattleLastTickAtUtc = user.OfflineBattleLastTickAtUtc,
                OfflineBattleTotalBattles = summary.TotalBattles,
                OfflineBattleWinBattles = summary.WinBattles,
                OfflineBattleExpGained = summary.ExpGained,
                OfflineBattleGoldGained = summary.GoldGained,
                ArrayLevel = arrayLevel,
                MetalLevel = NormalizeElementLevel(fiveElement?.MetalLevel ?? 1, arrayLevel),
                WoodLevel = NormalizeElementLevel(fiveElement?.WoodLevel ?? 1, arrayLevel),
                WaterLevel = NormalizeElementLevel(fiveElement?.WaterLevel ?? 1, arrayLevel),
                FireLevel = NormalizeElementLevel(fiveElement?.FireLevel ?? 1, arrayLevel),
                EarthLevel = NormalizeElementLevel(fiveElement?.EarthLevel ?? 1, arrayLevel),
                ProfessionLevelCap = professionLevelCap,
                AlchemistLevel = Math.Min(professionLevelCap, Math.Max(1, alchemy?.AlchemistLevel ?? 1)),
                AlchemistExp = Math.Max(0, alchemy?.AlchemistExp ?? 0),
                IsAlchemyCrafting = alchemyIsCrafting,
                ActiveAlchemyRecipeId = alchemy?.ActiveRecipeId,
                ActiveAlchemyCompleteAt = alchemy?.ActiveCraftCompleteAt,
                CanCollectAlchemy = alchemyIsCrafting &&
                    alchemy?.ActiveCraftCompleteAt.HasValue == true &&
                    alchemy.ActiveCraftCompleteAt <= DateTime.Now,
                BlacksmithLevel = Math.Min(professionLevelCap, Math.Max(1, forge?.BlacksmithLevel ?? 1)),
                BlacksmithExp = Math.Max(0, forge?.BlacksmithExp ?? 0),
                IsForging = forgeIsForging,
                ActiveForgeRecipeId = forge?.ActiveRecipeId,
                ActiveForgeCompleteAt = forge?.ActiveForgeCompleteAt,
                CanCollectForge = forgeIsForging &&
                    forge?.ActiveForgeCompleteAt.HasValue == true &&
                    forge.ActiveForgeCompleteAt <= DateTime.Now,
                SpiritFieldLevel = Math.Max(DefaultSpiritFieldLevel, spiritField?.FieldLevel ?? DefaultSpiritFieldLevel),
                SpiritFieldUnlockedPlots = Math.Max(DefaultSpiritFieldUnlockedPlots, spiritField?.UnlockedPlots ?? DefaultSpiritFieldUnlockedPlots),
                SpiritFieldMaxPlots = Math.Max(DefaultSpiritFieldMaxPlots, spiritField?.MaxPlots ?? DefaultSpiritFieldMaxPlots),
                SpiritFieldGlobalYieldBonus = Math.Max(0, spiritField?.GlobalYieldBonus ?? FiveElementProgressionRules.GetSpiritFieldYieldBonusPercent(arrayLevel))
            };
        }

        private static int NormalizeArrayLevel(int level)
        {
            return Math.Clamp(level <= 0 ? 1 : level, 1, 50);
        }

        private static int NormalizeElementLevel(int level, int arrayLevel)
        {
            return Math.Clamp(level <= 0 ? 1 : level, 1, NormalizeArrayLevel(arrayLevel));
        }

        private static OfflineBattleSummaryDto DeserializeOfflineBattleSummary(UserEntity user)
        {
            if (!string.IsNullOrWhiteSpace(user.OfflineBattleSummaryJson))
            {
                try
                {
                    var summary = JsonSerializer.Deserialize<OfflineBattleSummaryDto>(user.OfflineBattleSummaryJson);
                    if (summary != null)
                    {
                        summary.MapId = string.IsNullOrWhiteSpace(summary.MapId)
                            ? user.OfflineBattleMapId ?? string.Empty
                            : summary.MapId;
                        summary.MapName = string.IsNullOrWhiteSpace(summary.MapName)
                            ? ResolveMapName(user.OfflineBattleMapId ?? summary.MapId)
                            : summary.MapName;
                        summary.StartedAtUtc ??= user.OfflineBattleStartedAtUtc;
                        return summary;
                    }
                }
                catch
                {
                    // 忽略损坏的历史 JSON，返回空汇总。
                }
            }

            return new OfflineBattleSummaryDto
            {
                MapId = user.OfflineBattleMapId ?? string.Empty,
                MapName = ResolveMapName(user.OfflineBattleMapId),
                StartedAtUtc = user.OfflineBattleStartedAtUtc
            };
        }

        private static string ResolveMapName(string? mapId)
        {
            if (!string.IsNullOrWhiteSpace(mapId) &&
                global::XXX.GameData.Maps.TryGetValue(mapId, out var map) &&
                !string.IsNullOrWhiteSpace(map.Name))
            {
                return map.Name;
            }

            return string.Empty;
        }
    }
}
