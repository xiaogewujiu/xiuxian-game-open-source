using XXX.Battle;

namespace XXX.Application.DTOs
{
    /// <summary>
    /// 构造副本链快照，用于前端按副本分组选择本场入口。
    /// </summary>
    public sealed class DungeonMapStageDto
    {
        /// <summary>副本编号。</summary>
        public string DungeonId { get; set; } = string.Empty;
        /// <summary>副本名称。</summary>
        public string DungeonName { get; set; } = string.Empty;
        /// <summary>层序号。</summary>
        public int StageIndex { get; set; }
        /// <summary>地图编号。</summary>
        public string MapId { get; set; } = string.Empty;
        /// <summary>地图名称。</summary>
        public string MapName { get; set; } = string.Empty;
        /// <summary>要求队伍人数。</summary>
        public int RequiredTeamSize { get; set; }
        /// <summary>是否可用。</summary>
        public bool IsAvailable { get; set; }
        /// <summary>不可用原因。</summary>
        public string? UnavailableReason { get; set; }
    }

    /// <summary>
    /// 组队挑战请求体。
    /// </summary>
    public sealed class PartyDungeonChallengeRequestDto
    {
        /// <summary>
        /// 客户端请求幂等编号；为空时由服务端生成一次性编号。
        /// </summary>
        public string? RequestId { get; set; }
    }

    /// <summary>
    /// 战斗请求 DTO。
    /// </summary>
    public class BattleRequestDto
    {
        /// <summary>
        /// 目标编号。
        /// PVP 时用于指定目标玩家；旧兼容链路里也可能承载特殊目标标识。
        /// </summary>
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// 战斗类型。
        /// 支持：PVE / PVP。
        /// </summary>
        public string BattleType { get; set; } = "PVE";

        /// <summary>
        /// 地图编号。
        /// </summary>
        public string? MapId { get; set; }
    }

    /// <summary>
    /// 战斗结果 DTO。
    /// </summary>
    public class BattleResultDto
    {
        /// <summary>
        /// 当前请求是否成功进入战斗流程。
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 当前结果对应的提示文案。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 本次战斗结果的唯一编号。
        /// </summary>
        public string BattleId { get; set; } = string.Empty;

        /// <summary>
        /// 若为组队战斗，则记录队伍编号。
        /// </summary>
        public string PartyId { get; set; } = string.Empty;

        /// <summary>
        /// 发起本次战斗的玩家编号。
        /// </summary>
        public string StartedByPlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 是否获胜。
        /// </summary>
        public bool IsWin { get; set; }

        /// <summary>
        /// 是否胜利。
        /// 与 IsWin 保持兼容，便于旧前端逐步切换。
        /// </summary>
        public bool IsVictory { get; set; }

        /// <summary>
        /// 地图或副本展示名称。
        /// </summary>
        public string MapName { get; set; } = string.Empty;

        /// <summary>
        /// 回合数。
        /// </summary>
        public int Rounds { get; set; }

        /// <summary>
        /// 总回合数。
        /// </summary>
        public int TotalRounds { get; set; }

        /// <summary>
        /// 战斗冷却剩余秒数。
        /// </summary>
        public int BattleCooldownSeconds { get; set; }

        /// <summary>
        /// 战斗冷却截止时间（UTC）。
        /// </summary>
        public DateTime? BattleCooldownUntilUtc { get; set; }

        /// <summary>
        /// 获得经验。
        /// </summary>
        public long ExpGained { get; set; }

        /// <summary>
        /// 获得金币。
        /// </summary>
        public long GoldGained { get; set; }

        /// <summary>
        /// 掉落列表。
        /// </summary>
        public List<BattleDropDto> Drops { get; set; } = [];

        /// <summary>
        /// 扁平化战斗日志。
        /// 主要给旧前端直接展示；更结构化的数据请使用 RoundLogs / Stages。
        /// </summary>
        public List<string> BattleLog { get; set; } = [];

        /// <summary>
        /// 结构化回合日志。
        /// </summary>
        public List<RoundLog> RoundLogs { get; set; } = [];

        /// <summary>
        /// 参战单位技能统计。
        /// </summary>
        public Dictionary<string, FighterSkillStats> FighterSkillStats { get; set; } = [];

        /// <summary>
        /// 分阶段战斗结果。
        /// 主要用于多层副本；普通战斗和 PVP 一般为空。
        /// </summary>
        public List<BattleStageDto> Stages { get; set; } = [];
    }

