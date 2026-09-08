namespace XXX.Application.DTOs
{
    /// <summary>
    /// 中文注释：
    /// 统一奖励类型常量。
    /// 之所以单独抽一层，是为了让签到、兑换码、后续活动奖励都走同一套类型约定，
    /// 避免前后端分别写 "Gold" / "gold" / "金币" 这种不一致的值，导致展示和发奖逻辑错位。
    /// </summary>
    public static class RewardTypes
    {
        /// <summary>金币奖励类型。</summary>
        public const string Gold = "gold";
        /// <summary>修为奖励类型。</summary>
        public const string Exp = "exp";
        /// <summary>灵石奖励类型。</summary>
        public const string SpiritStone = "spiritStone";
        /// <summary>荣誉奖励类型。</summary>
        public const string Honor = "honor";
        /// <summary>宗门贡献奖励类型。</summary>
        public const string GuildContribution = "guildContribution";
        /// <summary>道具奖励类型。</summary>
        public const string Item = "item";
    }

    /// <summary>
    /// 中文注释：
    /// 这是前后端统一使用的奖励展示 DTO。
    /// 前端只需要依赖 Type / Name / Count 这三个核心字段，就可以稳定渲染奖励列表；
    /// 如果奖励是道具，再通过 ItemId 做进一步跳转或图标映射。
    /// </summary>
    public class RewardItemDto
    {
        /// <summary>
        /// 奖励类型。
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 奖励名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 奖励数量。
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// 道具模板 ID。
        /// 只有道具奖励会用到，其他奖励类型保持为空。
        /// </summary>
        public string? ItemId { get; set; }

        public bool IsLocked { get; set; }

        /// <summary>
        /// 额外说明。
        /// 用于给前端补充“第 7 天里程碑奖励”这类上下文信息。
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// 中文注释：
    /// 这是服务内部发奖时使用的输入模型。
    /// 它和 RewardItemDto 很像，但职责不同：
    /// - RewardGrantItemDto 关注“怎么发”
    /// - RewardItemDto 关注“发完后怎么展示”
    /// 这样可以把服务层的输入意图和接口层的输出结果明确分开。
    /// </summary>
    public class RewardGrantItemDto
    {
        /// <summary>
        /// 奖励类型。
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 奖励数量。
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// 道具模板 ID。
        /// 只有 Type=item 时必填。
        /// </summary>
        public string? ItemId { get; set; }


        /// <summary>
        /// 备注说明。
        /// 用于在奖励快照里保留来源上下文，方便后续排查。
        /// </summary>
        public string? Description { get; set; }
    }
}
