namespace XXX.Application.DTOs
{
    /// <summary>
    /// 创建临时队伍请求。
    /// </summary>
    public class CreatePartyRequestDto
    {
        /// <summary>
        /// 队伍名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 固定队伍人数，只允许 1、2、3。
        /// </summary>
        public int TeamSize { get; set; } = 1;
    }

    /// <summary>
    /// 临时队伍成员展示 DTO。
    /// </summary>
    public class PartyMemberDto
    {
        /// <summary>
        /// 玩家 ID。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 玩家等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 境界名称。
        /// </summary>
        public string RealmName { get; set; } = string.Empty;

        /// <summary>
        /// 境界层数。
        /// </summary>
        public int RealmLayer { get; set; }

        /// <summary>
        /// 是否为队长。
        /// </summary>
        public bool IsLeader { get; set; }

        /// <summary>
        /// 是否为当前玩家本人。
        /// </summary>
        public bool IsSelf { get; set; }

        /// <summary>
        /// 灵根名称。
        /// </summary>
        public string SpiritRootName { get; set; } = string.Empty;

        /// <summary>
        /// 灵根图标。
        /// </summary>
        public string SpiritRootIcon { get; set; } = string.Empty;

        /// <summary>
        /// 加入队伍时间。
        /// </summary>
        public DateTime JoinTime { get; set; }
    }

    /// <summary>
    /// 临时队伍摘要 DTO。
    /// </summary>
    public class PartySummaryDto
    {
        /// <summary>
        /// 队伍 ID。
        /// </summary>
        public string PartyId { get; set; } = string.Empty;

        /// <summary>
        /// 队伍名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 队长玩家 ID。
        /// </summary>
        public string LeaderPlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 队长名称。
        /// </summary>
        public string LeaderName { get; set; } = string.Empty;

        /// <summary>
        /// 目标副本 ID。
        /// </summary>
        public string TargetDungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 目标副本名称。
        /// </summary>
        public string TargetDungeonName { get; set; } = string.Empty;

        /// <summary>
        /// 最低等级要求。
        /// </summary>
        public int MinLevel { get; set; }

        /// <summary>
        /// 最大成员数。
        /// </summary>
        public int MaxMembers { get; set; }

        /// <summary>
        /// 当前成员数。
        /// </summary>
        public int CurrentMembers { get; set; }

        /// <summary>
        /// 挑战副本所需人数。
        /// </summary>
        public int RequiredTeamSize { get; set; }

        /// <summary>
        /// 是否处于招募状态。
        /// </summary>
        public bool IsRecruiting { get; set; }

        /// <summary>
        /// 当前玩家是否可加入。
        /// </summary>
        public bool CanJoin { get; set; }

        /// <summary>
        /// 当前是否可发起挑战。
        /// </summary>
        public bool CanChallenge { get; set; }

        /// <summary>
        /// 状态文本。
        /// </summary>
        public string StatusText { get; set; } = string.Empty;

        /// <summary>
        /// 当前不可加入或不可挑战的原因。
        /// </summary>
        public string? UnavailableReason { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 过期时间。
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// 当前队伍详情 DTO。
    /// </summary>
    public class PartyDetailDto : PartySummaryDto
    {
        /// <summary>
        /// 当前玩家是否为队长。
        /// </summary>
        public bool IsLeader { get; set; }

        /// <summary>
        /// 当前玩家是否为队伍成员。
        /// </summary>
        public bool IsMember { get; set; }

        /// <summary>
        /// 成员详情列表。
        /// </summary>
        public List<PartyMemberDto> Members { get; set; } = [];
    }
}
