namespace XXX.Ranking
{
    /// <summary>
    /// 排行榜快照类
    /// 用于保存排行榜的历史状态
    /// </summary>
    public class RankingSnapshot
    {
        /// <summary>
        /// 快照唯一ID
        /// </summary>
        public string SnapshotId { get; set; } = string.Empty;

        /// <summary>
        /// 排行榜ID
        /// </summary>
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 排行榜名称（快照时保存）
        /// </summary>
        public string RankingName { get; set; } = string.Empty;

        /// <summary>
        /// 快照时间
        /// </summary>
        public DateTime SnapshotTime { get; set; }

        /// <summary>
        /// 赛季编号
        /// </summary>
        public int SeasonNumber { get; set; }

        /// <summary>
        /// 快照时的排行榜数据
        /// 按排名排序
        /// </summary>
        public List<RankingEntry> Entries { get; set; } = [];

        /// <summary>
        /// 快照描述
        /// 例如："赛季结束"、"每日快照"等
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 快照类型
        /// </summary>
        public SnapshotType Type { get; set; }

        /// <summary>
        /// 获取指定排名的条目
        /// </summary>
        /// <param name="rank">排名</param>
        /// <returns>排行榜条目，如果不存在则返回null</returns>
        public RankingEntry? GetEntryByRank(int rank)
        {
            return Entries.FirstOrDefault(e => e.Rank == rank);
        }

        /// <summary>
        /// 获取指定玩家的条目
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>排行榜条目，如果不存在则返回null</returns>
        public RankingEntry? GetEntryByPlayer(string playerId)
        {
            return Entries.FirstOrDefault(e => e.PlayerId == playerId);
        }

        /// <summary>
        /// 获取前N名
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>前N名列表</returns>
        public List<RankingEntry> GetTopN(int count)
        {
            return Entries.Take(count).ToList();
        }

        /// <summary>
        /// 获取快照摘要
        /// </summary>
        /// <returns>摘要文本</returns>
        public string GetSummary()
        {
            return $"[{SnapshotTime:yyyy-MM-dd HH:mm}] {RankingName} - {Description} (赛季{SeasonNumber})";
        }

        /// <summary>
        /// 获取玩家排名变化
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="previousSnapshot">上一个快照</param>
        /// <returns>排名变化，null表示不在快照中</returns>
        public int? GetPlayerRankChange(string playerId, RankingSnapshot previousSnapshot)
        {
            var currentEntry = GetEntryByPlayer(playerId);
            var previousEntry = previousSnapshot?.GetEntryByPlayer(playerId);

            if (currentEntry == null || previousEntry == null)
            {
                return null;
            }

            return previousEntry.Rank - currentEntry.Rank;
        }
    }

    /// <summary>
    /// 快照类型枚举
    /// </summary>
    public enum SnapshotType
    {
        /// <summary>
        /// 每日快照 - 每天自动创建
        /// </summary>
        Daily,

        /// <summary>
        /// 每周快照 - 每周自动创建
        /// </summary>
        Weekly,

        /// <summary>
        /// 赛季快照 - 赛季结束时创建
        /// </summary>
        SeasonEnd,

        /// <summary>
        /// 手动快照 - 手动创建
        /// </summary>
        Manual
    }

    /// <summary>
    /// 玩家排名历史类
    /// 记录玩家在排行榜上的历史表现
    /// </summary>
    public class PlayerRankHistory
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 排行榜ID
        /// </summary>
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 历史排名记录
        /// Key: 时间, Value: 排名
        /// </summary>
        private SortedDictionary<DateTime, int> rankHistory = [];

        /// <summary>
        /// 最高排名
        /// </summary>
        public int BestRank { get; private set; } = int.MaxValue;

        /// <summary>
        /// 最低排名
        /// </summary>
        public int WorstRank { get; private set; } = 0;

        /// <summary>
        /// 平均排名
        /// </summary>
        public float AverageRank { get; private set; }

