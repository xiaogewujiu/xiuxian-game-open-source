namespace XXX.Application.DTOs
{
    /// <summary>
    /// 宗门摘要信息。
    /// </summary>
    public class GuildDto
    {
        /// <summary>
        /// 宗门 ID。
        /// </summary>
        public string GuildId { get; set; } = string.Empty;

        /// <summary>
        /// 宗门名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 宗门等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 当前成员数。
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// 最大成员数。
        /// </summary>
        public int MaxMembers { get; set; }

        /// <summary>
        /// 宗主名称。
        /// </summary>
        public string LeaderName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 创建宗门请求。
    /// </summary>
    public class CreateGuildRequestDto
    {
        /// <summary>
        /// 新宗门名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 宗门成员展示数据。
    /// </summary>
    public class GuildMemberDto
    {
        /// <summary>
        /// 玩家 ID。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称。
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 玩家等级。
        /// </summary>
        public int PlayerLevel { get; set; }

        /// <summary>
        /// 宗门职位。
        /// </summary>
        public string Position { get; set; } = string.Empty;

        /// <summary>
        /// 贡献值。
        /// </summary>
        public int Contribution { get; set; }

        /// <summary>
        /// 入宗时间。
        /// </summary>
        public DateTime JoinTime { get; set; }
    }
}
