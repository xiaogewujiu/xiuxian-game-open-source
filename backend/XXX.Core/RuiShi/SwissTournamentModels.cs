namespace XXX.RuiShi
{
    #region ==================== 瑞士轮数据模型 ====================

    /// <summary>
    /// 瑞士轮比赛总结果
    /// </summary>
    public class SwissTournamentResult
    {
        /// <summary>
        /// 所有轮次结果（按轮次顺序）
        /// </summary>
        public List<SwissRoundResult> Rounds { get; set; } = [];

        /// <summary>
        /// 最终选手排名列表
        /// </summary>
        public List<PlayerStanding> FinalStandings { get; set; } = [];

        /// <summary>
        /// 实际进行的轮数
        /// </summary>
        public int TotalRounds => Rounds.Count;
    }

    /// <summary>
    /// 单轮比赛结果
    /// </summary>
    public class SwissRoundResult
    {
        /// <summary>
        /// 轮次编号（从1开始）
        /// </summary>
        public int RoundNumber { get; set; }

        /// <summary>
        /// 本轮所有比赛结果（包括轮空）
        /// </summary>
        public List<MatchResult> Matches { get; set; } = [];

        /// <summary>
        /// 本轮轮空的选手
        /// </summary>
        public string ByePlayerName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 单场比赛结果
    /// </summary>
    public class MatchResult
    {
        /// <summary>
        /// 选手A名称
        /// </summary>
        public string PlayerAName { get; set; } = string.Empty;

        /// <summary>
        /// 选手A的GID
        /// </summary>
        public string PlayerAGid { get; set; } = string.Empty;

        /// <summary>
        /// 选手B名称（轮空则为null）
        /// </summary>
        public string PlayerBName { get; set; } = string.Empty;

        /// <summary>
        /// 选手B的GID
        /// </summary>
        public string PlayerBGid { get; set; } = string.Empty;

        /// <summary>
        /// 是否轮空
        /// </summary>
        public bool IsBye { get; set; }

        /// <summary>
        /// 胜者名称
        /// </summary>
        public string WinnerName { get; set; } = string.Empty;

        /// <summary>
        /// 当前轮次编号
        /// </summary>
        public int RoundNumber { get; set; }

        /// <summary>
        /// A选手本轮得分
        /// </summary>
        public int PlayerAScore { get; set; }

        /// <summary>
        /// B选手本轮得分
        /// </summary>
        public int PlayerBScore { get; set; }
    }

    /// <summary>
    /// 选手最终排名
    /// </summary>
    public class PlayerStanding
    {
        /// <summary>
        /// 选手GID
        /// </summary>
        public string PlayerGid { get; set; } = string.Empty;

        /// <summary>
        /// 选手名称
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 总胜场数
        /// </summary>
        public int TotalWins { get; set; }

        /// <summary>
        /// 总积分
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// 对手强度（Strength of Schedule）
        /// 所有对手的最终积分总和，用于解决同分排名
        /// </summary>
        public int StrengthOfSchedule { get; set; }

        /// <summary>
        /// 最终排名（按积分降序）
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// 对战场次
        /// </summary>
        public int MatchesPlayed { get; set; }

        /// <summary>
        /// 轮空次数
        /// </summary>
        public int ByeCount { get; set; }
    }

    #endregion
}
