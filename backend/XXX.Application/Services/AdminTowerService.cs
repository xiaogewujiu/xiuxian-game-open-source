using Microsoft.Extensions.Logging;
using System.Text.Json;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminTowerService : IAdminTowerService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<TowerFloorConfigEntity> _floorConfigRepository;
        private readonly ILogger<AdminTowerService> _logger;

        public AdminTowerService(
            DbContext dbContext,
            IRepository<TowerFloorConfigEntity> floorConfigRepository,
            ILogger<AdminTowerService> logger)
        {
            _dbContext = dbContext;
            _floorConfigRepository = floorConfigRepository;
            _logger = logger;
        }

        public async Task<AdminTowerOverviewDto> GetOverviewAsync()
        {
            var totalPlayers = await _dbContext.Db.Queryable<TowerProgressEntity>().CountAsync();

            var today = DateTime.Today;
            var todayChallenges = await _dbContext.Db.Queryable<TowerBattleLogEntity>()
                .Where(l => l.CreatedAt >= today)
                .CountAsync();

            int avgFloor = 0;
            if (totalPlayers > 0)
            {
                var floors = await _dbContext.Db.Queryable<TowerProgressEntity>()
                    .Select(t => t.HighestFloor)
                    .ToListAsync();
                avgFloor = (int)floors.Average();
            }

            return new AdminTowerOverviewDto
            {
                TotalPlayers = totalPlayers,
                TodayChallenges = todayChallenges,
                AverageHighestFloor = avgFloor
            };
        }

        public async Task<List<AdminTowerPlayerDto>> GetPlayersAsync(string? keyword = null, int take = 200)
        {
            var players = await _dbContext.Db.Queryable<TowerProgressEntity>()
                .OrderByDescending(t => t.HighestFloor)
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
                return new AdminTowerPlayerDto
                {
                    Id = p.Id,
                    PlayerId = p.PlayerId,
                    PlayerName = user?.Name ?? "未知",
                    HighestFloor = p.HighestFloor,
                    CurrentFloor = p.CurrentFloor,
                    DailyAttemptsUsed = p.DailyAttemptsUsed,
                    LastAttemptAt = p.LastAttemptAt
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

        public async Task<List<AdminTowerFloorConfigDto>> GetFloorConfigsAsync()
        {
            var configs = await _dbContext.Db.Queryable<TowerFloorConfigEntity>()
                .OrderBy(f => f.Floor)
                .ToListAsync();

            return configs.Select(c => new AdminTowerFloorConfigDto
            {
                Id = c.Id,
                Floor = c.Floor,
                MonsterTemplateIdsJson = c.MonsterTemplateIdsJson,
                MonsterCount = c.MonsterCount,
                StatMultiplier = c.StatMultiplier,
                RewardGold = c.RewardGold,
                RewardExp = c.RewardExp,
                MilestoneRewardJson = c.MilestoneRewardJson
            }).ToList();
        }

        public async Task<bool> UpdateFloorConfigAsync(long id, AdminTowerFloorConfigDto dto)
        {
            var entity = await _floorConfigRepository.GetByIdAsync(id);
            if (entity == null) return false;

            entity.MonsterTemplateIdsJson = dto.MonsterTemplateIdsJson;
            entity.MonsterCount = dto.MonsterCount;
            entity.StatMultiplier = dto.StatMultiplier;
            entity.RewardGold = dto.RewardGold;
            entity.RewardExp = dto.RewardExp;
            entity.MilestoneRewardJson = dto.MilestoneRewardJson;

            await _floorConfigRepository.UpdateAsync(entity);
            _logger.LogInformation("楼层配置已更新。Floor={Floor}", entity.Floor);
            return true;
        }

        public async Task<int> BatchGenerateFloorsAsync(AdminTowerBatchGenerateDto dto)
        {
            var existingFloors = await _dbContext.Db.Queryable<TowerFloorConfigEntity>()
                .Select(f => f.Floor)
                .ToListAsync();

            var existingSet = existingFloors.ToHashSet();
            var newConfigs = new List<TowerFloorConfigEntity>();

            for (int floor = 1; floor <= 100; floor++)
            {
                if (existingSet.Contains(floor)) continue;

                var multiplier = dto.BaseMultiplier + (floor - 1) * dto.MultiplierGrowth;
                var gold = dto.BaseRewardGold + (floor - 1) * 50;
                var exp = dto.BaseRewardExp + (floor - 1) * 30;

                string? milestoneReward = null;
                if (floor % 10 == 0)
                {
                    milestoneReward = JsonSerializer.Serialize(new
                    {
                        spiritStone = floor * 2,
                        title = floor >= 100 ? "tower_master" : null
                    });
                }

                newConfigs.Add(new TowerFloorConfigEntity
                {
                    Floor = floor,
                    MonsterTemplateIdsJson = dto.MonsterTemplateIdsJson,
                    MonsterCount = dto.MonsterCountPerFloor,
                    StatMultiplier = multiplier,
                    RewardGold = gold,
                    RewardExp = exp,
                    MilestoneRewardJson = milestoneReward,
                    IsBuiltIn = true
                });
            }

            if (newConfigs.Count > 0)
            {
                await _floorConfigRepository.AddRangeAsync(newConfigs);
                _logger.LogInformation("批量生成{Count}层配置", newConfigs.Count);
            }

            return newConfigs.Count;
        }

        public async Task<List<TowerFloorDistributionDto>> GetFloorDistributionAsync()
        {
            var all = await _dbContext.Db.Queryable<TowerProgressEntity>()
                .Select(t => t.HighestFloor)
                .ToListAsync();

            return all.GroupBy(f => f)
                .Select(g => new TowerFloorDistributionDto
                {
                    Floor = g.Key,
                    PlayerCount = g.Count()
                })
                .OrderBy(d => d.Floor)
                .ToList();
        }
    }
}
