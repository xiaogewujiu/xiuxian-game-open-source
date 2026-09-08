namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台玩家列表项。
    /// </summary>
    public class AdminPlayerListItemDto
    {
        /// <summary>
        /// 玩家编号。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 角色名。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 账号。
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 战斗职业键。
        /// </summary>
        public string Profession { get; set; } = XXX.Entity.PlayerProfessionCatalog.Warrior;

        /// <summary>
        /// 战斗职业显示名。
        /// </summary>
        public string ProfessionName { get; set; } = "战";

        /// <summary>
        /// 金币。
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 当前战斗模式。
        /// </summary>
        public string BattleMode { get; set; } = "Normal";

        /// <summary>
        /// 当前是否处于离线挂机中。
        /// </summary>
        public bool IsOfflineBattling { get; set; }

        /// <summary>
        /// 当前离线挂机地图名称。
        /// </summary>
        public string? OfflineBattleMapName { get; set; }

        /// <summary>
        /// 炼丹师等级。
        /// </summary>
        public int AlchemistLevel { get; set; }

        /// <summary>
        /// 锻造师等级。
        /// </summary>
        public int BlacksmithLevel { get; set; }

        /// <summary>
        /// 聚灵阵等级。
        /// </summary>
        public int ArrayLevel { get; set; }

        /// <summary>
        /// 灵田等级。
        /// </summary>
        public int SpiritFieldLevel { get; set; }
    }

    /// <summary>
    /// 后台玩家详情。
    /// </summary>
    public class AdminPlayerDetailDto
    {
        /// <summary>
        /// 玩家编号。
        /// </summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 角色名。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 账号。
        /// </summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 当前称号。
        /// </summary>
        public string? CurrentTitle { get; set; }

        /// <summary>
        /// 等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 战斗职业键。
        /// </summary>
        public string Profession { get; set; } = XXX.Entity.PlayerProfessionCatalog.Warrior;

        /// <summary>
        /// 战斗职业显示名。
        /// </summary>
        public string ProfessionName { get; set; } = "战";

        /// <summary>
        /// 当前经验。
        /// </summary>
        public long Exp { get; set; }

        /// <summary>
        /// 升级需求经验。
        /// </summary>
        public long XExp { get; set; }

        /// <summary>
        /// 金币。
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 荣誉。
        /// </summary>
        public int Honor { get; set; }

        /// <summary>
        /// 公会贡献。
        /// </summary>
        public int GuildContribution { get; set; }

        /// <summary>
        /// 灵石。
        /// </summary>
        public long SpiritStone { get; set; }

        /// <summary>
        /// 是否封禁。
        /// </summary>
        public bool IsBanned { get; set; }

        /// <summary>
        /// 封禁原因。
        /// </summary>
        public string? BanReason { get; set; }

        /// <summary>
        /// 封禁截止时间。
        /// </summary>
        public DateTime? BanExpiresAt { get; set; }

        /// <summary>
        /// 战斗冷却截止时间（UTC）。
        /// </summary>
        public DateTime? BattleCooldownUntilUtc { get; set; }

        /// <summary>
        /// 当前战斗模式。
        /// </summary>
        public string BattleMode { get; set; } = "Normal";

        /// <summary>
        /// 当前是否处于离线挂机中。
        /// </summary>
        public bool IsOfflineBattling { get; set; }

        /// <summary>
        /// 当前离线挂机地图编号。
        /// </summary>
        public string? OfflineBattleMapId { get; set; }

        /// <summary>
        /// 当前离线挂机地图名称。
        /// </summary>
        public string? OfflineBattleMapName { get; set; }

        /// <summary>
        /// 当前离线挂机开始时间（UTC）。
        /// </summary>
        public DateTime? OfflineBattleStartedAtUtc { get; set; }

        /// <summary>
        /// 当前离线挂机最后推进时间（UTC）。
        /// </summary>
        public DateTime? OfflineBattleLastTickAtUtc { get; set; }

        /// <summary>
        /// 当前离线挂机累计场次。
        /// </summary>
        public int OfflineBattleTotalBattles { get; set; }

        /// <summary>
        /// 当前离线挂机累计胜场。
        /// </summary>
        public int OfflineBattleWinBattles { get; set; }

        /// <summary>
        /// 当前离线挂机累计经验。
        /// </summary>
        public long OfflineBattleExpGained { get; set; }

        /// <summary>
        /// 当前离线挂机累计金币。
        /// </summary>
        public long OfflineBattleGoldGained { get; set; }

        /// <summary>
        /// 聚灵阵等级。
        /// </summary>
        public int ArrayLevel { get; set; }

        /// <summary>
        /// 金系等级。
        /// </summary>
        public int MetalLevel { get; set; }

        /// <summary>
        /// 木系等级。
        /// </summary>
        public int WoodLevel { get; set; }

        /// <summary>
        /// 水系等级。
        /// </summary>
        public int WaterLevel { get; set; }

        /// <summary>
        /// 火系等级。
        /// </summary>
        public int FireLevel { get; set; }

        /// <summary>
        /// 土系等级。
        /// </summary>
        public int EarthLevel { get; set; }

        /// <summary>
        /// 聚灵阵提供的职业等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 炼丹师等级。
        /// </summary>
        public int AlchemistLevel { get; set; }

        /// <summary>
        /// 炼丹师经验。
        /// </summary>
        public int AlchemistExp { get; set; }

        /// <summary>
        /// 当前是否正在炼丹。
        /// </summary>
        public bool IsAlchemyCrafting { get; set; }

        /// <summary>
        /// 当前炼丹中的配方编号。
        /// </summary>
        public string? ActiveAlchemyRecipeId { get; set; }

        /// <summary>
        /// 当前炼丹完成时间。
        /// </summary>
        public DateTime? ActiveAlchemyCompleteAt { get; set; }

        /// <summary>
        /// 当前炼丹是否可领取。
        /// </summary>
        public bool CanCollectAlchemy { get; set; }

        /// <summary>
        /// 锻造师等级。
        /// </summary>
        public int BlacksmithLevel { get; set; }

        /// <summary>
        /// 锻造师经验。
        /// </summary>
        public int BlacksmithExp { get; set; }

        /// <summary>
        /// 当前是否正在锻造。
        /// </summary>
        public bool IsForging { get; set; }

        /// <summary>
        /// 当前锻造中的图纸编号。
        /// </summary>
        public string? ActiveForgeRecipeId { get; set; }

        /// <summary>
        /// 当前锻造完成时间。
        /// </summary>
        public DateTime? ActiveForgeCompleteAt { get; set; }

        /// <summary>
        /// 当前锻造是否可领取。
        /// </summary>
        public bool CanCollectForge { get; set; }

        /// <summary>
        /// 灵田等级。
        /// </summary>
        public int SpiritFieldLevel { get; set; }

        /// <summary>
        /// 已解锁灵田地块数。
        /// </summary>
        public int SpiritFieldUnlockedPlots { get; set; }

        /// <summary>
        /// 灵田最大地块数。
        /// </summary>
        public int SpiritFieldMaxPlots { get; set; }

        /// <summary>
        /// 灵田全局产量加成。
        /// </summary>
        public int SpiritFieldGlobalYieldBonus { get; set; }
    }

    /// <summary>
    /// 后台离线挂机列表项。
    /// </summary>
    public class AdminOfflineBattleListItemDto
    {
        /// <summary>玩家编号。</summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>玩家名称。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>玩家账号。</summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>挂机地图编号。</summary>
        public string MapId { get; set; } = string.Empty;

        /// <summary>挂机地图名称。</summary>
        public string MapName { get; set; } = string.Empty;

        /// <summary>挂机开始时间（UTC）。</summary>
        public DateTime? StartedAtUtc { get; set; }

        /// <summary>最近一次结算时间（UTC）。</summary>
        public DateTime? LastTickAtUtc { get; set; }

        /// <summary>战斗冷却截止时间（UTC）。</summary>
        public DateTime? BattleCooldownUntilUtc { get; set; }

        /// <summary>累计战斗场次。</summary>
        public int TotalBattles { get; set; }

        /// <summary>累计胜利场次。</summary>
        public int WinBattles { get; set; }

        /// <summary>挂机累计获得经验。</summary>
        public long ExpGained { get; set; }

        /// <summary>挂机累计获得金币。</summary>
        public long GoldGained { get; set; }
    }

    /// <summary>
    /// 后台强制停止离线挂机后的汇总。
    /// </summary>
    public class AdminOfflineBattleSummaryDto
    {
        /// <summary>玩家编号。</summary>
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>玩家名称。</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>玩家账号。</summary>
        public string Account { get; set; } = string.Empty;

        /// <summary>挂机地图编号。</summary>
        public string MapId { get; set; } = string.Empty;

        /// <summary>挂机地图名称。</summary>
        public string MapName { get; set; } = string.Empty;

        /// <summary>挂机开始时间（UTC）。</summary>
        public DateTime? StartedAtUtc { get; set; }

        /// <summary>挂机停止时间（UTC）。</summary>
        public DateTime? StoppedAtUtc { get; set; }

        /// <summary>本次挂机持续秒数。</summary>
        public int DurationSeconds { get; set; }

        /// <summary>累计战斗场次。</summary>
        public int TotalBattles { get; set; }

        /// <summary>累计胜利场次。</summary>
        public int WinBattles { get; set; }

        /// <summary>挂机累计获得经验。</summary>
        public long ExpGained { get; set; }

        /// <summary>挂机累计获得金币。</summary>
        public long GoldGained { get; set; }

        /// <summary>挂机掉落物品列表。</summary>
        public List<BattleDropDto> ItemDrops { get; set; } = [];

        /// <summary>挂机掉落装备列表。</summary>
        public List<BattleDropDto> EquipmentDrops { get; set; } = [];
    }

    /// <summary>
    /// 后台发放货币请求。
    /// </summary>
    public class AdminGrantCurrencyRequestDto
    {
        /// <summary>
        /// 资源类型。
        /// 支持：Gold、SpiritStone、Honor、GuildContribution、Exp。
        /// </summary>
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// 发放数量。
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 发放原因。
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// 后台发放道具请求。
    /// </summary>
    public class AdminGrantItemRequestDto
    {
        /// <summary>
        /// 道具编号。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 发放数量。
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// 发放原因。
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// 后台封禁玩家请求。
    /// </summary>
    public class AdminBanPlayerRequestDto
    {
        /// <summary>
        /// 封禁原因。
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 封禁时长小时数。
        /// 0 表示长期封禁。
        /// </summary>
        public int Hours { get; set; }
    }
}
