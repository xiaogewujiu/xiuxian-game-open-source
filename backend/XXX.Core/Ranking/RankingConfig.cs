namespace XXX.Ranking
{
    /// <summary>
    /// 排行榜类型枚举
    /// 定义不同类型的排行榜
    /// </summary>
    public enum RankingType
    {
        /// <summary>
        /// 等级排行榜 - 按玩家等级排序
        /// </summary>
        Level,

        /// <summary>
        /// 战力排行榜 - 按战斗力排序
        /// </summary>
        Combat,

        /// <summary>
        /// 竞技场排行榜 - 按竞技场积分排序
        /// </summary>
        Arena,

        /// <summary>
        /// 副本排行榜 - 按副本通关记录排序
        /// </summary>
        Dungeon,

        /// <summary>
        /// 成就排行榜 - 按成就点数排序
        /// </summary>
        Achievement,

        /// <summary>
        /// 财富排行榜 - 按金币数量排序
        /// </summary>
        Wealth,

        /// <summary>
        /// 通天塔排行榜 - 按最高通关层排序
        /// </summary>
        Tower
    }

    /// <summary>
    /// 排行榜配置类
    /// 定义一个排行榜的完整配置
    /// </summary>
    public class RankingConfig
    {
        /// <summary>
        /// 排行榜唯一ID
        /// </summary>
        public string RankingId { get; set; } = string.Empty;

        /// <summary>
        /// 排行榜名称
        /// </summary>
        public string RankingName { get; set; } = string.Empty;

        /// <summary>
        /// 排行榜类型
        /// </summary>
        public RankingType RankingType { get; set; }

        /// <summary>
        /// 排行榜描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 最大排名数量
        /// </summary>
        public int MaxSize { get; set; } = 100;

        /// <summary>
        /// 更新间隔（分钟）
        /// 0表示实时更新
        /// </summary>
        public int UpdateInterval { get; set; } = 60;

        /// <summary>
        /// 是否启用赛季制
        /// </summary>
        public bool SeasonEnabled { get; set; } = false;

        /// <summary>
        /// 赛季时长（天）
        /// </summary>
        public int SeasonDuration { get; set; } = 30;

        /// <summary>
        /// 当前赛季编号
        /// </summary>
        public int CurrentSeason { get; set; } = 1;

        /// <summary>
        /// 赛季开始时间
        /// </summary>
        public DateTime SeasonStartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 排名奖励列表
        /// </summary>
        public List<RankingRewardEntry> Rewards { get; set; } = [];

        /// <summary>
        /// 排行榜图标
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 排序（用于UI显示顺序）
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 检查是否需要新赛季
        /// </summary>
        /// <returns>是否需要新赛季</returns>
        public bool NeedsNewSeason()
        {
            if (!SeasonEnabled)
            {
                return false;
            }

            var seasonEnd = SeasonStartTime.AddDays(SeasonDuration);
            return DateTime.Now >= seasonEnd;
        }

        /// <summary>
        /// 开始新赛季
        /// </summary>
        public void StartNewSeason()
        {
            CurrentSeason++;
            SeasonStartTime = DateTime.Now;
        }

        /// <summary>
        /// 获取赛季剩余天数
        /// </summary>
        /// <returns>剩余天数</returns>
        public int GetSeasonDaysRemaining()
        {
            if (!SeasonEnabled)
            {
                return 0;
            }

            var seasonEnd = SeasonStartTime.AddDays(SeasonDuration);
            var remaining = seasonEnd - DateTime.Now;
            return Math.Max(0, (int)remaining.TotalDays);
        }

        /// <summary>
        /// 获取赛季结束时间
        /// </summary>
        /// <returns>赛季结束时间</returns>
        public DateTime GetSeasonEndTime()
        {
            return SeasonStartTime.AddDays(SeasonDuration);
        }

        /// <summary>
        /// 根据排名获取奖励
        /// </summary>
        /// <param name="rank">排名</param>
        /// <returns>奖励配置，如果没有则返回null</returns>
        public RankingRewardEntry? GetRewardByRank(int rank)
        {
            return Rewards.FirstOrDefault(r => rank >= r.MinRank && rank <= r.MaxRank);
        }

        /// <summary>
        /// 检查排名是否有奖励
        /// </summary>
        /// <param name="rank">排名</param>
        /// <returns>是否有奖励</returns>
        public bool HasRewardForRank(int rank)
        {
            return Rewards.Any(r => rank >= r.MinRank && rank <= r.MaxRank);
        }
    }

    /// <summary>
    /// 排行榜条目类
    /// 表示排行榜中的一个条目
    /// </summary>
    public class RankingEntry
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 玩家等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 排行分数（根据类型不同含义不同）
        /// - Level: 等级
        /// - Combat: 战斗力
        /// - Arena: 竞技场积分
        /// - Dungeon: 副本通关记录
        /// - Achievement: 成就点数
        /// - Wealth: 金币数量
        /// </summary>
        public long Score { get; set; }

        /// <summary>
        /// 当前排名
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 上次排名（用于显示排名变化）
        /// </summary>
        public int LastRank { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 额外数据（用于存储特定排行榜的额外信息）
        /// </summary>
        public Dictionary<string, string> ExtraData { get; set; } = [];

        /// <summary>
        /// 当前称号
        /// </summary>
        public string? CurrentTitle { get; set; }

        /// <summary>
        /// 获取排名变化
        /// </summary>
        /// <returns>排名变化（正数表示上升，负数表示下降，0表示不变）</returns>
        public int GetRankChange()
        {
            return LastRank - Rank;
        }

        /// <summary>
        /// 获取排名变化文本
        /// </summary>
        /// <returns>变化文本（↑↑ ↑ - ↓ ↓↓）</returns>
        public string GetRankChangeText()
        {
            int change = GetRankChange();

            if (change > 10)
            {
                return "↑↑";
            }
            else if (change > 0)
            {
                return "↑";
            }
            else if (change < -10)
            {
                return "↓↓";
            }
            else if (change < 0)
            {
                return "↓";
            }
            else
            {
                return "-";
            }
        }

        /// <summary>
        /// 获取排名变化颜色
        /// </summary>
        /// <returns>颜色代码</returns>
        public string GetRankChangeColor()
        {
            int change = GetRankChange();

            if (change > 0)
            {
                return "#00FF00"; // 绿色 - 上升
            }
            else if (change < 0)
            {
                return "#FF0000"; // 红色 - 下降
            }
            else
            {
                return "#FFFFFF"; // 白色 - 不变
            }
        }

        /// <summary>
        /// 更新排名并保存上一次排名
        /// </summary>
        /// <param name="newRank">新排名</param>
        public void UpdateRank(int newRank)
        {
            LastRank = Rank;
            Rank = newRank;
            LastUpdateTime = DateTime.Now;
        }

        /// <summary>
        /// 更新分数
        /// </summary>
        /// <param name="newScore">新分数</param>
        public void UpdateScore(long newScore)
        {
            Score = newScore;
            LastUpdateTime = DateTime.Now;
        }

        /// <summary>
        /// 设置额外数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void SetExtraData(string key, string value)
        {
            ExtraData[key] = value;
        }

        /// <summary>
        /// 获取额外数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>值</returns>
        public string GetExtraData(string key)
        {
            return ExtraData.ContainsKey(key) ? ExtraData[key] : string.Empty;
        }
    }

    /// <summary>
    /// 排名奖励条目类
    /// </summary>
    public class RankingRewardEntry
    {
        /// <summary>
        /// 最低排名（包含）
        /// </summary>
        public int MinRank { get; set; }

        /// <summary>
        /// 最高排名（包含）
        /// </summary>
        public int MaxRank { get; set; }

        /// <summary>
        /// 奖励标题
        /// </summary>
        public string RewardTitle { get; set; } = string.Empty;

        /// <summary>
        /// 经验奖励
        /// </summary>
        public long Exp { get; set; }

        /// <summary>
        /// 金币奖励
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 灵石奖励
        /// </summary>
        public long SpiritStone { get; set; }

        /// <summary>
        /// 称号奖励
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 道具奖励列表
        /// Key: 道具ID, Value: 数量
        /// </summary>
        public Dictionary<string, int> Items { get; set; } = [];

        /// <summary>
        /// 装备奖励列表（装备模板ID）
        /// </summary>
        public List<int> EquipmentIds { get; set; } = [];

        /// <summary>
        /// 获取奖励摘要文本
        /// </summary>
        /// <returns>奖励摘要</returns>
        public string GetRewardSummary()
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(Title))
            {
                parts.Add($"称号：{Title}");
            }

            if (Exp > 0)
            {
                parts.Add($"{Exp} 经验");
            }

            if (Gold > 0)
            {
                parts.Add($"{Gold} 金币");
            }

            if (SpiritStone > 0)
            {
                parts.Add($"{SpiritStone} 灵石");
            }

            if (Items != null && Items.Count > 0)
            {
                foreach (var item in Items)
                {
                    parts.Add($"{item.Key} x{item.Value}");
                }
            }

            if (EquipmentIds != null && EquipmentIds.Count > 0)
            {
                foreach (var equipId in EquipmentIds)
                {
                    parts.Add($"装备({equipId})");
                }
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "无奖励";
        }

        /// <summary>
        /// 检查排名是否在奖励范围内
        /// </summary>
        /// <param name="rank">排名</param>
        /// <returns>是否在范围内</returns>
        public bool IsInRewardRange(int rank)
        {
            return rank >= MinRank && rank <= MaxRank;
        }

        /// <summary>
        /// 获取排名范围文本
        /// </summary>
        /// <returns>范围文本</returns>
        public string GetRangeText()
        {
            if (MinRank == MaxRank)
            {
                return $"第{MinRank}名";
            }
            else
            {
                return $"第{MinRank}-{MaxRank}名";
            }
        }
    }
}
