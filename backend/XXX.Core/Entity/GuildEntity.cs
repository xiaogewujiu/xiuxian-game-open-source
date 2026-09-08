using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 公会实体
    /// </summary>
    [SugarTable("guild")]
    public class GuildEntity
    {
        /// <summary>
        /// 公会ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "公会ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 公会名称
        /// </summary>
        [SugarColumn(ColumnDescription = "公会名称", Length = 50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 公会公告
        /// </summary>
        [SugarColumn(ColumnDescription = "公会公告", Length = 500, IsNullable = true)]
        public string? Announcement { get; set; }

        /// <summary>
        /// 会长ID
        /// </summary>
        [SugarColumn(ColumnDescription = "会长ID")]
        public string LeaderId { get; set; } = string.Empty;

        /// <summary>
        /// 会长名称
        /// </summary>
        [SugarColumn(ColumnDescription = "会长名称", Length = 50)]
        public string LeaderName { get; set; } = string.Empty;

        /// <summary>
        /// 公会等级
        /// </summary>
        [SugarColumn(ColumnDescription = "公会等级")]
        public int Level { get; set; } = 1;

        /// <summary>
        /// 公会经验
        /// </summary>
        [SugarColumn(ColumnDescription = "公会经验")]
        public long Exp { get; set; } = 0;

        /// <summary>
        /// 成员数量
        /// </summary>
        [SugarColumn(ColumnDescription = "成员数量")]
        public int MemberCount { get; set; } = 1;

        /// <summary>
        /// 最大成员数
        /// </summary>
        [SugarColumn(ColumnDescription = "最大成员数")]
        public int MaxMembers { get; set; } = 20;

        /// <summary>
        /// 公会资金
        /// </summary>
        [SugarColumn(ColumnDescription = "公会资金")]
        public long Funds { get; set; } = 0;

        /// <summary>
        /// 加入条件：最低等级
        /// </summary>
        [SugarColumn(ColumnDescription = "加入条件-最低等级")]
        public int RequiredLevel { get; set; } = 1;

        /// <summary>
        /// 是否允许自由加入
        /// </summary>
        [SugarColumn(ColumnDescription = "是否允许自由加入")]
        public bool AutoJoin { get; set; } = false;

        /// <summary>
        /// 公会图标
        /// </summary>
        [SugarColumn(ColumnDescription = "公会图标", Length = 100, IsNullable = true)]
        public string? Icon { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否已删除
        /// </summary>
        [SugarColumn(ColumnDescription = "是否已删除")]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 所属宗门模板ID
        /// </summary>
        [SugarColumn(ColumnDescription = "宗门模板ID", Length = 100, IsNullable = true)]
        public string? SectTemplateId { get; set; }

        /// <summary>
        /// 累计捐献金额（用于宗门升级）
        /// </summary>
        [SugarColumn(ColumnDescription = "累计捐献")]
        public int TotalDonation { get; set; } = 0;
    }

    /// <summary>
    /// 公会成员实体
    /// </summary>
    [SugarTable("guild_member")]
    public class GuildMemberEntity
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "记录ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 公会ID
        /// </summary>
        [SugarColumn(ColumnDescription = "公会ID")]
        public string GuildId { get; set; } = string.Empty;

        /// <summary>
        /// 成员ID
        /// </summary>
        [SugarColumn(ColumnDescription = "成员ID")]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 成员名称
        /// </summary>
        [SugarColumn(ColumnDescription = "成员名称", Length = 50)]
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// 成员等级
        /// </summary>
        [SugarColumn(ColumnDescription = "成员等级")]
        public int PlayerLevel { get; set; } = 1;

        /// <summary>
        /// 公会职位
        /// </summary>
        [SugarColumn(ColumnDescription = "公会职位")]
        public GuildPosition Position { get; set; } = GuildPosition.Member;

        /// <summary>
        /// 公会贡献值
        /// </summary>
        [SugarColumn(ColumnDescription = "公会贡献值")]
        public int Contribution { get; set; } = 0;

        /// <summary>
        /// 总贡献值
        /// </summary>
        [SugarColumn(ColumnDescription = "总贡献值")]
        public int TotalContribution { get; set; } = 0;

        /// <summary>
        /// 加入时间
        /// </summary>
        [SugarColumn(ColumnDescription = "加入时间")]
        public DateTime JoinTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后活跃时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后活跃时间")]
        public DateTime LastActiveTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 公会职位
    /// </summary>
    public enum GuildPosition
    {
        /// <summary>
        /// 成员
        /// </summary>
        Member = 0,

        /// <summary>
        /// 长老
        /// </summary>
        Elder = 1,

        /// <summary>
        /// 副会长
        /// </summary>
        ViceLeader = 2,

        /// <summary>
        /// 会长
        /// </summary>
        Leader = 3
    }
}
