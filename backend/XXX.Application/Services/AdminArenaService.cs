using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;
using XXX.Ranking;

namespace XXX.Application.Services
{
    public class AdminArenaService : IAdminArenaService
    {
        private const string RANKING_ID = "ranking_arena";

        private readonly DbContext _dbContext;
        private readonly IRepository<ArenaPlayerEntity> _arenaPlayerRepository;
        private readonly IRepository<RankingRewardEntity> _rewardRepository;
        private readonly IRankingService _rankingService;
        private readonly IMailService _mailService;
        private readonly ITitleService _titleService;
        private readonly ILogger<AdminArenaService> _logger;

        public AdminArenaService(
            DbContext dbContext,
            IRepository<ArenaPlayerEntity> arenaPlayerRepository,
            IRepository<RankingRewardEntity> rewardRepository,
            IRankingService rankingService,
            IMailService mailService,
            ITitleService titleService,
            ILogger<AdminArenaService> logger)
        {
            _dbContext = dbContext;
            _arenaPlayerRepository = arenaPlayerRepository;
            _rewardRepository = rewardRepository;
            _rankingService = rankingService;
            _mailService = mailService;
            _titleService = titleService;
            _logger = logger;
        }

        public async Task<AdminArenaOverviewDto> GetOverviewAsync()
        {
            var totalPlayers = await _dbContext.Db.Queryable<ArenaPlayerEntity>().CountAsync();

            var today = DateTime.Today;
            var todayBattles = await _dbContext.Db.Queryable<ArenaBattleLogEntity>()
                .Where(l => l.CreatedAt >= today)
                .CountAsync();

            var activePlayers = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .Where(a => a.LastBattleAt >= today)
                .CountAsync();

            int averagePoints = 0;
            if (totalPlayers > 0)
            {
                var allPoints = await _dbContext.Db.Queryable<ArenaPlayerEntity>().Select(a => a.Points).ToListAsync();
                averagePoints = (int)allPoints.Average();
            }

            var maxSeason = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .MaxAsync(a => a.SeasonNumber);

            return new AdminArenaOverviewDto
            {
                SeasonNumber = maxSeason > 0 ? maxSeason : 1,
                TotalPlayers = totalPlayers,
                TodayBattles = todayBattles,
                ActivePlayers = activePlayers,
                AveragePoints = (int)averagePoints
            };
        }

