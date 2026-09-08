using XXX.Entity;
using XXX.Inventory;
using XXX.Player;

namespace XXX.Ranking
{
    /// <summary>
    /// 排行榜奖励管理器
    /// 负责排行榜奖励的发放和管理
    /// </summary>
    public class RankingRewardManager
    {
        /// <summary>
        /// 已发放奖励记录
        /// Key: "PlayerId_RankingId_Season", Value: 已领取的奖励
        /// </summary>
        private static Dictionary<string, HashSet<string>> claimedRewards = [];

        /// <summary>
        /// 发放排名奖励
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="rank">排名</param>
        /// <param name="rewardEntry">奖励配置</param>
        /// <param name="inventory">背包管理器</param>
        /// <returns>发放结果</returns>
        public static RankingRewardResult GrantReward(UserEntity player, string rankingId, int rank,
            RankingRewardEntry rewardEntry, InventoryManager inventory)
        {
            var result = new RankingRewardResult
            {
                Success = true,
                PlayerId = player.GID,
                RankingId = rankingId,
                Rank = rank
            };

            try
            {
                // 发放经验
                if (rewardEntry.Exp > 0)
                {
                    var expResult = PlayerManager.AddExp(player, rewardEntry.Exp);
                    if (expResult.Success)
                    {
                        result.GrantedExp = rewardEntry.Exp;
                        result.Messages.Add($"获得 {rewardEntry.Exp} 经验");

                        if (expResult.LeveledUp)
                        {
                            result.Messages.Add($"升级！从 Lv.{expResult.OldLevel} 升到 Lv.{expResult.NewLevel}");
                        }
                    }
                }

                // 发放金币
                if (rewardEntry.Gold > 0)
                {
                    if (PlayerManager.AddGold(player, rewardEntry.Gold, "排行榜奖励"))
                    {
                        result.GrantedGold = rewardEntry.Gold;
                        result.Messages.Add($"获得 {rewardEntry.Gold} 金币");
                    }
                }

                // 发放灵石
                if (rewardEntry.SpiritStone > 0)
                {
                    if (PlayerManager.AddSpiritStone(player, rewardEntry.SpiritStone, "排行榜奖励"))
                    {
                        result.GrantedSpiritStone = rewardEntry.SpiritStone;
                        result.Messages.Add($"获得 {rewardEntry.SpiritStone} 灵石");
                    }
                }

                // 记录称号
                if (!string.IsNullOrEmpty(rewardEntry.Title))
                {
                    result.GrantedTitle = rewardEntry.Title;
                    result.Messages.Add($"获得称号：{rewardEntry.Title}");
                }

                // 发放道具
                if (rewardEntry.Items != null && rewardEntry.Items.Count > 0)
                {
                    foreach (var item in rewardEntry.Items)
                    {
                        // TODO: 需要InventoryManager.AddItem
                        // if (inventory.AddItem(item.Key, item.Value))
                        // {
                        //     string itemName = GetItemName(item.Key);
                        //     result.GrantedItems.Add(item.Key);
                        //     result.Messages.Add($"获得 {itemName} x{item.Value}");
                        // }
                    }
                }

                // 发放装备
                if (rewardEntry.EquipmentIds != null && rewardEntry.EquipmentIds.Count > 0)
                {
                    foreach (var equipId in rewardEntry.EquipmentIds)
                    {
                        if (GameData.EquipmentTemplates.ContainsKey(equipId))
                        {
                            var template = GameData.EquipmentTemplates[equipId];
                            var equipment = new EquipmentInstance
                            {
                                InstanceId = Guid.NewGuid().ToString(),
                                Template = template,
                                EnhanceLevel = 0,
                                RerolledAttrs = [],
                                RerollCount = 0
                            };

                            // TODO: 需要InventoryManager.AddEquipment
                            // if (inventory.AddEquipment(equipment))
                            // {
                            //     string equipName = template.Name;
                            //     result.GrantedEquipment.Add(equipment.InstanceId);
                            //     result.Messages.Add($"获得 {equipName}");
                            // }
                        }
                    }
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Messages.Add($"发放奖励失败：{ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 批量发放赛季奖励
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="snapshot">快照</param>
        /// <param name="rankingConfig">排行榜配置</param>
        /// <returns>发放结果列表</returns>
        public static List<RankingRewardResult> GrantSeasonRewards(string rankingId,
            RankingSnapshot snapshot, RankingConfig rankingConfig)
        {
            var results = new List<RankingRewardResult>();

            // TODO: 需要获取玩家列表和背包管理器
            // 这里简化处理，只返回空列表
            // 实际使用时需要：
            // 1. 遍历快照中的所有条目
            // 2. 获取每个玩家的实体和背包管理器
            // 3. 调用 GrantReward 发放奖励
            // 4. 记录已发放的奖励

            return results;
        }

        /// <summary>
        /// 玩家领取排行榜奖励
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="rank">排名</param>
        /// <param name="inventory">背包管理器</param>
        /// <returns>领取结果</returns>
        public static RankingRewardResult ClaimReward(UserEntity player, string rankingId, int rank, InventoryManager inventory)
        {
            var result = new RankingRewardResult
            {
                Success = false,
                PlayerId = player.GID,
                RankingId = rankingId,
                Rank = rank
            };

            // 检查是否已领取
            var rankingConfig = RankingManager.GetRankingConfig(rankingId);
            if (rankingConfig == null)
            {
                result.Messages.Add("排行榜不存在");
                return result;
            }

            var seasonKey = $"{player.GID}_{rankingId}_S{rankingConfig.CurrentSeason}";
            var rewardKey = $"{seasonKey}_{rank}";

            if (HasClaimedReward(player.GID, rankingId, rankingConfig.CurrentSeason, rank))
            {
                result.Messages.Add("该奖励已领取");
                return result;
            }

            // 获取奖励配置
            var rewardEntry = rankingConfig.GetRewardByRank(rank);
            if (rewardEntry == null)
            {
                result.Messages.Add("该排名无奖励");
                return result;
            }

            // 发放奖励
            result = GrantReward(player, rankingId, rank, rewardEntry, inventory);

            // 标记为已领取
            if (result.Success)
            {
                MarkRewardAsClaimed(player.GID, rankingId, rankingConfig.CurrentSeason, rank);
            }

            return result;
        }

        /// <summary>
        /// 检查玩家是否已领取奖励
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="season">赛季编号</param>
        /// <param name="rank">排名</param>
        /// <returns>是否已领取</returns>
        public static bool HasClaimedReward(string playerId, string rankingId, int season, int rank)
        {
            var seasonKey = $"{playerId}_{rankingId}_S{season}";

            if (!claimedRewards.ContainsKey(seasonKey))
            {
                return false;
            }

            var rewardKey = $"{seasonKey}_{rank}";
            return claimedRewards[seasonKey].Contains(rewardKey);
        }

        /// <summary>
        /// 标记奖励为已领取
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="season">赛季编号</param>
        /// <param name="rank">排名</param>
        private static void MarkRewardAsClaimed(string playerId, string rankingId, int season, int rank)
        {
            var seasonKey = $"{playerId}_{rankingId}_S{season}";

            if (!claimedRewards.ContainsKey(seasonKey))
            {
                claimedRewards[seasonKey] = [];
            }

            var rewardKey = $"{seasonKey}_{rank}";
            claimedRewards[seasonKey].Add(rewardKey);
        }

        /// <summary>
        /// 清空已领取奖励记录
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="season">赛季编号</param>
        public static void ClearClaimedRewards(string playerId, string rankingId, int season)
        {
            var seasonKey = $"{playerId}_{rankingId}_S{season}";

            if (claimedRewards.ContainsKey(seasonKey))
            {
                claimedRewards.Remove(seasonKey);
            }
        }

        /// <summary>
        /// 获取玩家可领取的奖励列表
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>可领取的奖励列表</returns>
        public static List<ClaimableReward> GetClaimableRewards(string playerId, string rankingId)
        {
            var result = new List<ClaimableReward>();

            var rankingConfig = RankingManager.GetRankingConfig(rankingId);
            if (rankingConfig == null)
            {
                return result;
            }

            // 获取玩家当前排名
            var rank = RankingManager.GetPlayerRank(playerId, rankingId);
            if (rank == null || rank.Value == 0)
            {
                return result;
            }

            // 检查该排名是否有奖励
            var rewardEntry = rankingConfig.GetRewardByRank(rank.Value);
            if (rewardEntry == null)
            {
                return result;
            }

            // 检查是否已领取
            if (!HasClaimedReward(playerId, rankingId, rankingConfig.CurrentSeason, rank.Value))
            {
                result.Add(new ClaimableReward
                {
                    RankingId = rankingId,
                    RankingName = rankingConfig.RankingName,
                    Season = rankingConfig.CurrentSeason,
                    Rank = rank.Value,
                    RewardEntry = rewardEntry
                });
            }

            return result;
        }

        /// <summary>
        /// 获取道具名称
        /// </summary>
        /// <param name="itemId">道具ID</param>
        /// <returns>道具名称</returns>
        private static string GetItemName(string itemId)
        {
            if (GameData.Items.ContainsKey(itemId))
            {
                return GameData.Items[itemId].Name;
            }
            return $"未知道具({itemId})";
        }
    }

    /// <summary>
    /// 排行榜奖励结果类
    /// </summary>
    public class RankingRewardResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息列表
        /// </summary>
        public List<string> Messages { get; set; } = [];

        /// <summary>
        /// 玩家ID
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 排行榜ID
        /// </summary>
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 实际发放的经验
        /// </summary>
        public long GrantedExp { get; set; }

        /// <summary>
        /// 实际发放的金币
        /// </summary>
        public long GrantedGold { get; set; }

        /// <summary>
        /// 实际发放的灵石
        /// </summary>
        public long GrantedSpiritStone { get; set; }

        /// <summary>
        /// 实际发放的称号
        /// </summary>
        public string GrantedTitle { get; set; } = string.Empty;

        /// <summary>
        /// 实际发放的道具ID列表
        /// </summary>
        public List<string> GrantedItems { get; set; } = [];

        /// <summary>
        /// 实际发放的装备实例ID列表
        /// </summary>
        public List<string> GrantedEquipment { get; set; } = [];

        /// <summary>
        /// 获取完整的消息文本
        /// </summary>
        /// <returns>消息文本</returns>
        public string GetFullMessage()
        {
            return string.Join("\n", Messages);
        }
    }

    /// <summary>
    /// 可领取的奖励类
    /// </summary>
    public class ClaimableReward
    {
        /// <summary>
        /// 排行榜ID
        /// </summary>
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 排行榜名称
        /// </summary>
        public string RankingName { get; set; } = string.Empty;

        /// <summary>
        /// 赛季编号
        /// </summary>
        public int Season { get; set; }

        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 奖励配置
        /// </summary>
        public RankingRewardEntry RewardEntry { get; set; } = new RankingRewardEntry();

        /// <summary>
        /// 获取奖励摘要
        /// </summary>
        /// <returns>摘要文本</returns>
        public string GetSummary()
        {
            return $"{RankingName} 第{Rank}名 - {RewardEntry.GetRewardSummary()}";
        }
    }
}