    /// <summary>
    /// 战斗阶段 DTO。
    /// </summary>
    public class BattleStageDto
    {
        /// <summary>
        /// 阶段序号。
        /// </summary>
        public int StageIndex { get; set; }

        /// <summary>
        /// 当前阶段地图名称。
        /// </summary>
        public string MapName { get; set; } = string.Empty;

        /// <summary>
        /// 当前阶段是否胜利。
        /// </summary>
        public bool IsVictory { get; set; }

        /// <summary>
        /// 当前阶段总回合数。
        /// </summary>
        public int TotalRounds { get; set; }

        /// <summary>
        /// 当前阶段回合日志。
        /// </summary>
        public List<RoundLog> RoundLogs { get; set; } = [];

        /// <summary>
        /// 当前阶段技能统计。
        /// </summary>
        public Dictionary<string, FighterSkillStats> FighterSkillStats { get; set; } = [];
    }

    /// <summary>
    /// 组队副本持久化摘要 DTO。
    /// </summary>
    public sealed class PartyBattleRecordDto
    {
        /// <summary>战斗编号。</summary>
        public string BattleId { get; set; } = string.Empty;
        /// <summary>队伍编号。</summary>
        public string PartyId { get; set; } = string.Empty;
        /// <summary>副本编号。</summary>
        public string DungeonId { get; set; } = string.Empty;
        /// <summary>发起人编号。</summary>
        public string InitiatorPlayerId { get; set; } = string.Empty;
        /// <summary>成员编号 JSON。</summary>
        public string MemberIdsJson { get; set; } = "[]";
        /// <summary>请求幂等编号。</summary>
        public string RequestId { get; set; } = string.Empty;
        /// <summary>开始时间（UTC）。</summary>
        public DateTime StartedAtUtc { get; set; }
        /// <summary>完成时间（UTC）。</summary>
        public DateTime? CompletedAtUtc { get; set; }
        /// <summary>是否胜利。</summary>
        public bool IsVictory { get; set; }
        /// <summary>完成层数。</summary>
        public int StageCount { get; set; }
        /// <summary>总回合数。</summary>
        public int TotalRounds { get; set; }
        /// <summary>结算状态。</summary>
        public string SettlementStatus { get; set; } = string.Empty;
        /// <summary>奖励摘要 JSON。</summary>
        public string RewardSummaryJson { get; set; } = "{}";
    }

    /// <summary>
    /// 战斗掉落 DTO。
    /// </summary>
    public class BattleDropDto
    {
        /// <summary>
        /// 道具编号。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 道具名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 数量。
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 品质。
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 图标路径。
        /// </summary>
        public string? IconPath { get; set; }
    }

    /// <summary>
    /// 普通地图中的怪物预览 DTO。
    /// </summary>
    public class BattleMapMonsterPreviewDto
    {
        /// <summary>
        /// 怪物模板编号。
        /// </summary>
        public string MonsterId { get; set; } = string.Empty;

        /// <summary>
        /// 怪物名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 怪物等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 地图刷怪权重。
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// 单场该怪物最多生成数量。
        /// </summary>
        public int MaxCount { get; set; }
    }

    /// <summary>
    /// 战斗地图 DTO。
    /// </summary>
    public class BattleMapDto
    {
        /// <summary>
        /// 地图编号。
        /// </summary>
        public string MapId { get; set; } = string.Empty;

        /// <summary>
        /// 地图名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 地图描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int RecommendedLevel { get; set; }

        /// <summary>
        /// 最小怪物数量。
        /// </summary>
        public int MonsterCountMin { get; set; }

        /// <summary>
        /// 最大怪物数量。
        /// </summary>
        public int MonsterCountMax { get; set; }

        /// <summary>
        /// 当前是否可挑战。
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// 不可挑战原因。
        /// </summary>
        public string? UnavailableReason { get; set; }

        /// <summary>
        /// 地图实际可能出现的怪物预览。
        /// </summary>
        public List<BattleMapMonsterPreviewDto> Monsters { get; set; } = [];

        /// <summary>
        /// 地图关联怪物配置中的具体道具和装备掉落预览。
        /// </summary>
        public List<BattleDropDto> Drops { get; set; } = [];
    }

    /// <summary>
    /// 开始离线挂机请求 DTO。
    /// </summary>
    public class StartOfflineBattleRequestDto
    {
        /// <summary>
        /// 普通地图编号。
        /// </summary>
        public string MapId { get; set; } = string.Empty;

    }

    /// <summary>
    /// 离线挂机当前状态 DTO。
    /// </summary>
    public class OfflineBattleStatusDto
    {
        /// <summary>
        /// 当前是否处于离线挂机中。
        /// </summary>
        public bool IsOfflineBattling { get; set; }