        /// <summary>
        /// 添加排名记录
        /// </summary>
        /// <param name="rank">排名</param>
        /// <param name="time">时间</param>
        public void AddRecord(int rank, DateTime time)
        {
            rankHistory[time] = rank;

            // 更新统计数据
            if (rank < BestRank)
            {
                BestRank = rank;
            }

            if (rank > WorstRank)
            {
                WorstRank = rank;
            }

            CalculateAverageRank();
        }

        /// <summary>
        /// 获取指定时间的排名
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>排名，如果不存在则返回null</returns>
        public int? GetRankAt(DateTime time)
        {
            // 查找最接近的时间点
            var closestTime = rankHistory.Keys.FirstOrDefault(t => t >= time);

            if (closestTime == default)
            {
                if (rankHistory.Count > 0)
                {
                    return rankHistory.Values.Last();
                }
                return null;
            }

            return rankHistory[closestTime];
        }

        /// <summary>
        /// 获取排名趋势（最近N次记录）
        /// </summary>
        /// <param name="count">记录数量</param>
        /// <returns>排名列表</returns>
        public List<int> GetTrend(int count)
        {
            return rankHistory.Values.TakeLast(count).ToList();
        }

        /// <summary>
        /// 获取最近N天的排名变化
        /// </summary>
        /// <param name="days">天数</param>
        /// <returns>排名变化数据</returns>
        public List<RankChangeData> GetRankChanges(int days)
        {
            var result = new List<RankChangeData>();
            var cutoffDate = DateTime.Now.AddDays(-days);

            var recentRecords = rankHistory.Where(kvp => kvp.Key >= cutoffDate).ToList();

            for (int i = 1; i < recentRecords.Count; i++)
            {
                result.Add(new RankChangeData
                {
                    Time = recentRecords[i].Key,
                    Rank = recentRecords[i].Value,
                    Change = recentRecords[i - 1].Value - recentRecords[i].Value
                });
            }

            return result;
        }

        /// <summary>
        /// 计算平均排名
        /// </summary>
        private void CalculateAverageRank()
        {
            if (rankHistory.Count == 0)
            {
                AverageRank = 0;
                return;
            }

            long sum = 0;
            foreach (var rank in rankHistory.Values)
            {
                sum += rank;
            }

            AverageRank = (float)sum / rankHistory.Count;
        }

        /// <summary>
        /// 清空历史记录
        /// </summary>
        public void ClearHistory()
        {
            rankHistory.Clear();
            BestRank = int.MaxValue;
            WorstRank = 0;
            AverageRank = 0;
        }

        /// <summary>
        /// 获取历史记录数量
        /// </summary>
        /// <returns>记录数量</returns>
        public int GetRecordCount()
        {
            return rankHistory.Count;
        }

