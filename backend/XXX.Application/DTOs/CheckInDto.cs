namespace XXX.Application.DTOs
{
    /// <summary>
    /// 中文注释：
    /// 单个月份签到面板的完整展示数据。
    /// 前端日历切月份时，只需要重新请求这一份数据，就能同时得到：
    /// - 月历勾选状态
    /// - 当前连续签到天数
    /// - 今日奖励预览
    /// - 里程碑进度
    /// </summary>
    public class CheckInStatusDto
    {
        /// <summary>
        /// 当前查询年份。
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 当前查询月份。
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 当前连续签到天数。
        /// </summary>
        public int ContinuousDays { get; set; }

        /// <summary>
        /// 历史累计签到天数。
        /// </summary>
        public int TotalDays { get; set; }

        /// <summary>
        /// 今天是否已签到。
        /// </summary>
        public bool HasCheckedToday { get; set; }

        /// <summary>
        /// 下一个里程碑天数。
        /// </summary>
        public int NextMilestoneDay { get; set; }

        /// <summary>
        /// 距离下一个里程碑还差多少天。
        /// </summary>
        public int DaysToNextMilestone { get; set; }

        /// <summary>
        /// 最近一次签到日期。
        /// </summary>
        public DateTime? LastCheckInDate { get; set; }

        /// <summary>
        /// 已签到日期列表，统一用 yyyy-MM-dd 字符串输出。
        /// 这样前端在不同时区和不同浏览器下都能稳定匹配日期格子。
        /// </summary>
        public List<string> CheckedDates { get; set; } = [];

        /// <summary>
        /// 今日可领取或已领取的奖励预览。
        /// </summary>
        public List<RewardItemDto> TodayRewards { get; set; } = [];

        /// <summary>
        /// 里程碑奖励概览列表。
        /// </summary>
        public List<CheckInMilestoneDto> Milestones { get; set; } = [];

        /// <summary>
        /// 当前面板提示文案。
        /// </summary>
        public string HintMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 中文注释：
    /// 里程碑展示项。
    /// 这里直接返回摘要文案，是为了避免前端再去拼接奖励字符串，减少页面逻辑分叉。
    /// </summary>
    public class CheckInMilestoneDto
    {
        /// <summary>
        /// 里程碑天数。
        /// </summary>
        public int Days { get; set; }

        /// <summary>
        /// 是否已达到该里程碑。
        /// </summary>
        public bool IsReached { get; set; }

        /// <summary>
        /// 奖励摘要文本。
        /// </summary>
        public string RewardSummary { get; set; } = string.Empty;
    }

    /// <summary>
    /// 中文注释：
    /// 执行签到后的返回结构。
    /// 除了本次实际发到手的奖励，还会直接回传最新签到状态，
    /// 这样前端点完“立即签到”后不需要再额外补一次状态接口。
    /// </summary>
    public class ClaimCheckInResultDto
    {
        /// <summary>
        /// 本次签到的结果提示。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 本次实际发放的奖励。
        /// </summary>
        public List<RewardItemDto> Rewards { get; set; } = [];

        /// <summary>
        /// 签到后的最新状态快照。
        /// </summary>
        public CheckInStatusDto Status { get; set; } = new();
    }
}
