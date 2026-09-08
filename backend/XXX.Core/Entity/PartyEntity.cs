using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 临时副本队伍实体。
    /// </summary>
    [SugarTable("Parties")]
    public class PartyEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string PartyId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false)]
        public string LeaderPlayerId { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false)]
        public string TargetDungeonId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string TargetDungeonName { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int MinLevel { get; set; }

        [SugarColumn(IsNullable = false)]
        public int MaxMembers { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsRecruiting { get; set; } = true;

        [SugarColumn(IsNullable = false)]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        [SugarColumn(IsNullable = false)]
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddHours(12);
    }

    /// <summary>
    /// 临时副本队伍成员实体。
    /// </summary>
    [SugarTable("PartyMembers")]
    public class PartyMemberEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50)]
        public string MembershipId { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false)]
        public string PartyId { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false)]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsLeader { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime JoinTime { get; set; } = DateTime.Now;
    }
}
