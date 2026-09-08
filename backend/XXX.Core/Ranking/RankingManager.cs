namespace XXX.Ranking
{
    /// <summary>
    /// 排行榜管理器
    /// 负责排行榜的初始化、更新、查询等核心功能
    ///
    /// 使用说明：
    /// 1. 游戏启动时调用 Initialize() 初始化排行榜系统
    /// 2. 使用 GetRanking() 获取排行榜数据
    /// 3. 使用 UpdatePlayerScore() 更新玩家分数
    /// 4. 使用 GetPlayerRank() 查询玩家排名
    /// </summary>
    public static class RankingManager
    {
        /// <summary>
        /// 所有排行榜配置
        /// Key: RankingId, Value: RankingConfig
        /// </summary>
        private static Dictionary<string, RankingConfig> rankingConfigs = [];

        /// <summary>
        /// 当前排行榜数据
        /// Key: RankingId, Value: List<RankingEntry>
        /// </summary>
        private static Dictionary<string, List<RankingEntry>> currentRankings = [];

        /// <summary>
        /// 上次刷新时间
        /// Key: RankingId, Value: 上次刷新时间
        /// </summary>
        private static Dictionary<string, DateTime> lastRefreshTime = [];

        #region 初始化

        /// <summary>
        /// 初始化排行榜系统
        /// 创建所有预设排行榜
        /// </summary>
        public static void Initialize()
        {
            rankingConfigs.Clear();
            currentRankings.Clear();
            lastRefreshTime.Clear();

            // 创建预设排行榜
            CreateLevelRanking();
            CreateArenaRanking();
            CreateAchievementRanking();
            CreateWealthRanking();

            // 初始化所有排行榜
            foreach (var config in rankingConfigs.Values)
            {
                if (!currentRankings.ContainsKey(config.RankingId))
                {
                    currentRankings[config.RankingId] = [];
                }
                lastRefreshTime[config.RankingId] = DateTime.Now;
            }
        }

        /// <summary>
        /// 创建等级排行榜
        /// </summary>
        private static void CreateLevelRanking()
        {
            var ranking = new RankingConfig
            {
                RankingId = "ranking_level",
                RankingName = "等级排行榜",
                RankingType = RankingType.Level,
                Description = "按玩家等级排序",
                MaxSize = 100,
                UpdateInterval = 60,
                SeasonEnabled = false,
                SortOrder = 1
            };

            // 设置奖励
            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 1,
                MaxRank = 1,
                RewardTitle = "等级之王",
                Gold = 10000,
                SpiritStone =500,
                Title = "等级王者"
            });

            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 2,
                MaxRank = 3,
                RewardTitle = "等级精英",
                Gold = 5000,
                SpiritStone =200
            });

            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 4,
                MaxRank = 10,
                RewardTitle = "等级高手",
                Gold = 2000,
                SpiritStone =50
            });

            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 11,
                MaxRank = 100,
                RewardTitle = "等级达人",
                Gold = 500,
                SpiritStone =10
            });

            rankingConfigs[ranking.RankingId] = ranking;
        }

        /// <summary>
        /// 创建竞技场排行榜
        /// </summary>
        private static void CreateArenaRanking()
        {
            var ranking = new RankingConfig
            {
                RankingId = "ranking_arena",
                RankingName = "竞技场排行榜",
                RankingType = RankingType.Arena,
                Description = "按竞技场积分排序",
                MaxSize = 50,
                UpdateInterval = 30,
                SeasonEnabled = true,
                SeasonDuration = 30,
                CurrentSeason = 1,
                SeasonStartTime = DateTime.Now,
                SortOrder = 3
            };

            // 设置奖励
            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 1,
                MaxRank = 1,
                RewardTitle = "竞技冠军",
                Gold = 50000,
                SpiritStone =2000,
                Title = "竞技场冠军"
            });

            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 2,
                MaxRank = 3,
                RewardTitle = "竞技亚军",
                Gold = 20000,
                SpiritStone =1000
            });

            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 4,
                MaxRank = 10,
                RewardTitle = "竞技高手",
                Gold = 10000,
                SpiritStone =500
            });

            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 11,
                MaxRank = 50,
                RewardTitle = "竞技达人",
                Gold = 2000,
                SpiritStone =100
            });

            rankingConfigs[ranking.RankingId] = ranking;
        }

        /// <summary>
        /// 创建成就排行榜
        /// </summary>
        private static void CreateAchievementRanking()
        {
            var ranking = new RankingConfig
            {
                RankingId = "ranking_achievement",
                RankingName = "成就排行榜",
                RankingType = RankingType.Achievement,
                Description = "按成就点数排序",
                MaxSize = 100,
                UpdateInterval = 60,
                SeasonEnabled = false,
                SortOrder = 4
            };

            // 设置奖励
            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 1,
                MaxRank = 1,
                RewardTitle = "成就大师",
                Gold = 20000,
                SpiritStone =1000,
                Title = "成就之王"
            });

            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 2,
                MaxRank = 10,
                RewardTitle = "成就精英",
                Gold = 5000,
                SpiritStone =200
            });

            rankingConfigs[ranking.RankingId] = ranking;
        }

        /// <summary>
        /// 创建财富排行榜
        /// </summary>
        private static void CreateWealthRanking()
        {
            var ranking = new RankingConfig
            {
                RankingId = "ranking_wealth",
                RankingName = "财富排行榜",
                RankingType = RankingType.Wealth,
                Description = "按金币数量排序",
                MaxSize = 100,
                UpdateInterval = 60,
                SeasonEnabled = false,
                SortOrder = 5
            };

            // 设置奖励
            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 1,
                MaxRank = 1,
                RewardTitle = "首富",
                SpiritStone =500,
                Title = "大富翁"
            });

            ranking.Rewards.Add(new RankingRewardEntry
            {
                MinRank = 2,
                MaxRank = 10,
                RewardTitle = "富商",
                SpiritStone =100
            });

            rankingConfigs[ranking.RankingId] = ranking;
        }

        #endregion

        #region 排行榜查询

        /// <summary>
        /// 获取排行榜配置
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>排行榜配置</returns>
        public static RankingConfig? GetRankingConfig(string rankingId)
        {
            return rankingConfigs.ContainsKey(rankingId) ? rankingConfigs[rankingId] : null;
        }

        /// <summary>
        /// 获取所有排行榜配置
        /// </summary>
        /// <returns>排行榜配置列表</returns>
        public static List<RankingConfig> GetAllRankings()
        {
            return rankingConfigs.Values.OrderBy(r => r.SortOrder).ToList();
        }

        /// <summary>
        /// 获取排行榜数据
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>排行榜条目列表</returns>
        public static List<RankingEntry> GetRanking(string rankingId)
        {
            if (!currentRankings.ContainsKey(rankingId))
            {
                return [];
            }

            return [.. currentRankings[rankingId]];
        }

        /// <summary>
        /// 获取前N名
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="count">数量</param>
        /// <returns>前N名列表</returns>
        public static List<RankingEntry> GetTopN(string rankingId, int count)
        {
            if (!currentRankings.ContainsKey(rankingId))
            {
                return [];
            }

            return currentRankings[rankingId].Take(count).ToList();
        }

        /// <summary>
        /// 获取指定范围的排名
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="startRank">起始排名</param>
        /// <param name="endRank">结束排名</param>
        /// <returns>排名范围列表</returns>
        public static List<RankingEntry> GetRankingRange(string rankingId, int startRank, int endRank)
        {
            if (!currentRankings.ContainsKey(rankingId))
            {
                return [];
            }

            return currentRankings[rankingId]
                .Where(e => e.Rank >= startRank && e.Rank <= endRank)
                .ToList();
        }

        /// <summary>
        /// 获取玩家排名
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>排名，如果不在排行榜中则返回null</returns>
        public static int? GetPlayerRank(string playerId, string rankingId)
        {
            if (!currentRankings.ContainsKey(rankingId))
            {
                return null;
            }

            var entry = currentRankings[rankingId].FirstOrDefault(e => e.PlayerId == playerId);
            return entry?.Rank;
        }

        /// <summary>
        /// 获取玩家在所有排行榜中的排名
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>排名字典 Key: RankingId, Value: Rank</returns>
        public static Dictionary<string, int> GetAllPlayerRanks(string playerId)
        {
            var result = new Dictionary<string, int>();

            foreach (var kvp in currentRankings)
            {
                var entry = kvp.Value.FirstOrDefault(e => e.PlayerId == playerId);
                if (entry != null)
                {
                    result[kvp.Key] = entry.Rank;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取玩家周边排名（玩家排名前后的玩家）
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="range">范围（前后各多少名）</param>
        /// <returns>周边排名列表</returns>
        public static List<RankingEntry> GetPlayerNearbyRank(string playerId, string rankingId, int range = 5)
        {
            var ranking = GetRanking(rankingId);
            var playerEntry = ranking.FirstOrDefault(e => e.PlayerId == playerId);

            if (playerEntry == null)
            {
                return [];
            }

            int minRank = Math.Max(1, playerEntry.Rank - range);
            int maxRank = Math.Min(ranking.Count, playerEntry.Rank + range);

            return ranking.Where(e => e.Rank >= minRank && e.Rank <= maxRank).ToList();
        }

        #endregion

        #region 排名更新

        /// <summary>
        /// 更新玩家分数
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="score">分数</param>
        /// <returns>新排名，如果不在排行榜中则返回null</returns>
        public static int? UpdatePlayerScore(string playerId, string rankingId, long score)
        {
            if (!currentRankings.ContainsKey(rankingId))
            {
                return null;
            }

            var config = GetRankingConfig(rankingId);
            if (config == null)
            {
                return null;
            }

            var ranking = currentRankings[rankingId];
            var entry = ranking.FirstOrDefault(e => e.PlayerId == playerId);

            if (entry != null)
            {
                // 更新现有条目
                entry.UpdateScore(score);
            }
            else
            {
                // 创建新条目
                entry = new RankingEntry
                {
                    PlayerId = playerId,
                    Score = score,
                    Rank = ranking.Count + 1,
                    LastRank = ranking.Count + 1
                };

                // TODO: 获取玩家名称和等级
                // entry.PlayerName = player.Name;
                // entry.Level = GetLevel(player);

                ranking.Add(entry);
            }

            // 重新排序
            RefreshRanking(rankingId);

            // 返回新排名
            entry = ranking.FirstOrDefault(e => e.PlayerId == playerId);
            return entry?.Rank;
        }

        /// <summary>
        /// 刷新排行榜（重新排序）
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        public static void RefreshRanking(string rankingId)
        {
            if (!currentRankings.ContainsKey(rankingId))
            {
                return;
            }

            var config = GetRankingConfig(rankingId);
            if (config == null)
            {
                return;
            }

            var ranking = currentRankings[rankingId];

            // 保存旧排名
            var oldRanks = ranking.ToDictionary(e => e.PlayerId, e => e.Rank);

            // 按分数降序排序
            var sorted = ranking.OrderByDescending(e => e.Score).ToList();

            // 更新排名（限制最大数量）
            for (int i = 0; i < sorted.Count && i < config.MaxSize; i++)
            {
                sorted[i].UpdateRank(i + 1);
            }

            // 移除超出最大数量的条目
            if (sorted.Count > config.MaxSize)
            {
                sorted.RemoveRange(config.MaxSize, sorted.Count - config.MaxSize);
            }

            currentRankings[rankingId] = sorted;
            lastRefreshTime[rankingId] = DateTime.Now;
        }

        /// <summary>
        /// 自动刷新所有排行榜
        /// </summary>
        public static void AutoRefreshAll()
        {
            foreach (var rankingId in rankingConfigs.Keys)
            {
                if (NeedsRefresh(rankingId))
                {
                    RefreshRanking(rankingId);
                }
            }

            // 检查赛季
            CheckSeasons();
        }

        /// <summary>
        /// 检查排行榜是否需要刷新
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>是否需要刷新</returns>
        public static bool NeedsRefresh(string rankingId)
        {
            var config = GetRankingConfig(rankingId);
            if (config == null)
            {
                return false;
            }

            if (config.UpdateInterval == 0)
            {
                return false; // 实时更新，不需要自动刷新
            }

            if (!lastRefreshTime.ContainsKey(rankingId))
            {
                return true;
            }

            var nextRefresh = lastRefreshTime[rankingId].AddMinutes(config.UpdateInterval);
            return DateTime.Now >= nextRefresh;
        }

        #endregion

        #region 赛季管理

        /// <summary>
        /// 检查所有排行榜的赛季状态
        /// </summary>
        public static void CheckSeasons()
        {
            foreach (var config in rankingConfigs.Values)
            {
                if (config.NeedsNewSeason())
                {
                    // 创建赛季结束快照
                    if (currentRankings.ContainsKey(config.RankingId))
                    {
                        RankingHistoryManager.CreateSnapshot(
                            config.RankingId,
                            currentRankings[config.RankingId],
                            SnapshotType.SeasonEnd,
                            $"赛季{config.CurrentSeason}结束",
                            config.CurrentSeason
                        );
                    }

                    // 开始新赛季
                    config.StartNewSeason();

                    // 清空排行榜（可选）
                    // currentRankings[config.RankingId].Clear();
                }
            }
        }

        /// <summary>
        /// 手动开启新赛季
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>是否成功</returns>
        public static bool StartNewSeason(string rankingId)
        {
            var config = GetRankingConfig(rankingId);
            if (config == null || !config.SeasonEnabled)
            {
                return false;
            }

            // 创建赛季结束快照
            if (currentRankings.ContainsKey(rankingId))
            {
                RankingHistoryManager.CreateSnapshot(
                    rankingId,
                    currentRankings[rankingId],
                    SnapshotType.SeasonEnd,
                    $"赛季{config.CurrentSeason}结束",
                    config.CurrentSeason
                );
            }

            config.StartNewSeason();
            return true;
        }

        #endregion

        #region 注册自定义排行榜

        /// <summary>
        /// 注册新的排行榜
        /// </summary>
        /// <param name="config">排行榜配置</param>
        /// <returns>是否成功</returns>
        public static bool RegisterRanking(RankingConfig config)
        {
            if (rankingConfigs.ContainsKey(config.RankingId))
            {
                return false;
            }

            rankingConfigs[config.RankingId] = config;
            currentRankings[config.RankingId] = [];
            lastRefreshTime[config.RankingId] = DateTime.Now;

            return true;
        }

        /// <summary>
        /// 注销排行榜
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>是否成功</returns>
        public static bool UnregisterRanking(string rankingId)
        {
            if (!rankingConfigs.ContainsKey(rankingId))
            {
                return false;
            }

            rankingConfigs.Remove(rankingId);
            currentRankings.Remove(rankingId);
            lastRefreshTime.Remove(rankingId);

            return true;
        }

        #endregion

        #region 快照相关

        /// <summary>
        /// 创建当前排行榜的快照
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="type">快照类型</param>
        /// <param name="description">描述</param>
        /// <returns>快照ID</returns>
        public static string? CreateSnapshot(string rankingId, SnapshotType type, string description)
        {
            var config = GetRankingConfig(rankingId);
            if (config == null)
            {
                return null;
            }

            if (!currentRankings.ContainsKey(rankingId))
            {
                return null;
            }

            return RankingHistoryManager.CreateSnapshot(
                rankingId,
                currentRankings[rankingId],
                type,
                description,
                config.CurrentSeason
            );
        }

        /// <summary>
        /// 创建每日快照
        /// </summary>
        public static void CreateDailySnapshots()
        {
            foreach (var rankingId in rankingConfigs.Keys)
            {
                CreateSnapshot(rankingId, SnapshotType.Daily, "每日快照");
            }
        }

        /// <summary>
        /// 创建每周快照
        /// </summary>
        public static void CreateWeeklySnapshots()
        {
            foreach (var rankingId in rankingConfigs.Keys)
            {
                CreateSnapshot(rankingId, SnapshotType.Weekly, "每周快照");
            }
        }

        #endregion

        #region 统计信息

        /// <summary>
        /// 获取排行榜统计信息
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>统计信息</returns>
        public static RankingStats GetRankingStats(string rankingId)
        {
            var config = GetRankingConfig(rankingId);
            var ranking = GetRanking(rankingId);

            return new RankingStats
            {
                RankingId = rankingId,
                RankingName = config?.RankingName ?? string.Empty,
                TotalEntries = ranking.Count,
                MaxSize = config?.MaxSize ?? 0,
                LastRefreshTime = lastRefreshTime.ContainsKey(rankingId) ? lastRefreshTime[rankingId] : DateTime.Now,
                SeasonEnabled = config?.SeasonEnabled ?? false,
                CurrentSeason = config?.CurrentSeason ?? 1,
                SeasonDaysRemaining = config?.GetSeasonDaysRemaining() ?? 0
            };
        }

        #endregion
    }

    /// <summary>
    /// 排行榜统计信息类
    /// </summary>
    public class RankingStats
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
        /// 总条目数
        /// </summary>
        public int TotalEntries { get; set; }

        /// <summary>
        /// 最大容量
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// 上次刷新时间
        /// </summary>
        public DateTime LastRefreshTime { get; set; }

        /// <summary>
        /// 是否启用赛季
        /// </summary>
        public bool SeasonEnabled { get; set; }

        /// <summary>
        /// 当前赛季
        /// </summary>
        public int CurrentSeason { get; set; }

        /// <summary>
        /// 赛季剩余天数
        /// </summary>
        public int SeasonDaysRemaining { get; set; }

        /// <summary>
        /// 获取统计摘要文本
        /// </summary>
        /// <returns>摘要文本</returns>
        public string GetSummaryText()
        {
            var text = $"{RankingName}\n";
            text += $"人数：{TotalEntries}/{MaxSize}\n";
            text += $"上次刷新：{LastRefreshTime:yyyy-MM-dd HH:mm:ss}";

            if (SeasonEnabled)
            {
                text += $"\n赛季：{CurrentSeason} (剩余{SeasonDaysRemaining}天)";
            }

            return text;
        }
    }
}
