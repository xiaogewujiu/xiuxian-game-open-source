using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminLotteryService : IAdminLotteryService
    {
        private readonly IRepository<LotteryPoolEntity> _poolRepo;
        private readonly IRepository<LotteryPrizeEntity> _prizeRepo;
        private readonly IRepository<LotteryLogEntity> _logRepo;
        private readonly ILogger<AdminLotteryService> _logger;

        public AdminLotteryService(
            IRepository<LotteryPoolEntity> poolRepo,
            IRepository<LotteryPrizeEntity> prizeRepo,
            IRepository<LotteryLogEntity> logRepo,
            ILogger<AdminLotteryService> logger)
        {
            _poolRepo = poolRepo;
            _prizeRepo = prizeRepo;
            _logRepo = logRepo;
            _logger = logger;
        }

        public async Task<List<AdminLotteryPoolListItemDto>> GetPoolListAsync(string? keyword)
        {
            var query = _poolRepo.Db.Queryable<LotteryPoolEntity>();
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.Name.Contains(keyword));
            var list = await query.OrderBy(x => x.SortOrder).ToListAsync();
            return list.Select(x => new AdminLotteryPoolListItemDto
            {
                PoolId = x.PoolId, Name = x.Name, LotteryType = x.LotteryType,
                IsEnabled = x.IsEnabled, IsBuiltIn = x.IsBuiltIn, BuiltInVersion = x.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminLotteryPoolDetailDto?> GetPoolDetailAsync(string poolId)
        {
            var entity = await _poolRepo.GetByIdAsync(poolId);
            if (entity == null) return null;
            return new AdminLotteryPoolDetailDto
            {
                PoolId = entity.PoolId, Name = entity.Name, LotteryType = entity.LotteryType,
                CostType = entity.CostType, CostItemId = entity.CostItemId, CostAmount = entity.CostAmount,
                IsEnabled = entity.IsEnabled, StartTime = entity.StartTime, EndTime = entity.EndTime,
                SupportSingle = entity.SupportSingle, SupportTen = entity.SupportTen,
                DailyLimit = entity.DailyLimit, TotalLimit = entity.TotalLimit,
                SortOrder = entity.SortOrder, IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey, BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        public async Task<AdminLotteryPoolDetailDto> SavePoolAsync(AdminLotteryPoolDetailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PoolId))
                throw new InvalidOperationException("抽奖池ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("抽奖池名称不能为空。");
            if (dto.CostAmount <= 0)
                throw new InvalidOperationException("消耗数量必须大于0。");
            if (dto.CostType == 2 && string.IsNullOrWhiteSpace(dto.CostItemId))
                throw new InvalidOperationException("消耗类型为道具时必须选择道具ID。");

            // 校验概率总和
            if (dto.IsEnabled)
            {
                var enabledPrizes = await _prizeRepo.Db.Queryable<LotteryPrizeEntity>()
                    .Where(x => x.PoolId == dto.PoolId && x.IsEnabled)
                    .ToListAsync();
                var totalProb = enabledPrizes.Sum(x => x.Probability);
                if (totalProb > 10000)
                    throw new InvalidOperationException($"启用奖项概率总和将超过10000（当前总和：{totalProb}）。");
            }

            var existing = await _poolRepo.GetByIdAsync(dto.PoolId);
            if (existing != null)
            {
                existing.Name = dto.Name;
                existing.LotteryType = dto.LotteryType;
                existing.CostType = dto.CostType;
                existing.CostItemId = dto.CostItemId;
                existing.CostAmount = dto.CostAmount;
                existing.IsEnabled = dto.IsEnabled;
                existing.StartTime = dto.StartTime;
                existing.EndTime = dto.EndTime;
                existing.SupportSingle = dto.SupportSingle;
                existing.SupportTen = dto.SupportTen;
                existing.DailyLimit = dto.DailyLimit;
                existing.TotalLimit = dto.TotalLimit;
                existing.SortOrder = dto.SortOrder;
                existing.SeedKey = dto.SeedKey;
                existing.IsBuiltIn = dto.IsBuiltIn;
                existing.BuiltInVersion = dto.BuiltInVersion;
                existing.LastUpdateTime = DateTime.Now;
                await _poolRepo.UpdateAsync(existing);
            }
            else
            {
                var entity = new LotteryPoolEntity
                {
                    PoolId = dto.PoolId, Name = dto.Name, LotteryType = dto.LotteryType,
                    CostType = dto.CostType, CostItemId = dto.CostItemId, CostAmount = dto.CostAmount,
                    IsEnabled = dto.IsEnabled, StartTime = dto.StartTime, EndTime = dto.EndTime,
                    SupportSingle = dto.SupportSingle, SupportTen = dto.SupportTen,
                    DailyLimit = dto.DailyLimit, TotalLimit = dto.TotalLimit,
                    SortOrder = dto.SortOrder, SeedKey = dto.SeedKey,
                    IsBuiltIn = dto.IsBuiltIn, BuiltInVersion = dto.BuiltInVersion,
                    LastUpdateTime = DateTime.Now
                };
                await _poolRepo.AddAsync(entity);
            }
            return dto;
        }

        public async Task<bool> DeletePoolAsync(string poolId)
        {
            var prizes = await _prizeRepo.Db.Queryable<LotteryPrizeEntity>().Where(x => x.PoolId == poolId).ToListAsync();
            if (prizes.Count > 0)
                throw new InvalidOperationException("该抽奖池下还有奖项配置，请先删除奖项。");
            return await _poolRepo.DeleteAsync(poolId) > 0;
        }

        public async Task<List<AdminLotteryPrizeListItemDto>> GetPrizeListAsync(string? poolId)
        {
            var query = _prizeRepo.Db.Queryable<LotteryPrizeEntity>();
            if (!string.IsNullOrWhiteSpace(poolId))
                query = query.Where(x => x.PoolId == poolId);
            var list = await query.OrderBy(x => x.SortOrder).ToListAsync();
            return list.Select(x => new AdminLotteryPrizeListItemDto
            {
                PrizeId = x.PrizeId, PoolId = x.PoolId, RewardType = x.RewardType,
                RewardTargetId = x.RewardTargetId, RewardAmount = x.RewardAmount,
                Probability = x.Probability, IsEnabled = x.IsEnabled,
                IsBuiltIn = x.IsBuiltIn, BuiltInVersion = x.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminLotteryPrizeDetailDto?> GetPrizeDetailAsync(string prizeId)
        {
            var entity = await _prizeRepo.GetByIdAsync(prizeId);
            if (entity == null) return null;
            return new AdminLotteryPrizeDetailDto
            {
                PrizeId = entity.PrizeId, PoolId = entity.PoolId, RewardType = entity.RewardType,
                RewardTargetId = entity.RewardTargetId, RewardAmount = entity.RewardAmount,
                Probability = entity.Probability, SortOrder = entity.SortOrder,
                IsEnabled = entity.IsEnabled, IsBuiltIn = entity.IsBuiltIn,
                SeedKey = entity.SeedKey, BuiltInVersion = entity.BuiltInVersion,
                LastUpdateTime = entity.LastUpdateTime
            };
        }

        public async Task<AdminLotteryPrizeDetailDto> SavePrizeAsync(AdminLotteryPrizeDetailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.PrizeId))
                throw new InvalidOperationException("奖项ID不能为空。");
            if (string.IsNullOrWhiteSpace(dto.PoolId))
                throw new InvalidOperationException("所属抽奖池ID不能为空。");
            if (dto.Probability < 0)
                throw new InvalidOperationException("概率不能为负数。");

            var existing = await _prizeRepo.GetByIdAsync(dto.PrizeId);
            if (existing != null)
            {
                existing.PoolId = dto.PoolId;
                existing.RewardType = dto.RewardType;
                existing.RewardTargetId = dto.RewardTargetId;
                existing.RewardAmount = dto.RewardAmount;
                existing.Probability = dto.Probability;
                existing.SortOrder = dto.SortOrder;
                existing.IsEnabled = dto.IsEnabled;
                existing.LastUpdateTime = DateTime.Now;
                await _prizeRepo.UpdateAsync(existing);
            }
            else
            {
                var entity = new LotteryPrizeEntity
                {
                    PrizeId = dto.PrizeId, PoolId = dto.PoolId, RewardType = dto.RewardType,
                    RewardTargetId = dto.RewardTargetId, RewardAmount = dto.RewardAmount,
                    Probability = dto.Probability, SortOrder = dto.SortOrder,
                    IsEnabled = dto.IsEnabled, SeedKey = dto.SeedKey,
                    IsBuiltIn = dto.IsBuiltIn, BuiltInVersion = dto.BuiltInVersion,
                    LastUpdateTime = DateTime.Now
                };
                await _prizeRepo.AddAsync(entity);
            }
            return dto;
        }

        public async Task<bool> DeletePrizeAsync(string prizeId) => await _prizeRepo.DeleteAsync(prizeId) > 0;

        public async Task<(List<AdminLotteryLogListItemDto> Items, int Total)> GetLogsAsync(string? playerId, string? poolId, int page, int pageSize)
        {
            var query = _logRepo.Db.Queryable<LotteryLogEntity>();
            if (!string.IsNullOrWhiteSpace(playerId))
                query = query.Where(x => x.PlayerId == playerId);
            if (!string.IsNullOrWhiteSpace(poolId))
                query = query.Where(x => x.PoolId == poolId);

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(x => x.LotteryTime)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items.Select(x => new AdminLotteryLogListItemDto
            {
                LogId = x.LogId, PlayerId = x.PlayerId, PoolId = x.PoolId,
                LotteryType = x.LotteryType, CostType = x.CostType, CostAmount = x.CostAmount,
                RewardType = x.RewardType, RewardName = x.RewardName,
                RewardAmount = x.RewardAmount, IsThanks = x.IsThanks,
                LotteryTime = x.LotteryTime
            }).ToList(), total);
        }
    }
}