        /// <summary>
        /// 获取所有历史记录时间
        /// </summary>
        /// <returns>时间列表</returns>
        public List<DateTime> GetRecordTimes()
        {
            return rankHistory.Keys.ToList();
        }
    }

    /// <summary>
    /// 排名变化数据类
    /// </summary>
    public class RankChangeData
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 变化（正数表示上升，负数表示下降）
        /// </summary>
        public int Change { get; set; }

        /// <summary>
        /// 获取变化文本
        /// </summary>
        /// <returns>变化文本</returns>
        public string GetChangeText()
        {
            if (Change > 0)
            {
                return $"↑{Change}";
            }
            else if (Change < 0)
            {
                return $"↓{Math.Abs(Change)}";
            }
            else
            {
                return "-";
            }
        }
    }

    /// <summary>
    /// 排行榜历史管理器
    /// 管理所有玩家的排名历史
    /// </summary>
    public class RankingHistoryManager
    {
        /// <summary>
        /// 所有快照
        /// Key: SnapshotId, Value: RankingSnapshot
        /// </summary>
        private static Dictionary<string, RankingSnapshot> snapshots = [];

        /// <summary>
        /// 玩家排名历史
        /// Key: "PlayerId_RankingId", Value: PlayerRankHistory
        /// </summary>
        private static Dictionary<string, PlayerRankHistory> playerHistories = [];

        /// <summary>
        /// 创建快照
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="entries">排行榜条目</param>
        /// <param name="type">快照类型</param>
        /// <param name="description">描述</param>
        /// <param name="seasonNumber">赛季编号</param>
        /// <returns>快照ID</returns>
        public static string CreateSnapshot(string rankingId, List<RankingEntry> entries,
            SnapshotType type, string description, int seasonNumber)
        {
            var snapshot = new RankingSnapshot
            {
                SnapshotId = Guid.NewGuid().ToString(),
                RankingId = rankingId,
                SnapshotTime = DateTime.Now,
                SeasonNumber = seasonNumber,
                Entries = [.. entries],
                Type = type,
                Description = description
            };

            snapshots[snapshot.SnapshotId] = snapshot;

            // 更新所有玩家的排名历史
            foreach (var entry in entries)
            {
                var historyKey = $"{entry.PlayerId}_{rankingId}";
                if (!playerHistories.ContainsKey(historyKey))
                {
                    playerHistories[historyKey] = new PlayerRankHistory
                    {
                        PlayerId = entry.PlayerId,
                        RankingId = rankingId
                    };
                }

                playerHistories[historyKey].AddRecord(entry.Rank, snapshot.SnapshotTime);
            }

            return snapshot.SnapshotId;
        }

        /// <summary>
        /// 获取快照
        /// </summary>
        /// <param name="snapshotId">快照ID</param>
        /// <returns>快照</returns>
        public static RankingSnapshot? GetSnapshot(string snapshotId)
        {
            if (snapshots.ContainsKey(snapshotId))
            {
                return snapshots[snapshotId];
            }

            return null;
        }

        /// <summary>
        /// 获取排行榜的所有快照
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>快照列表</returns>
        public static List<RankingSnapshot> GetSnapshotsByRanking(string rankingId)
        {
            return snapshots.Values
                .Where(s => s.RankingId == rankingId)
                .OrderByDescending(s => s.SnapshotTime)
                .ToList();
        }

        /// <summary>
        /// 获取指定类型的快照
        /// </summary>
        /// <param name="type">快照类型</param>
        /// <returns>快照列表</returns>
        public static List<RankingSnapshot> GetSnapshotsByType(SnapshotType type)
        {
            return snapshots.Values
                .Where(s => s.Type == type)
                .OrderByDescending(s => s.SnapshotTime)
                .ToList();
        }

        /// <summary>
        /// 获取玩家排名历史
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>玩家排名历史</returns>
        public static PlayerRankHistory GetPlayerHistory(string playerId, string rankingId)
        {
            var historyKey = $"{playerId}_{rankingId}";

            if (!playerHistories.ContainsKey(historyKey))
            {
                playerHistories[historyKey] = new PlayerRankHistory
                {
                    PlayerId = playerId,
                    RankingId = rankingId
                };
            }

            return playerHistories[historyKey];
        }

        /// <summary>
        /// 删除指定时间之前的快照
        /// </summary>
        /// <param name="beforeDate">日期</param>
        /// <returns>删除的快照数量</returns>
        public static int DeleteSnapshotsBefore(DateTime beforeDate)
        {
            var toDelete = snapshots.Values
                .Where(s => s.SnapshotTime < beforeDate)
                .Select(s => s.SnapshotId)
                .ToList();

            foreach (var id in toDelete)
            {
                snapshots.Remove(id);
            }

            return toDelete.Count;
        }

        /// <summary>
        /// 清空所有快照
        /// </summary>
        public static void ClearAllSnapshots()
        {
            snapshots.Clear();
        }

        /// <summary>
        /// 清空所有玩家历史
        /// </summary>
        public static void ClearAllHistories()
        {
            playerHistories.Clear();
        }
    }
}
