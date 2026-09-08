using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;
using XXX.Player;

namespace XXX.Application.Services
{
    public class PlayerRewardService : IPlayerRewardService
    {
        private readonly DbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository<UserEntity> _userRepository;
        private readonly ILogger<PlayerRewardService> _logger;

        public PlayerRewardService(
            DbContext dbContext,
            IServiceProvider serviceProvider,
            IRepository<UserEntity> userRepository,
            ILogger<PlayerRewardService> logger)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<List<RewardItemDto>> GrantRewardsAsync(
            string playerId,
            IEnumerable<RewardGrantItemDto> rewards,
            string source)
        {
            var rewardList = rewards?
                .Where(reward => reward != null && reward.Count > 0)
                .ToList() ?? [];

            if (rewardList.Count == 0)
            {
                return [];
            }

            var player = await _userRepository.GetByIdAsync(playerId);
            if (player == null || player.IsDeleted)
            {
                throw new InvalidOperationException("玩家不存在，无法发放奖励。");
            }

            var result = new List<RewardItemDto>();
            var playerChanged = false;

            foreach (var reward in rewardList)
            {
                switch (NormalizeRewardType(reward.Type))
                {
                    case RewardTypes.Gold:
                        player.Gold += reward.Count;
                        player.TotalGoldEarned += reward.Count;
                        playerChanged = true;
                        result.Add(new RewardItemDto
                        {
                            Type = RewardTypes.Gold,
                            Name = "金币",
                            Count = reward.Count,
                            Description = reward.Description
                        });
                        break;

                    case RewardTypes.Exp:
                        var expResult = PlayerManager.AddExp(player, reward.Count);
                        if (!expResult.Success)
                        {
                            throw new InvalidOperationException($"经验奖励发放失败：{expResult.Message}");
                        }

                        player.XExp = PlayerManager.GetRequiredExp(player);
                        playerChanged = true;
                        result.Add(new RewardItemDto
                        {
                            Type = RewardTypes.Exp,
                            Name = "修为",
                            Count = reward.Count,
                            Description = reward.Description
                        });
                        break;

                    case RewardTypes.SpiritStone:
                        player.SpiritStone += reward.Count;
                        playerChanged = true;
                        result.Add(new RewardItemDto
                        {
                            Type = RewardTypes.SpiritStone,
                            Name = "灵石",
                            Count = reward.Count,
                            Description = reward.Description
                        });
                        break;

                    case RewardTypes.Honor:
                        player.Honor += checked((int)reward.Count);
                        playerChanged = true;
                        result.Add(new RewardItemDto
                        {
                            Type = RewardTypes.Honor,
                            Name = "荣誉",
                            Count = reward.Count,
                            Description = reward.Description
                        });
                        break;

                    case RewardTypes.GuildContribution:
                        player.GuildContribution += checked((int)reward.Count);
                        playerChanged = true;
                        result.Add(new RewardItemDto
                        {
                            Type = RewardTypes.GuildContribution,
                            Name = "公会贡献",
                            Count = reward.Count,
                            Description = reward.Description
                        });
                        break;

                    case RewardTypes.Item:
                        if (string.IsNullOrWhiteSpace(reward.ItemId))
                        {
                            throw new InvalidOperationException("道具奖励缺少 ItemId，无法发放。");
                        }

                        var grantedItem = await InventoryItemGrantHelper.AddOrMergeAsync(
                            _dbContext.Db,
                            playerId,
                            reward.ItemId,
                            checked((int)reward.Count),
                            "背包已满，无法领取新的道具奖励。");

                        await NotifyCollectItemQuestAsync(playerId, reward.ItemId, checked((int)reward.Count));
                        result.Add(BuildItemRewardDto(reward, grantedItem.IsLocked));
                        break;

                    default:
                        throw new InvalidOperationException($"不支持的奖励类型：{reward.Type}");
                }
            }

            if (playerChanged)
            {
                player.LastUpdateTime = DateTime.Now;
                await _userRepository.UpdateAsync(player);
            }

            _logger.LogInformation(
                "玩家奖励发放完成。PlayerId={PlayerId}, Source={Source}",
                playerId,
                source);

            return result;
        }

        private async Task NotifyCollectItemQuestAsync(string playerId, string itemId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(itemId) || quantity <= 0)
            {
                return;
            }

            if (_serviceProvider.GetService(typeof(IQuestService)) is not IQuestService questService)
            {
                return;
            }

            await questService.RecordObjectiveEventAsync(playerId, new XXX.Quest.QuestObjectiveEvent
            {
                ObjectiveType = XXX.Quest.ObjectiveType.CollectItem,
                TargetId = itemId,
                Delta = quantity
            });
        }

        private static string NormalizeRewardType(string? rewardType)
        {
            return (rewardType ?? string.Empty).Trim() switch
            {
                RewardTypes.Gold => RewardTypes.Gold,
                RewardTypes.Exp => RewardTypes.Exp,
                RewardTypes.SpiritStone => RewardTypes.SpiritStone,
                RewardTypes.Honor => RewardTypes.Honor,
                RewardTypes.GuildContribution => RewardTypes.GuildContribution,
                RewardTypes.Item => RewardTypes.Item,
                _ => string.Empty
            };
        }

        private static RewardItemDto BuildItemRewardDto(RewardGrantItemDto reward, bool isLocked)
        {
            if (!string.IsNullOrWhiteSpace(reward.ItemId) &&
                XXX.GameData.Items.TryGetValue(reward.ItemId, out var itemTemplate))
            {
                return new RewardItemDto
                {
                    Type = RewardTypes.Item,
                    Name = itemTemplate.Name,
                    Count = reward.Count,
                    ItemId = reward.ItemId,
                    IsLocked = isLocked,
                    Description = reward.Description ?? itemTemplate.Description
                };
            }

            return new RewardItemDto
            {
                Type = RewardTypes.Item,
                Name = reward.ItemId ?? "未知道具",
                Count = reward.Count,
                ItemId = reward.ItemId,
                IsLocked = isLocked,
                Description = reward.Description
            };
        }
    }
}