        /// <summary>
        /// 当前离线挂机地图编号。
        /// </summary>
        public string MapId { get; set; } = string.Empty;

        /// <summary>
        /// 当前离线挂机地图名称。
        /// </summary>
        public string MapName { get; set; } = string.Empty;

        /// <summary>
        /// 离线挂机开始时间（UTC）。
        /// </summary>
        public DateTime? StartedAtUtc { get; set; }

        /// <summary>
        /// 最后一次推进时间（UTC）。
        /// </summary>
        public DateTime? LastTickAtUtc { get; set; }

        /// <summary>
        /// 本次离线挂机结束时间（UTC）。
        /// </summary>
        public DateTime? EndAtUtc { get; set; }

        /// <summary>
        /// 当前累计总场次。
        /// </summary>
        public int TotalBattles { get; set; }

        /// <summary>
        /// 当前累计胜场。
        /// </summary>
        public int WinBattles { get; set; }

        /// <summary>
        /// 当前累计总回合数。
        /// </summary>
        public int TotalRounds { get; set; }
    }

    /// <summary>
    /// 离线挂机停止后的汇总 DTO。
    /// </summary>
    public class OfflineBattleSummaryDto
    {
        /// <summary>
        /// 挂机地图编号。
        /// </summary>
        public string MapId { get; set; } = string.Empty;

        /// <summary>
        /// 挂机地图名称。
        /// </summary>
        public string MapName { get; set; } = string.Empty;

        /// <summary>
        /// 挂机开始时间（UTC）。
        /// </summary>
        public DateTime? StartedAtUtc { get; set; }

        /// <summary>
        /// 本次离线挂机计划结束时间（UTC）。
        /// </summary>
        public DateTime? EndAtUtc { get; set; }

        /// <summary>
        /// 挂机停止时间（UTC）。
        /// </summary>
        public DateTime? StoppedAtUtc { get; set; }

        /// <summary>
        /// 挂机总时长（秒）。
        /// </summary>
        public int DurationSeconds { get; set; }

        /// <summary>
        /// 总场次。
        /// </summary>
        public int TotalBattles { get; set; }

        /// <summary>
        /// 胜场。
        /// </summary>
        public int WinBattles { get; set; }

        /// <summary>
        /// 胜率（百分比）。
        /// </summary>
        public double WinRate => TotalBattles > 0
            ? Math.Round((double)WinBattles / TotalBattles * 100, 2)
            : 0;

        /// <summary>
        /// 总回合数。
        /// </summary>
        public int TotalRounds { get; set; }

        /// <summary>
        /// 获得经验。
        /// </summary>
        public long ExpGained { get; set; }

        /// <summary>
        /// 获得金币。
        /// </summary>
        public long GoldGained { get; set; }

        /// <summary>
        /// 累计掉落道具。
        /// </summary>
        public List<BattleDropDto> ItemDrops { get; set; } = [];

        /// <summary>
        /// 累计掉落装备。
        /// </summary>
        public List<BattleDropDto> EquipmentDrops { get; set; } = [];

        /// <summary>
        /// 累计掉落图鉴。
        /// </summary>
        public List<BattleDropDto> CollectionDrops { get; set; } = [];
    }

    /// <summary>
    /// 副本列表 DTO。
    /// </summary>
    public class DungeonDto
    {
        /// <summary>
        /// 副本编号。
        /// </summary>
        public string DungeonId { get; set; } = string.Empty;

        /// <summary>
        /// 副本名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 副本描述。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int RecommendedLevel { get; set; }

        /// <summary>
        /// 每日奖励上限。
        /// 进入副本不受该数值限制；达到上限后仍可参战，但不再获得副本奖励。
        /// </summary>
        public int DailyLimit { get; set; }

        /// <summary>
        /// 今日已获得奖励的次数。
        /// </summary>
        public int TodayCount { get; set; }

        /// <summary>
        /// 今日剩余奖励次数。
        /// </summary>
        public int RemainingCount { get; set; }

        /// <summary>
        /// 当前是否可挑战。
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// 不可挑战原因。
        /// </summary>
        public string? UnavailableReason { get; set; }

        /// <summary>
        /// 进入该副本所需的最小队伍人数。
        /// 1 表示单人即可挑战。
        /// </summary>
        public int RequiredTeamSize { get; set; } = 1;

        /// <summary>
        /// 当前副本是否支持临时队伍挑战。
        /// </summary>
        public bool SupportsParty { get; set; }
    }
}
