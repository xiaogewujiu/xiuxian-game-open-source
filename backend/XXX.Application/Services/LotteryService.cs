using Microsoft.Extensions.Logging;
using SqlSugar;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    public class LotteryService : ILotteryService
    {
        private readonly ISqlSugarClient _db;
        private readonly ICollectionService _collectionService;
        private readonly ILogger<LotteryService> _logger;

        public LotteryService(DbContext dbContext, ICollectionService collectionService, ILogger<LotteryService> logger)
        {
            _db = dbContext.Db;
            _collectionService = collectionService;
            _logger = logger;
        }

        public async Task<List<LotteryPoolViewDto>> GetLotteryPoolsAsync(string playerId)
        {
            var now = DateTime.Now;
            var pools = await _db.Queryable<LotteryPoolEntity>()
                .Where(x => x.IsEnabled && (x.StartTime == null || x.StartTime <= now) && (x.EndTime == null || x.EndTime >= now))
                .OrderBy(x => x.SortOrder).ToListAsync();

            var result = new List<LotteryPoolViewDto>();
            foreach (var pool in pools)
            {
                var dailyCount = await _db.Queryable<LotteryLogEntity>()
                    .Where(x => x.PlayerId == playerId && x.PoolId == pool.PoolId && x.LotteryTime >= DateTime.Today)
                    .CountAsync();
                var totalCount = await _db.Queryable<LotteryLogEntity>()
                    .Where(x => x.PlayerId == playerId && x.PoolId == pool.PoolId)
                    .CountAsync();

                // 计算玩家当前货币/道具数量
                var player = await _db.Queryable<UserEntity>().Where(x => x.GID == playerId).FirstAsync();
                long playerCurrency = pool.CostType switch
                {
                    0 => player?.Gold ?? 0,
                    1 => player?.SpiritStone ?? 0,
                    _ => 0
                };
                int playerItemCount = 0;
                if (pool.CostType == 2 && !string.IsNullOrEmpty(pool.CostItemId))
                {
                    playerItemCount = await _db.Queryable<InventoryItemEntity>()
                        .Where(x => x.PlayerId == playerId && x.ItemId == pool.CostItemId)
                        .SumAsync(x => x.Quantity);
                }

                int dailyRemaining = pool.DailyLimit < 0 ? -1 : Math.Max(0, pool.DailyLimit - dailyCount);
                int totalRemaining = pool.TotalLimit < 0 ? -1 : Math.Max(0, pool.TotalLimit - totalCount);
                bool canDraw = (pool.CostType != 2 ? playerCurrency >= pool.CostAmount : playerItemCount >= pool.CostAmount)
                    && (dailyRemaining != 0) && (totalRemaining != 0);

                result.Add(new LotteryPoolViewDto
                {
                    PoolId = pool.PoolId,
                    Name = pool.Name,
                    LotteryType = pool.LotteryType,
                    CostType = pool.CostType,
                    CostItemId = pool.CostItemId,
                    CostAmount = pool.CostAmount,
                    SupportSingle = pool.SupportSingle,
                    SupportTen = pool.SupportTen,
                    DailyRemaining = dailyRemaining,
                    TotalRemaining = totalRemaining,
                    PlayerCurrency = playerCurrency,
                    PlayerItemCount = playerItemCount,
                    CanDraw = canDraw
                });
            }
            return result;
        }

        public async Task<LotteryDrawResultDto> DrawAsync(UserEntity player, LotteryDrawRequestDto request)
        {
            // 1. 校验抽奖池
            var pool = await _db.Queryable<LotteryPoolEntity>().InSingleAsync(request.PoolId);
            if (pool == null)
                return FailResult("抽奖池不存在。");
            if (!pool.IsEnabled)
                return FailResult("该抽奖池已关闭。");

            var now = DateTime.Now;
            if (pool.StartTime.HasValue && pool.StartTime > now)
                return FailResult("该抽奖池尚未开放。");
            if (pool.EndTime.HasValue && pool.EndTime < now)
                return FailResult("该抽奖池已过期。");

            int drawCount = request.Count == 10 ? 10 : 1;
            if (drawCount == 1 && !pool.SupportSingle)
                return FailResult("该抽奖池不支持单抽。");
            if (drawCount == 10 && !pool.SupportTen)
                return FailResult("该抽奖池不支持十连抽。");

            // 2. 检查次数限制
            var dailyCount = await _db.Queryable<LotteryLogEntity>()
                .Where(x => x.PlayerId == player.GID && x.PoolId == pool.PoolId && x.LotteryTime >= DateTime.Today)
                .CountAsync();
            if (pool.DailyLimit >= 0 && dailyCount + drawCount > pool.DailyLimit)
                return FailResult($"今日抽奖次数已达上限（{pool.DailyLimit}次）。");

            var totalCount = await _db.Queryable<LotteryLogEntity>()
                .Where(x => x.PlayerId == player.GID && x.PoolId == pool.PoolId)
                .CountAsync();
            if (pool.TotalLimit >= 0 && totalCount + drawCount > pool.TotalLimit)
                return FailResult($"该抽奖池总抽奖次数已达上限（{pool.TotalLimit}次）。");

            // 3. 计算总消耗
            int totalCost = pool.CostAmount * drawCount;

            // 4. 开始事务
            _db.Ado.BeginTran();
            try
            {
                // 5. 扣除资源
                DeductResource(player, pool.CostType, pool.CostItemId, totalCost);
                await _db.Updateable(player).UpdateColumns(x => new { x.Gold, x.SpiritStone }).ExecuteCommandAsync();

                // 6. 执行抽奖
                var prizes = await _db.Queryable<LotteryPrizeEntity>()
                    .Where(x => x.PoolId == pool.PoolId && x.IsEnabled)
                    .OrderBy(x => x.SortOrder).ToListAsync();

                if (prizes.Count == 0)
                {
                    _db.Ado.RollbackTran();
                    return FailResult("该抽奖池暂无奖项配置。");
                }

                var rewards = new List<LotteryDrawItemDto>();
                for (int i = 0; i < drawCount; i++)
                {
                    var prize = RollByProbability(prizes);
                    var item = await GrantPrizeAsync(player, prize);
                    rewards.Add(item);

                    // 记录日志
                    await _db.Insertable(new LotteryLogEntity
                    {
                        LogId = Guid.NewGuid().ToString("N"),
                        PlayerId = player.GID,
                        PoolId = pool.PoolId,
                        LotteryType = pool.LotteryType,
                        CostType = pool.CostType,
                        CostItemId = pool.CostItemId,
                        CostAmount = pool.CostAmount,
                        PrizeId = prize.PrizeId,
                        RewardType = prize.RewardType,
                        RewardTargetId = prize.RewardTargetId ?? "",
                        RewardName = item.Name,
                        RewardAmount = prize.RewardAmount,
                        IsThanks = prize.RewardType == 5,
                        RandomValue = 0,
                        LotteryTime = DateTime.Now,
                        PlayerIp = ""
                    }).ExecuteCommandAsync();
                }

                _db.Ado.CommitTran();

                return new LotteryDrawResultDto
                {
                    Success = true,
                    DrawCount = drawCount,
                    Cost = new LotteryDrawCostDto
                    {
                        Type = pool.CostType switch { 0 => "gold", 1 => "spiritStone", 2 => "item", _ => "unknown" },
                        ItemId = pool.CostItemId,
                        Amount = totalCost
                    },
                    Rewards = rewards,
                    Message = "抽奖成功"
                };
            }
            catch (Exception ex)
            {
                _db.Ado.RollbackTran();
                _logger.LogError(ex, "抽奖失败 PlayerId={PlayerId}, PoolId={PoolId}", player.GID, pool.PoolId);
                return FailResult($"抽奖失败：{ex.Message}");
            }
        }

        private static LotteryDrawResultDto FailResult(string message)
        {
            return new LotteryDrawResultDto { Success = false, Message = message };
        }

        private void DeductResource(UserEntity player, int costType, string? costItemId, int amount)
        {
            switch (costType)
            {
                case 0: // 金币
                    if (player.Gold < amount)
                        throw new InvalidOperationException($"金币不足，需要{amount}，当前{player.Gold}。");
                    player.Gold -= amount;
                    break;
                case 1: // 灵石
                    if (player.SpiritStone < amount)
                        throw new InvalidOperationException($"灵石不足，需要{amount}，当前{player.SpiritStone}。");
                    player.SpiritStone -= amount;
                    break;
                case 2: // 道具
                    throw new NotImplementedException("道具消耗暂未实现，请使用金币或灵石消耗类型。");
                default:
                    throw new InvalidOperationException($"未知消耗类型：{costType}");
            }
        }

        private LotteryPrizeEntity RollByProbability(List<LotteryPrizeEntity> prizes)
        {
            var rand = new Random();
            int roll = rand.Next(0, 10000);
            int cumulative = 0;

            foreach (var prize in prizes)
            {
                cumulative += prize.Probability;
                if (roll < cumulative)
                    return prize;
            }

            return prizes.Last();
        }

        private async Task<LotteryDrawItemDto> GrantPrizeAsync(UserEntity player, LotteryPrizeEntity prize)
        {
            var result = new LotteryDrawItemDto
            {
                Count = prize.RewardAmount
            };

            switch (prize.RewardType)
            {
                case 0: // 金币
                    player.Gold += prize.RewardAmount;
                    await _db.Updateable(player).UpdateColumns(x => new { x.Gold }).ExecuteCommandAsync();
                    result.RewardType = "gold";
                    result.Name = $"{prize.RewardAmount}金币";
                    break;

                case 1: // 灵石
                    player.SpiritStone += prize.RewardAmount;
                    await _db.Updateable(player).UpdateColumns(x => new { x.SpiritStone }).ExecuteCommandAsync();
                    result.RewardType = "spiritStone";
                    result.Name = $"{prize.RewardAmount}灵石";
                    break;

                case 2: // 道具
                    result.RewardType = "item";
                    result.ItemId = prize.RewardTargetId;
                    result.Name = $"道具 x{prize.RewardAmount}";
                    break;

                case 3: // 文字图鉴
                    {
                        result.RewardType = "text_collection";
                        // rewardTargetId 可能是系列ID或图鉴项ID，优先按系列查找
                        var textItemId = prize.RewardTargetId!;
                        var textSeries = await _db.Queryable<TextCollectionSeriesEntity>()
                            .Where(x => x.SeriesId == prize.RewardTargetId).FirstAsync();
                        if (textSeries != null)
                        {
                            // 是系列ID，从中随机选一个图鉴项
                            var seriesItems = await _db.Queryable<TextCollectionItemEntity>()
                                .Where(x => x.SeriesId == prize.RewardTargetId && x.IsEnabled)
                                .ToListAsync();
                            if (seriesItems.Count > 0)
                                textItemId = seriesItems[Random.Shared.Next(seriesItems.Count)].ItemId;
                        }
                        result.ItemId = textItemId;
                        var owned = await GrantCollectionAsync(player.GID, 0, textItemId, prize.RewardAmount);
                        await _collectionService.InvalidatePlayerBonusCacheAsync(player.GID);
                        var textItem = await _db.Queryable<TextCollectionItemEntity>()
                            .Where(x => x.ItemId == textItemId).FirstAsync();
                        if (textItem != null)
                        {
                            result.SeriesId = textItem.SeriesId;
                            var series = await _db.Queryable<TextCollectionSeriesEntity>()
                                .Where(x => x.SeriesId == textItem.SeriesId).FirstAsync();
                            result.SeriesName = series?.Name;
                        }
                        result.Name = textItem?.Character ?? textItemId ?? "文字图鉴";
                        result.IsNew = owned == 0;
                        result.OwnedCount = owned + prize.RewardAmount;
                        break;
                    }

                case 4: // 图片图鉴
                    {
                        result.RewardType = "image_collection";
                        // rewardTargetId 可能是系列ID或图鉴项ID，优先按系列查找
                        var imgItemId = prize.RewardTargetId!;
                        var imgSeries = await _db.Queryable<ImageCollectionSeriesEntity>()
                            .Where(x => x.SeriesId == prize.RewardTargetId).FirstAsync();
                        if (imgSeries != null)
                        {
                            // 是系列ID，从中随机选一个图鉴项
                            var seriesItems = await _db.Queryable<ImageCollectionItemEntity>()
                                .Where(x => x.SeriesId == prize.RewardTargetId && x.IsEnabled)
                                .ToListAsync();
                            if (seriesItems.Count > 0)
                                imgItemId = seriesItems[Random.Shared.Next(seriesItems.Count)].ItemId;
                        }
                        result.ItemId = imgItemId;
                        var imgOwned = await GrantCollectionAsync(player.GID, 1, imgItemId, prize.RewardAmount);
                        await _collectionService.InvalidatePlayerBonusCacheAsync(player.GID);
                        var imageItem = await _db.Queryable<ImageCollectionItemEntity>()
                            .Where(x => x.ItemId == imgItemId).FirstAsync();
                        if (imageItem != null)
                        {
                            result.SeriesId = imageItem.SeriesId;
                            var series = await _db.Queryable<ImageCollectionSeriesEntity>()
                                .Where(x => x.SeriesId == imageItem.SeriesId).FirstAsync();
                            result.SeriesName = series?.Name;
                        }
                        result.Name = imageItem?.ImageName ?? imgItemId ?? "图片图鉴";
                        result.IsNew = imgOwned == 0;
                        result.OwnedCount = imgOwned + prize.RewardAmount;
                        break;
                    }

                case 5: // 谢谢惠顾
                    result.RewardType = "thanks";
                    result.Name = "谢谢惠顾";
                    break;

                default:
                    result.RewardType = "unknown";
                    result.Name = "未知奖励";
                    break;
            }

            return result;
        }

        /// <summary>
        /// 发放图鉴奖励，返回发放前的拥有数量。
        /// </summary>
        private async Task<int> GrantCollectionAsync(string playerId, int collectionType, string itemId, int amount)
        {
            string seriesId;
            if (collectionType == 0)
            {
                var item = await _db.Queryable<TextCollectionItemEntity>()
                    .Where(x => x.ItemId == itemId).FirstAsync();
                if (item == null) return 0;
                seriesId = item.SeriesId;
            }
            else
            {
                var item = await _db.Queryable<ImageCollectionItemEntity>()
                    .Where(x => x.ItemId == itemId).FirstAsync();
                if (item == null) return 0;
                seriesId = item.SeriesId;
            }

            var existing = await _db.Queryable<PlayerCollectionEntity>()
                .Where(x => x.PlayerId == playerId && x.CollectionType == collectionType
                    && x.SeriesId == seriesId && x.ItemId == itemId)
                .FirstAsync();

            int prevCount = existing?.OwnedCount ?? 0;

            if (existing != null)
            {
                existing.OwnedCount += amount;
                existing.LastGetTime = DateTime.Now;
                await _db.Updateable(existing)
                    .UpdateColumns(x => new { x.OwnedCount, x.LastGetTime })
                    .ExecuteCommandAsync();
            }
            else
            {
                await _db.Insertable(new PlayerCollectionEntity
                {
                    PlayerId = playerId,
                    CollectionType = collectionType,
                    SeriesId = seriesId,
                    ItemId = itemId,
                    OwnedCount = amount,
                    FirstGetTime = DateTime.Now,
                    LastGetTime = DateTime.Now
                }).ExecuteCommandAsync();
            }

            return prevCount;
        }
    }
}