        public async Task<List<AdminArenaPlayerDto>> GetPlayersAsync(string? keyword = null, int take = 200)
        {
            var players = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .OrderBy(a => a.Rank)
                .Take(Math.Max(1, take))
                .ToListAsync();

            if (players.Count == 0) return [];

            var playerIds = players.Select(p => p.PlayerId).ToList();
            var users = await _dbContext.Db.Queryable<UserEntity>()
                .Where(u => playerIds.Contains(u.GID))
                .ToListAsync();

            var userMap = users.ToDictionary(u => u.GID);

            var result = players.Select(p =>
            {
                userMap.TryGetValue(p.PlayerId, out var user);
                return new AdminArenaPlayerDto
                {
                    Id = p.Id,
                    PlayerId = p.PlayerId,
                    PlayerName = user?.Name ?? "未知",
                    Points = p.Points,
                    Rank = p.Rank,
                    Wins = p.Wins,
                    Losses = p.Losses,
                    WinStreak = p.WinStreak,
                    BestRank = p.BestRank,
                    LastBattleAt = p.LastBattleAt,
                    BannedUntil = p.BannedUntil,
                    BanReason = p.BanReason
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                result = result.Where(r =>
                    r.PlayerId.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    r.PlayerName.Contains(kw, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return result;
        }

        public async Task<List<AdminArenaBattleLogDto>> GetBattleLogsAsync(string? playerId = null, int take = 100)
        {
            var query = _dbContext.Db.Queryable<ArenaBattleLogEntity>();

            if (!string.IsNullOrWhiteSpace(playerId))
            {
                var pid = playerId.Trim();
                query = query.Where(l => l.AttackerId == pid || l.DefenderId == pid);
            }

            var logs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Take(Math.Max(1, take))
                .ToListAsync();

            return logs.Select(l => new AdminArenaBattleLogDto
            {
                Id = l.Id,
                AttackerId = l.AttackerId,
                AttackerName = l.AttackerName,
                DefenderId = l.DefenderId,
                DefenderName = l.DefenderName,
                AttackerPointsBefore = l.AttackerPointsBefore,
                DefenderPointsBefore = l.DefenderPointsBefore,
                AttackerPointsAfter = l.AttackerPointsAfter,
                DefenderPointsAfter = l.DefenderPointsAfter,
                WinnerId = l.WinnerId,
                BattleLogJson = l.BattleLogJson,
                SeasonNumber = l.SeasonNumber,
                CreatedAt = l.CreatedAt
            }).ToList();
        }

        public async Task<bool> AdjustPointsAsync(AdminAdjustPointsDto dto)
        {
            var arena = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .Where(a => a.PlayerId == dto.PlayerId)
                .FirstAsync();

            if (arena == null) return false;

            arena.Points = Math.Max(0, dto.Points);
            arena.UpdatedAt = DateTime.Now;
            await _arenaPlayerRepository.UpdateAsync(arena);

            _logger.LogInformation("管理员调整竞技场积分。PlayerId={PlayerId}, NewPoints={Points}", dto.PlayerId, dto.Points);
            return true;
        }

        public async Task<bool> BanPlayerAsync(AdminBanPlayerDto dto)
        {
            var arena = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .Where(a => a.PlayerId == dto.PlayerId)
                .FirstAsync();

            if (arena == null) return false;

            arena.BannedUntil = DateTime.Now.AddHours(Math.Max(1, dto.Hours));
            arena.BanReason = dto.Reason;
            arena.UpdatedAt = DateTime.Now;
            await _arenaPlayerRepository.UpdateAsync(arena);

            _logger.LogInformation("管理员禁赛竞技场玩家。PlayerId={PlayerId}, Until={Until}, Reason={Reason}",
                dto.PlayerId, arena.BannedUntil, dto.Reason);
            return true;
        }

        public async Task<bool> UnbanPlayerAsync(AdminUnbanPlayerDto dto)
        {
            var arena = await _dbContext.Db.Queryable<ArenaPlayerEntity>()
                .Where(a => a.PlayerId == dto.PlayerId)
                .FirstAsync();

            if (arena == null) return false;

            arena.BannedUntil = null;
            arena.BanReason = null;
            arena.UpdatedAt = DateTime.Now;
            await _arenaPlayerRepository.UpdateAsync(arena);

            _logger.LogInformation("管理员解禁竞技场玩家。PlayerId={PlayerId}", dto.PlayerId);
            return true;
        }

        public async Task<AdminArenaSeasonDto> GetSeasonInfoAsync()
        {
            var config = await _rankingService.GetRankingConfigAsync(RANKING_ID);
            if (config == null)
            {
                return new AdminArenaSeasonDto { SeasonNumber = 1 };
            }

            var rewards = await _dbContext.Db.Queryable<RankingRewardEntity>()
                .Where(r => r.RankingId == RANKING_ID)
                .OrderBy(r => r.MinRank)
                .ToListAsync();

            var totalPlayers = await _dbContext.Db.Queryable<ArenaPlayerEntity>().CountAsync();

            return new AdminArenaSeasonDto
            {
                SeasonNumber = config.CurrentSeason,
                SeasonEnabled = config.SeasonEnabled,
                SeasonDuration = config.SeasonDuration,
                SeasonStartTime = config.SeasonStartTime,
                SeasonEndTime = config.GetSeasonEndTime(),
                DaysRemaining = config.GetSeasonDaysRemaining(),
                TotalPlayers = totalPlayers,
                Rewards = rewards.Select(r => new AdminArenaSeasonRewardDto
                {
                    GID = r.GID,
                    MinRank = r.MinRank,
                    MaxRank = r.MaxRank,
                    RewardTitle = r.RewardTitle,
                    Gold = r.Gold,
                    SpiritStone = r.SpiritStone,
                    Title = r.Title
                }).ToList()
            };
        }

        public async Task<AdminSettleSeasonResultDto> SettleSeasonAsync()
        {
            var config = await _rankingService.GetRankingConfigAsync(RANKING_ID);
            if (config == null) throw new InvalidOperationException("竞技场排行榜配置不存在。");

            var seasonNumber = config.CurrentSeason;
            var messages = new List<string>();

            // 获取排行榜快照
            var entries = await _rankingService.GetTopNAsync(RANKING_ID, 100);
            var rewards = await _dbContext.Db.Queryable<RankingRewardEntity>()
                .Where(r => r.RankingId == RANKING_ID)
                .OrderBy(r => r.MinRank)
                .ToListAsync();

            int rewardedCount = 0;

            foreach (var entry in entries)
            {
                var reward = rewards.FirstOrDefault(r => entry.Rank >= r.MinRank && entry.Rank <= r.MaxRank);
                if (reward == null) continue;

                var goldReward = reward.Gold;
                var stoneReward = reward.SpiritStone;
                var titleReward = reward.Title;

                if (goldReward <= 0 && stoneReward <= 0 && string.IsNullOrWhiteSpace(titleReward)) continue;

                var mailSubject = $"竞技场第{seasonNumber}赛季结算奖励";
                var mailContent = $"恭喜您在竞技场第{seasonNumber}赛季中获得第{entry.Rank}名！";
                var items = new Dictionary<string, int>();
                if (goldReward > 0) mailContent += $"\n金币奖励: {goldReward}";
                if (stoneReward > 0) mailContent += $"\n灵石奖励: {stoneReward}";
                if (!string.IsNullOrWhiteSpace(titleReward)) mailContent += $"\n称号奖励: {titleReward}";

                try
                {
                    var attachments = new Dictionary<string, long>();
                    if (goldReward > 0) attachments["Gold"] = goldReward;
                    if (stoneReward > 0) attachments["SpiritStone"] = stoneReward;
                    var attachmentsJson = attachments.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(attachments) : null;

                    await _mailService.SendMailAsync(entry.PlayerId, "System", "竞技场赛季", mailSubject, mailContent, attachmentsJson);
                    rewardedCount++;
                    messages.Add($"玩家 {entry.PlayerName}(#{entry.Rank}) 已发放奖励邮件。");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "发放赛季奖励邮件失败。PlayerId={PlayerId}", entry.PlayerId);
                    messages.Add($"玩家 {entry.PlayerName}(#{entry.Rank}) 奖励发放失败: {ex.Message}");
                }

                // 发放称号
                if (!string.IsNullOrWhiteSpace(titleReward))
                {
                    try
                    {
                        await _titleService.GrantTitleAsync(entry.PlayerId, titleReward);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "发放赛季称号失败。PlayerId={PlayerId}, Title={Title}", entry.PlayerId, titleReward);
                    }
                }
            }

            // 创建赛季快照
            await _rankingService.CreateSnapshotAsync(RANKING_ID, SnapshotType.SeasonEnd, $"竞技场第{seasonNumber}赛季结算");

            // 开始新赛季并持久化
            config.StartNewSeason();
            await _dbContext.Db.Updateable<RankingConfigEntity>()
                .SetColumns(c => c.CurrentSeason == config.CurrentSeason)
                .SetColumns(c => c.SeasonStartTime == config.SeasonStartTime)
                .Where(c => c.RankingId == RANKING_ID)
                .ExecuteCommandAsync();
            await _rankingService.ReloadCacheAsync();

            // 重置所有玩家积分和赛季数据
            var allPlayers = await _dbContext.Db.Queryable<ArenaPlayerEntity>().ToListAsync();
            foreach (var player in allPlayers)
            {
                player.Points = 1000;
                player.Wins = 0;
                player.Losses = 0;
                player.WinStreak = 0;
                player.DailyBattlesUsed = 0;
                player.DailyBattlesPurchased = 0;
                player.SeasonNumber = config.CurrentSeason;
                player.UpdatedAt = DateTime.Now;
            }
            if (allPlayers.Count > 0)
                await _arenaPlayerRepository.UpdateRangeAsync(allPlayers);

            _logger.LogInformation("竞技场赛季结算完成。Season={Season}, RewardedPlayers={Count}", seasonNumber, rewardedCount);

            return new AdminSettleSeasonResultDto
            {
                SeasonNumber = seasonNumber,
                RewardedPlayers = rewardedCount,
                Messages = messages
            };
        }

        public async Task<bool> ResetSeasonAsync()
        {
            // 清空所有玩家数据
            await _dbContext.Db.Deleteable<ArenaPlayerEntity>().ExecuteCommandAsync();
            await _dbContext.Db.Deleteable<ArenaBattleLogEntity>().ExecuteCommandAsync();

            // 重置赛季编号为1
            var config = await _rankingService.GetRankingConfigAsync(RANKING_ID);
            if (config != null)
            {
                config.CurrentSeason = 1;
                config.SeasonStartTime = DateTime.Now;
                await _dbContext.Db.Updateable<RankingConfigEntity>()
                    .SetColumns(c => c.CurrentSeason == 1)
                    .SetColumns(c => c.SeasonStartTime == config.SeasonStartTime)
                    .Where(c => c.RankingId == RANKING_ID)
                    .ExecuteCommandAsync();
                await _rankingService.ReloadCacheAsync();
            }

            _logger.LogInformation("管理员重置竞技场赛季。所有玩家数据已清空。");
            return true;
        }

        public async Task<bool> AdjustSeasonAsync(AdminAdjustSeasonDto dto)
        {
            var config = await _rankingService.GetRankingConfigAsync(RANKING_ID);
            if (config == null) return false;

            config.SeasonDuration = Math.Max(1, dto.SeasonDuration);
            if (dto.SeasonEnabled.HasValue)
                config.SeasonEnabled = dto.SeasonEnabled.Value;

            // 持久化到数据库
            await _dbContext.Db.Updateable<RankingConfigEntity>()
                .SetColumns(c => c.SeasonDuration == config.SeasonDuration)
                .SetColumns(c => c.SeasonEnabled == config.SeasonEnabled)
                .Where(c => c.RankingId == RANKING_ID)
                .ExecuteCommandAsync();
            await _rankingService.ReloadCacheAsync();

            _logger.LogInformation("管理员调整竞技场赛季配置。Duration={Duration}, Enabled={Enabled}",
                config.SeasonDuration, config.SeasonEnabled);
            return true;
        }

        public async Task<List<AdminArenaSeasonRewardDto>> GetSeasonRewardsAsync()
        {
            var rewards = await _dbContext.Db.Queryable<RankingRewardEntity>()
                .Where(r => r.RankingId == RANKING_ID)
                .OrderBy(r => r.MinRank)
                .ToListAsync();

            return rewards.Select(r => new AdminArenaSeasonRewardDto
            {
                GID = r.GID,
                MinRank = r.MinRank,
                MaxRank = r.MaxRank,
                RewardTitle = r.RewardTitle,
                Gold = r.Gold,
                SpiritStone = r.SpiritStone,
                Title = r.Title
            }).ToList();
        }

        public async Task<bool> SaveSeasonRewardAsync(AdminSaveSeasonRewardDto dto)
        {
            if (dto.MinRank < 1 || dto.MaxRank < dto.MinRank)
                throw new InvalidOperationException("排名范围无效。");

            if (!string.IsNullOrWhiteSpace(dto.GID))
            {
                // 更新
                var existing = await _dbContext.Db.Queryable<RankingRewardEntity>()
                    .Where(r => r.GID == dto.GID)
                    .FirstAsync();
                if (existing == null) return false;

                existing.MinRank = dto.MinRank;
                existing.MaxRank = dto.MaxRank;
                existing.RewardTitle = dto.RewardTitle;
                existing.Gold = dto.Gold;
                existing.SpiritStone = dto.SpiritStone;
                existing.Title = dto.Title;
                existing.LastUpdateTime = DateTime.Now;
                await _rewardRepository.UpdateAsync(existing);
            }
            else
            {
                // 新增
                var entity = new RankingRewardEntity
                {
                    GID = Guid.NewGuid().ToString("N"),
                    RankingId = RANKING_ID,
                    MinRank = dto.MinRank,
                    MaxRank = dto.MaxRank,
                    RewardTitle = dto.RewardTitle,
                    Gold = dto.Gold,
                    SpiritStone = dto.SpiritStone,
                    Title = dto.Title,
                    LastUpdateTime = DateTime.Now
                };
                await _rewardRepository.AddAsync(entity);
            }

            // 刷新缓存
            await _rankingService.ReloadCacheAsync();

            _logger.LogInformation("保存竞技场赛季奖励。MinRank={Min}, MaxRank={Max}, Title={Title}",
                dto.MinRank, dto.MaxRank, dto.RewardTitle);
            return true;
        }

        public async Task<bool> DeleteSeasonRewardAsync(string gid)
        {
            var existing = await _dbContext.Db.Queryable<RankingRewardEntity>()
                .Where(r => r.GID == gid && r.RankingId == RANKING_ID)
                .FirstAsync();
            if (existing == null) return false;

            await _rewardRepository.DeleteAsync(existing);
            await _rankingService.ReloadCacheAsync();

            _logger.LogInformation("删除竞技场赛季奖励。GID={GID}", gid);
            return true;
        }
    }
}
