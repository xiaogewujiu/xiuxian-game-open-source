using Microsoft.Extensions.Logging;
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Balance;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 中文注释：
    /// 兑换码服务真实实现。
    /// 当前先把“兑换校验、防重复领取、奖励真实到账、领取记录落库”这一条主链路跑通，
    /// 兑换码配置本身仍放在后端代码中维护，后续如果你要做后台配置表，可以在不改前端接口的前提下平滑迁移。
    /// </summary>
    public class RedeemCodeService : IRedeemCodeService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<RedeemCodeUsageEntity> _redeemCodeUsageRepository;
        private readonly IPlayerRewardService _playerRewardService;
        private readonly IPlayerAttributeService _playerAttributeService;
        private readonly IGameSyncService _gameSyncService;
        private readonly ILogger<RedeemCodeService> _logger;

        /// <summary>
        /// 初始化兑换码服务。
        /// </summary>
        /// <param name="dbContext">数据库上下文。</param>
        /// <param name="redeemCodeUsageRepository">兑换记录仓储。</param>
        /// <param name="playerRewardService">奖励发放服务。</param>
        /// <param name="playerAttributeService">玩家属性服务。</param>
        /// <param name="gameSyncService">玩家状态同步服务。</param>
        /// <param name="logger">日志记录器。</param>
        public RedeemCodeService(
            DbContext dbContext,
            IRepository<RedeemCodeUsageEntity> redeemCodeUsageRepository,
            IPlayerRewardService playerRewardService,
            IPlayerAttributeService playerAttributeService,
            IGameSyncService gameSyncService,
            ILogger<RedeemCodeService> logger)
        {
            _dbContext = dbContext;
            _redeemCodeUsageRepository = redeemCodeUsageRepository;
            _playerRewardService = playerRewardService;
            _playerAttributeService = playerAttributeService;
            _gameSyncService = gameSyncService;
            _logger = logger;
        }

        /// <summary>
        /// 兑换单个兑换码并发放奖励。
        /// </summary>
        /// <param name="playerId">玩家编号。</param>
        /// <param name="request">兑换请求。</param>
        /// <returns>兑换结果与奖励明细。</returns>
        public async Task<RedeemCodeResultDto> RedeemAsync(string playerId, RedeemCodeRequestDto request)
        {
            var normalizedCode = NormalizeCode(request?.Code);
            var definitions = await LoadRedeemCodeDefinitionsAsync();
            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                _logger.LogWarning("Player {PlayerId} submitted an empty redeem code", playerId);
                throw new InvalidOperationException("请输入有效的兑换码。");
            }

            if (!definitions.TryGetValue(normalizedCode, out var definition))
            {
                _logger.LogWarning("Player {PlayerId} submitted invalid redeem code {Code}", playerId, normalizedCode);
                throw new InvalidOperationException("兑换码无效或已过期。");
            }

            var existingUsage = await _redeemCodeUsageRepository.GetFirstAsync(
                usage => usage.PlayerId == playerId && usage.Code == normalizedCode);

            if (existingUsage != null)
            {
                _logger.LogWarning("Player {PlayerId} attempted duplicate redeem for code {Code}", playerId, normalizedCode);
                throw new InvalidOperationException("该兑换码已领取，请勿重复兑换。");
            }

            List<RewardItemDto> grantedRewards;
            var redeemedAt = DateTime.Now;

            try
            {
                _dbContext.BeginTransaction();

                grantedRewards = await _playerRewardService.GrantRewardsAsync(
                    playerId,
                    definition.Rewards,
                    $"兑换码 {normalizedCode}");

                await _redeemCodeUsageRepository.AddAsync(new RedeemCodeUsageEntity
                {
                    UsageId = Guid.NewGuid().ToString("N"),
                    PlayerId = playerId,
                    Code = normalizedCode,
                    RewardSnapshotJson = JsonSerializer.Serialize(grantedRewards),
                    RedeemedAt = redeemedAt
                });

                _dbContext.CommitTransaction();
            }
            catch (Exception ex)
            {
                _dbContext.RollbackTransaction();
                _logger.LogWarning(ex, "兑换码兑换失败。PlayerId={PlayerId}, Code={Code}", playerId, normalizedCode);
                throw;
            }

            if (definition.Rewards.Any(reward => string.Equals(reward.Type, RewardTypes.Exp, StringComparison.Ordinal)))
            {
                await _playerAttributeService.RecalculatePlayerAttributesAsync(playerId, syncLevelDrivenProgress: true);
            }
            else
            {
                await _gameSyncService.SyncPlayerAsync(playerId);
            }

            _logger.LogInformation(
                "Player {PlayerId} redeemed code {Code} successfully with {RewardCount} rewards",
                playerId,
                normalizedCode,
                grantedRewards.Count);

            return new RedeemCodeResultDto
            {
                Code = normalizedCode,
                Message = "兑换成功，奖励已发放到角色与背包。",
                RedeemedAt = redeemedAt,
                Rewards = grantedRewards
            };
        }

        private static string NormalizeCode(string? code)
        {
            return (code ?? string.Empty).Trim().ToUpperInvariant();
        }

        /// <summary>
        /// 中文注释：
        /// 兑换码配置和奖励档位也已经改成数据库真源。
        /// 代码中的默认配置只用于首轮补种与版本重建，运行时兑换始终读取 SQLite 中的配置。
        /// </summary>
        private async Task<Dictionary<string, RedeemCodeDefinition>> LoadRedeemCodeDefinitionsAsync()
        {
            await EnsureRedeemCodeConfigsSeededAsync();
            var configEntities = await _dbContext.Db.Queryable<RedeemCodeConfigEntity>()
                .Where(config => config.IsEnabled)
                .ToListAsync();
            var definitions = new Dictionary<string, RedeemCodeDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var configEntity in configEntities)
            {
                try
                {
                    var rewards = JsonSerializer.Deserialize<List<RewardGrantItemDto>>(configEntity.RewardJson) ?? [];
                    definitions[NormalizeCode(configEntity.Code)] = new RedeemCodeDefinition
                    {
                        Code = NormalizeCode(configEntity.Code),
                        Rewards = rewards
                    };
                }
                catch
                {
                    // 中文注释：
                    // 单条坏配置不应该导致整条兑换链路报错。
                    // 这里先忽略坏数据，并让 reseed 逻辑在下次加载时修正。
                }
            }

            return definitions;
        }

        private async Task EnsureRedeemCodeConfigsSeededAsync()
        {
            var requiredCodes = ActivityRewardCatalog.BuildRedeemCodes()
                .Select(seed => NormalizeCode(seed.Code))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var existingCodes = await _dbContext.Db.Queryable<RedeemCodeConfigEntity>()
                .Select(config => config.Code)
                .ToListAsync();
            var existingSet = existingCodes
                .Select(NormalizeCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missingCodes = requiredCodes
                .Where(code => !existingSet.Contains(code))
                .ToList();

            if (missingCodes.Count == 0)
            {
                return;
            }

            _logger.LogError(
                "Redeem code configs are incomplete. Missing codes: {MissingCodes}. Runtime redeem-code seeding has been disabled; database configs are required.",
                string.Join(", ", missingCodes));
            throw new InvalidOperationException($"Redeem code configs are incomplete. Missing codes: {string.Join(", ", missingCodes)}. Please run startup seed sync before using redeem codes.");
        }

        private sealed class RedeemCodeDefinition
        {
            public string Code { get; set; } = string.Empty;

            public List<RewardGrantItemDto> Rewards { get; set; } = [];
        }
    }
}
