#pragma warning disable CS1591
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Balance;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminRewardConfigService : IAdminRewardConfigService
    {
        private readonly IRepository<CheckInRewardConfigEntity> _checkInRepository;
        private readonly IRepository<RedeemCodeConfigEntity> _redeemCodeRepository;
        private readonly IRepository<RedeemCodeUsageEntity> _redeemCodeUsageRepository;

        public AdminRewardConfigService(
            IRepository<CheckInRewardConfigEntity> checkInRepository,
            IRepository<RedeemCodeConfigEntity> redeemCodeRepository,
            IRepository<RedeemCodeUsageEntity> redeemCodeUsageRepository)
        {
            _checkInRepository = checkInRepository;
            _redeemCodeRepository = redeemCodeRepository;
            _redeemCodeUsageRepository = redeemCodeUsageRepository;
        }

        public async Task<List<AdminCheckInRewardConfigDto>> GetCheckInConfigsAsync(bool? isMilestone = null)
        {
            var query = _checkInRepository.Db.Queryable<CheckInRewardConfigEntity>();

            query = query.WhereIF(isMilestone.HasValue, config => config.IsMilestone == isMilestone!.Value);

            var configs = await query
                .OrderBy(config => config.ContinuousDay)
                .ToListAsync();

            return configs.Select(config => new AdminCheckInRewardConfigDto
            {
                ContinuousDay = config.ContinuousDay,
                IsBuiltIn = config.IsBuiltIn,
                SeedKey = config.SeedKey,
                BuiltInVersion = config.BuiltInVersion,
                ConfigVersion = config.ConfigVersion,
                IsMilestone = config.IsMilestone,
                RewardJson = config.RewardJson,
                Description = config.Description,
                LastUpdateTime = config.LastUpdateTime
            }).ToList();
        }

        public async Task<AdminCheckInRewardConfigDto> SaveCheckInConfigAsync(AdminCheckInRewardConfigDto request)
        {
            if (request.ContinuousDay <= 0)
            {
                throw new InvalidOperationException("连续签到天数必须大于 0。");
            }

            var existing = await _checkInRepository.GetByIdAsync(request.ContinuousDay);
            if (existing == null)
            {
                existing = new CheckInRewardConfigEntity { ContinuousDay = request.ContinuousDay };
                await _checkInRepository.AddAsync(ApplyCheckIn(existing, request));
                return request;
            }

            ApplyCheckIn(existing, request);
            await _checkInRepository.UpdateAsync(existing);
            return request;
        }

        public async Task<bool> DeleteCheckInConfigAsync(int continuousDay)
        {
            if (continuousDay <= 0)
            {
                return false;
            }

            return await _checkInRepository.DeleteAsync(continuousDay) > 0;
        }

        public async Task<List<AdminRedeemCodeConfigDto>> GetRedeemCodeConfigsAsync(string? keyword = null, bool? isEnabled = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _redeemCodeRepository.Db.Queryable<RedeemCodeConfigEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(config => config.Code.Contains(normalizedKeyword) || config.Description.Contains(normalizedKeyword));
            }

            query = query.WhereIF(isEnabled.HasValue, config => config.IsEnabled == isEnabled!.Value);

            var configs = await query.OrderBy(config => config.Code).ToListAsync();
            return configs.Select(config => new AdminRedeemCodeConfigDto
            {
                Code = config.Code,
                IsBuiltIn = config.IsBuiltIn,
                SeedKey = config.SeedKey,
                BuiltInVersion = config.BuiltInVersion,
                ConfigVersion = config.ConfigVersion,
                IsEnabled = config.IsEnabled,
                Description = config.Description,
                RewardJson = config.RewardJson,
                LastUpdateTime = config.LastUpdateTime
            }).ToList();
        }

        public async Task<AdminRedeemCodeConfigDto> SaveRedeemCodeConfigAsync(AdminRedeemCodeConfigDto request)
        {
            var code = (request.Code ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("兑换码不能为空。");
            }

            var existing = await _redeemCodeRepository.GetByIdAsync(code);
            if (existing == null)
            {
                existing = new RedeemCodeConfigEntity { Code = code };
                await _redeemCodeRepository.AddAsync(ApplyRedeemCode(existing, request));
                request.Code = code;
                return request;
            }

            ApplyRedeemCode(existing, request);
            await _redeemCodeRepository.UpdateAsync(existing);
            request.Code = code;
            return request;
        }

        public async Task<bool> DeleteRedeemCodeConfigAsync(string code)
        {
            var normalizedCode = (code ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                return false;
            }

            var hasUsage = await _redeemCodeUsageRepository.Db.Queryable<RedeemCodeUsageEntity>()
                .Where(usage => usage.Code == normalizedCode)
                .AnyAsync();
            if (hasUsage)
            {
                throw new InvalidOperationException("当前兑换码已有玩家使用记录，不能直接删除。");
            }

            return await _redeemCodeRepository.DeleteAsync(normalizedCode) > 0;
        }

        public async Task ReloadBuiltInActivityConfigsAsync(string? operatorName = null)
        {
            var now = DateTime.Now;

            var builtInCheckInConfigs = ActivityRewardCatalog.BuildCheckInRewards()
                .Select(seed => new CheckInRewardConfigEntity
                {
                    ContinuousDay = seed.ContinuousDay,
                    SeedKey = BuildCheckInSeedKey(seed.ContinuousDay),
                    IsBuiltIn = true,
                    BuiltInVersion = ActivityRewardCatalog.CheckInConfigVersion,
                    ConfigVersion = ActivityRewardCatalog.CheckInConfigVersion,
                    IsMilestone = seed.IsMilestone,
                    RewardJson = JsonSerializer.Serialize(seed.Rewards),
                    Description = $"连续签到第 {seed.ContinuousDay} 天奖励",
                    LastUpdateTime = now
                })
                .ToList();

            foreach (var expected in builtInCheckInConfigs)
            {
                var existing = await _checkInRepository.GetByIdAsync(expected.ContinuousDay);
                if (existing == null)
                {
                    await _checkInRepository.AddAsync(expected);
                    continue;
                }

                var canOverwrite = existing.IsBuiltIn ||
                    string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase) ||
                    MatchesBuiltInCheckIn(existing, expected);
                if (!canOverwrite)
                {
                    continue;
                }

                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.ConfigVersion = expected.ConfigVersion;
                existing.IsMilestone = expected.IsMilestone;
                existing.RewardJson = expected.RewardJson;
                existing.Description = expected.Description;
                existing.LastUpdateTime = now;
                await _checkInRepository.UpdateAsync(existing);
            }

            var builtInRedeemConfigs = ActivityRewardCatalog.BuildRedeemCodes()
                .Select(seed => new RedeemCodeConfigEntity
                {
                    Code = seed.Code,
                    SeedKey = BuildRedeemSeedKey(seed.Code),
                    IsBuiltIn = true,
                    BuiltInVersion = ActivityRewardCatalog.RedeemCodeConfigVersion,
                    ConfigVersion = ActivityRewardCatalog.RedeemCodeConfigVersion,
                    IsEnabled = true,
                    Description = seed.Description,
                    RewardJson = JsonSerializer.Serialize(seed.Rewards),
                    LastUpdateTime = now
                })
                .ToList();

            foreach (var expected in builtInRedeemConfigs)
            {
                var existing = await _redeemCodeRepository.GetByIdAsync(expected.Code);
                if (existing == null)
                {
                    await _redeemCodeRepository.AddAsync(expected);
                    continue;
                }

                var canOverwrite = existing.IsBuiltIn ||
                    string.Equals(existing.SeedKey, expected.SeedKey, StringComparison.OrdinalIgnoreCase) ||
                    MatchesBuiltInRedeemCode(existing, expected);
                if (!canOverwrite)
                {
                    continue;
                }

                existing.SeedKey = expected.SeedKey;
                existing.IsBuiltIn = true;
                existing.BuiltInVersion = expected.BuiltInVersion;
                existing.ConfigVersion = expected.ConfigVersion;
                existing.IsEnabled = expected.IsEnabled;
                existing.Description = expected.Description;
                existing.RewardJson = expected.RewardJson;
                existing.LastUpdateTime = now;
                await _redeemCodeRepository.UpdateAsync(existing);
            }
        }

        private static CheckInRewardConfigEntity ApplyCheckIn(CheckInRewardConfigEntity entity, AdminCheckInRewardConfigDto request)
        {
            entity.ConfigVersion = (request.ConfigVersion ?? string.Empty).Trim();
            entity.IsMilestone = request.IsMilestone;
            entity.RewardJson = string.IsNullOrWhiteSpace(request.RewardJson) ? "[]" : request.RewardJson.Trim();
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.SeedKey = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static RedeemCodeConfigEntity ApplyRedeemCode(RedeemCodeConfigEntity entity, AdminRedeemCodeConfigDto request)
        {
            entity.ConfigVersion = (request.ConfigVersion ?? string.Empty).Trim();
            entity.IsEnabled = request.IsEnabled;
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.RewardJson = string.IsNullOrWhiteSpace(request.RewardJson) ? "[]" : request.RewardJson.Trim();
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.SeedKey = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static string BuildCheckInSeedKey(int day) => $"built-in:checkin:{day}";

        private static string BuildRedeemSeedKey(string code) => $"built-in:redeem:{code.Trim().ToUpperInvariant()}";

        private static bool MatchesBuiltInCheckIn(CheckInRewardConfigEntity actual, CheckInRewardConfigEntity expected)
        {
            return actual.ContinuousDay == expected.ContinuousDay &&
                   string.Equals(actual.ConfigVersion ?? string.Empty, expected.ConfigVersion ?? string.Empty, StringComparison.Ordinal) &&
                   actual.IsMilestone == expected.IsMilestone &&
                   string.Equals(NormalizeJson(actual.RewardJson), NormalizeJson(expected.RewardJson), StringComparison.Ordinal) &&
                   string.Equals(NormalizeText(actual.Description), NormalizeText(expected.Description), StringComparison.Ordinal);
        }

        private static bool MatchesBuiltInRedeemCode(RedeemCodeConfigEntity actual, RedeemCodeConfigEntity expected)
        {
            return string.Equals(actual.Code, expected.Code, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(actual.ConfigVersion ?? string.Empty, expected.ConfigVersion ?? string.Empty, StringComparison.Ordinal) &&
                   actual.IsEnabled == expected.IsEnabled &&
                   string.Equals(NormalizeText(actual.Description), NormalizeText(expected.Description), StringComparison.Ordinal) &&
                   string.Equals(NormalizeJson(actual.RewardJson), NormalizeJson(expected.RewardJson), StringComparison.Ordinal);
        }

        private static string NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string NormalizeJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return "[]";
            }

            try
            {
                using var document = JsonDocument.Parse(json);
                return document.RootElement.GetRawText();
            }
            catch
            {
                return json.Trim();
            }
        }
    }
}
#pragma warning restore CS1591
