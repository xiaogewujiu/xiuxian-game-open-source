#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminRankingService : IAdminRankingService
    {
        private readonly IRepository<RankingConfigEntity> _rankingConfigRepository;
        private readonly IRepository<RankingRewardEntity> _rankingRewardRepository;
        private readonly IRepository<RankingEntryEntity> _rankingEntryRepository;
        private readonly IRepository<RankingHistoryEntity> _rankingHistoryRepository;
        private readonly IAdminRuntimeRefreshService _runtimeRefreshService;

        public AdminRankingService(
            IRepository<RankingConfigEntity> rankingConfigRepository,
            IRepository<RankingRewardEntity> rankingRewardRepository,
            IRepository<RankingEntryEntity> rankingEntryRepository,
            IRepository<RankingHistoryEntity> rankingHistoryRepository,
            IAdminRuntimeRefreshService runtimeRefreshService)
        {
            _rankingConfigRepository = rankingConfigRepository;
            _rankingRewardRepository = rankingRewardRepository;
            _rankingEntryRepository = rankingEntryRepository;
            _rankingHistoryRepository = rankingHistoryRepository;
            _runtimeRefreshService = runtimeRefreshService;
        }

        public async Task<List<AdminRankingConfigListItemDto>> GetConfigsAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _rankingConfigRepository.Db.Queryable<RankingConfigEntity>();
            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(config => config.RankingId.Contains(normalizedKeyword) || config.RankingName.Contains(normalizedKeyword));
            }

            var configs = await query.OrderBy(config => config.SortOrder).ToListAsync();
            return configs.Select(config => new AdminRankingConfigListItemDto
            {
                RankingId = config.RankingId,
                RankingName = config.RankingName,
                RankingType = config.RankingType,
                IsEnabled = config.IsEnabled,
                IsBuiltIn = config.IsBuiltIn,
                BuiltInVersion = config.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminRankingConfigDetailDto?> GetConfigDetailAsync(string rankingId)
        {
            if (string.IsNullOrWhiteSpace(rankingId)) return null;
            var config = await _rankingConfigRepository.GetByIdAsync(rankingId.Trim());
            if (config == null) return null;
            return new AdminRankingConfigDetailDto
            {
                RankingId = config.RankingId,
                RankingName = config.RankingName,
                RankingType = config.RankingType,
                Description = config.Description,
                MaxSize = config.MaxSize,
                UpdateInterval = config.UpdateInterval,
                SeasonEnabled = config.SeasonEnabled,
                SeasonDuration = config.SeasonDuration,
                CurrentSeason = config.CurrentSeason,
                SeasonStartTime = config.SeasonStartTime,
                SortOrder = config.SortOrder,
                IsEnabled = config.IsEnabled,
                IsBuiltIn = config.IsBuiltIn,
                SeedKey = config.SeedKey,
                BuiltInVersion = config.BuiltInVersion,
                LastUpdateTime = config.LastUpdateTime
            };
        }

        public async Task<AdminRankingConfigDetailDto> SaveConfigAsync(AdminRankingConfigDetailDto request)
        {
            var rankingId = (request.RankingId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rankingId)) throw new InvalidOperationException("排行编号不能为空。");
            if (string.Equals(rankingId, "ranking_combat", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("战力排行榜已移除，不能重新创建或保存。");
            if (string.IsNullOrWhiteSpace(request.RankingName)) throw new InvalidOperationException("排行名称不能为空。");
            if (request.SeasonEnabled && request.SeasonStartTime == null) throw new InvalidOperationException("启用赛季机制时，必须填写赛季开始时间。");

            var existing = await _rankingConfigRepository.GetByIdAsync(rankingId);
            if (existing == null)
            {
                existing = new RankingConfigEntity { RankingId = rankingId };
                await _rankingConfigRepository.AddAsync(ApplyConfig(existing, request));
                await _runtimeRefreshService.ReloadRankingCacheAsync();
                return (await GetConfigDetailAsync(rankingId))!;
            }

            ApplyConfig(existing, request);
            await _rankingConfigRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRankingCacheAsync();
            return (await GetConfigDetailAsync(rankingId))!;
        }

        public async Task<bool> DeleteConfigAsync(string rankingId)
        {
            var normalizedRankingId = (rankingId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedRankingId)) return false;

            if (await _rankingRewardRepository.Db.Queryable<RankingRewardEntity>().Where(reward => reward.RankingId == normalizedRankingId).AnyAsync())
            {
                throw new InvalidOperationException("当前排行下仍存在奖励区间，请先删除奖励后再删除排行配置。");
            }

            if (await _rankingEntryRepository.Db.Queryable<RankingEntryEntity>().Where(entry => entry.RankingId == normalizedRankingId).AnyAsync())
            {
                throw new InvalidOperationException("当前排行仍被排行榜条目引用，不能直接删除。");
            }

            if (await _rankingHistoryRepository.Db.Queryable<RankingHistoryEntity>().Where(history => history.RankingId == normalizedRankingId).AnyAsync())
            {
                throw new InvalidOperationException("当前排行仍被排行榜历史快照引用，不能直接删除。");
            }

            var deleteRows = await _rankingConfigRepository.DeleteAsync(normalizedRankingId);
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRankingCacheAsync();
            }

            return deleteRows > 0;
        }

        public async Task<List<AdminRankingRewardListItemDto>> GetRewardsAsync(string? rankingId = null)
        {
            var normalizedRankingId = (rankingId ?? string.Empty).Trim();
            var query = _rankingRewardRepository.Db.Queryable<RankingRewardEntity>();
            if (!string.IsNullOrWhiteSpace(normalizedRankingId))
            {
                query = query.Where(reward => reward.RankingId == normalizedRankingId);
            }

            var rewards = await query.OrderBy(reward => reward.MinRank).ToListAsync();
            return rewards.Select(reward => new AdminRankingRewardListItemDto
            {
                GID = reward.GID,
                RankingId = reward.RankingId,
                RewardTitle = reward.RewardTitle,
                MinRank = reward.MinRank,
                MaxRank = reward.MaxRank,
                IsBuiltIn = reward.IsBuiltIn,
                BuiltInVersion = reward.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminRankingRewardDetailDto?> GetRewardDetailAsync(string gid)
        {
            if (string.IsNullOrWhiteSpace(gid)) return null;
            var reward = await _rankingRewardRepository.GetByIdAsync(gid.Trim());
            if (reward == null) return null;
            return new AdminRankingRewardDetailDto
            {
                GID = reward.GID,
                RankingId = reward.RankingId,
                MinRank = reward.MinRank,
                MaxRank = reward.MaxRank,
                RewardTitle = reward.RewardTitle,
                Gold = reward.Gold,
                SpiritStone = reward.SpiritStone,
                Title = reward.Title,
                IsBuiltIn = reward.IsBuiltIn,
                SeedKey = reward.SeedKey,
                BuiltInVersion = reward.BuiltInVersion,
                LastUpdateTime = reward.LastUpdateTime
            };
        }

        public async Task<AdminRankingRewardDetailDto> SaveRewardAsync(AdminRankingRewardDetailDto request)
        {
            var gid = (request.GID ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(gid)) { gid = Guid.NewGuid().ToString("N"); request.GID = gid; }
            var rankingId = (request.RankingId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(rankingId)) throw new InvalidOperationException("排行编号不能为空。");
            if (string.Equals(rankingId, "ranking_combat", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("战力排行榜已移除，不能配置奖励。");
            if (string.IsNullOrWhiteSpace(request.RewardTitle)) throw new InvalidOperationException("奖励标题不能为空。");
            if (request.MinRank <= 0 || request.MaxRank <= 0 || request.MinRank > request.MaxRank) throw new InvalidOperationException("奖励名次区间必须合法，且最小名次不能大于最大名次。");
            if (!await _rankingConfigRepository.ExistsAsync(config => config.RankingId == rankingId)) throw new InvalidOperationException("所属排行不存在，请先创建排行配置。");

            var hasOverlap = await _rankingRewardRepository.Db.Queryable<RankingRewardEntity>()
                .Where(reward => reward.RankingId == rankingId && reward.GID != gid)
                .Where(reward => request.MinRank <= reward.MaxRank && request.MaxRank >= reward.MinRank)
                .AnyAsync();
            if (hasOverlap) throw new InvalidOperationException("当前排行已存在重叠的奖励名次区间，请调整后再保存。");

            var existing = await _rankingRewardRepository.GetByIdAsync(gid);
            if (existing == null)
            {
                existing = new RankingRewardEntity { GID = gid };
                await _rankingRewardRepository.AddAsync(ApplyReward(existing, request));
                await _runtimeRefreshService.ReloadRankingCacheAsync();
                return (await GetRewardDetailAsync(gid))!;
            }

            ApplyReward(existing, request);
            await _rankingRewardRepository.UpdateAsync(existing);
            await _runtimeRefreshService.ReloadRankingCacheAsync();
            return (await GetRewardDetailAsync(gid))!;
        }

        public async Task<bool> DeleteRewardAsync(string gid)
        {
            if (string.IsNullOrWhiteSpace(gid)) return false;

            var deleteRows = await _rankingRewardRepository.DeleteAsync(gid.Trim());
            if (deleteRows > 0)
            {
                await _runtimeRefreshService.ReloadRankingCacheAsync();
            }

            return deleteRows > 0;
        }

        private static RankingConfigEntity ApplyConfig(RankingConfigEntity entity, AdminRankingConfigDetailDto request)
        {
            entity.RankingName = request.RankingName.Trim();
            entity.RankingType = request.RankingType;
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.MaxSize = Math.Max(1, request.MaxSize);
            entity.UpdateInterval = Math.Max(1, request.UpdateInterval);
            entity.SeasonEnabled = request.SeasonEnabled;
            entity.SeasonDuration = Math.Max(1, request.SeasonDuration);
            entity.CurrentSeason = Math.Max(1, request.CurrentSeason);
            entity.SeasonStartTime = request.SeasonStartTime;
            entity.SortOrder = request.SortOrder;
            entity.IsEnabled = request.IsEnabled;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }

        private static RankingRewardEntity ApplyReward(RankingRewardEntity entity, AdminRankingRewardDetailDto request)
        {
            entity.RankingId = request.RankingId.Trim();
            entity.MinRank = Math.Max(1, request.MinRank);
            entity.MaxRank = Math.Max(entity.MinRank, request.MaxRank);
            entity.RewardTitle = (request.RewardTitle ?? string.Empty).Trim();
            entity.Gold = Math.Max(0, request.Gold);
            entity.SpiritStone = Math.Max(0L, request.SpiritStone);
            entity.Title = string.IsNullOrWhiteSpace(request.Title) ? null : request.Title.Trim();
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }
    }
}
#pragma warning restore CS1591
