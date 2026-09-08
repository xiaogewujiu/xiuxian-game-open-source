namespace XXX.Application.DTOs
{
    /// <summary>
    /// 玩家基础信息数据传输对象。
    /// </summary>
    public class PlayerDto
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 道号
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 战斗职业键。
        /// </summary>
        public string Profession { get; set; } = XXX.Entity.PlayerProfessionCatalog.Warrior;

        /// <summary>
        /// 战斗职业显示名。
        /// </summary>
        public string ProfessionName { get; set; } = "战";

        /// <summary>
        /// 当前佩戴称号
        /// </summary>
        public string? CurrentTitle { get; set; }

        /// <summary>
        /// 玩家头像图片路径。
        /// </summary>
        public string? AvatarImagePath { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 当前经验
        /// </summary>
        public long Exp { get; set; }

        /// <summary>
        /// 升级所需经验
        /// </summary>
        public long XExp { get; set; }

        /// <summary>
        /// 经验百分比
        /// </summary>
        public double ExpPercent => XExp > 0 ? (double)Exp / XExp * 100 : 0;

        /// <summary>
        /// 生命值
        /// </summary>
        public int HP { get; set; }

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHP { get; set; }

        /// <summary>
        /// 法力值
        /// </summary>
        public int MP { get; set; }

        /// <summary>
        /// 最大法力值
        /// </summary>
        public int MaxMP { get; set; }

        /// <summary>
        /// 攻击力
        /// </summary>
        public int Attack { get; set; }

        /// <summary>
        /// 法术攻击力
        /// </summary>
        public int MagicAttack { get; set; }

        /// <summary>
        /// 防御力
        /// </summary>
        public int Defense { get; set; }

        /// <summary>
        /// 法术防御力
        /// </summary>
        public int MagicDefense { get; set; }

        /// <summary>
        /// 速度
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// 命中率
        /// </summary>
        public double HitRate { get; set; }

        /// <summary>
        /// 闪避率
        /// </summary>
        public double DodgeRate { get; set; }

        /// <summary>
        /// 暴击率
        /// </summary>
        public double CritRate { get; set; }

        /// <summary>
        /// 暴击伤害
        /// </summary>
        public double CritDamage { get; set; }

        /// <summary>
        /// 连击率
        /// </summary>
        public double ComboRate { get; set; }

        /// <summary>
        /// 反击率
        /// </summary>
        public double CounterRate { get; set; }

        /// <summary>
        /// 破甲率
        /// </summary>
        public double ArmorBreak { get; set; }

        /// <summary>
        /// 额外伤害倍率
        /// </summary>
        public double BonusDamage { get; set; }

        /// <summary>
        /// 金币
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 装备背包容量。
        /// </summary>
        public int EquipmentInventoryCapacity { get; set; } = 100;

        /// <summary>
        /// 道具背包容量。
        /// </summary>
        public int ItemInventoryCapacity { get; set; } = 100;

        /// <summary>
        /// 灵石
        /// </summary>
        public long SpiritStone { get; set; }

        /// <summary>
        /// 荣誉点
        /// </summary>
        public int Honor { get; set; }

        /// <summary>
        /// 公会贡献
        /// </summary>
        public int GuildContribution { get; set; }

        /// <summary>
        /// 当前出战宠物ID
        /// </summary>
        public string? PetId { get; set; }

        /// <summary>
        /// 公会ID
        /// </summary>
        public string? GuildId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime LastLoginTime { get; set; }

        /// <summary>
        /// 战斗冷却剩余秒数。
        /// </summary>
        public int BattleCooldownSeconds { get; set; }

        /// <summary>
        /// 战斗冷却截止时间（UTC）。
        /// </summary>
        public DateTime? BattleCooldownUntilUtc { get; set; }

        /// <summary>
        /// 当前角色的灵根展示信息。
        /// 前端主界面直接消费这个字段，不再在页面里写死灵根名称和克制表。
        /// </summary>
        public PlayerSpiritRootDto SpiritRoot { get; set; } = new();

        /// <summary>
        /// 当前角色的属性加点总览。
        /// 前端主界面的“属性加点”区域直接读取这个结构，这样刷新页面后仍能准确还原已投入点数和剩余点数。
        /// </summary>
        public PlayerAttributePointOverviewDto AttributePoints { get; set; } = new();

        /// <summary>
        /// 当前角色的突破状态。
        /// 前端主界面直接消费这个结构，不再用经验条满值去伪装突破条件。
        /// </summary>
        public PlayerBreakthroughDto Breakthrough { get; set; } = new();
    }

    /// <summary>
    /// 玩家详细信息数据传输对象。
    /// </summary>
    public class PlayerDetailDto : PlayerDto
    {
        /// <summary>
        /// 战斗统计
        /// </summary>
        public PlayerBattleStatsDto BattleStats { get; set; } = new();

        /// <summary>
        /// 货币统计
        /// </summary>
        public PlayerCurrencyStatsDto CurrencyStats { get; set; } = new();
    }

    /// <summary>
    /// 玩家战斗统计数据传输对象。
    /// </summary>
    public class PlayerBattleStatsDto
    {
        /// <summary>
        /// 总战斗场次
        /// </summary>
        public int TotalBattles { get; set; }

        /// <summary>
        /// 胜利场次
        /// </summary>
        public int WinBattles { get; set; }

        /// <summary>
        /// 胜率
        /// </summary>
        public double WinRate => TotalBattles > 0 ? (double)WinBattles / TotalBattles * 100 : 0;

        /// <summary>
        /// 总击杀数
        /// </summary>
        public int TotalKills { get; set; }

        /// <summary>
        /// 最大连击数
        /// </summary>
        public int MaxCombo { get; set; }
    }

    /// <summary>
    /// 玩家货币统计数据传输对象。
    /// </summary>
    public class PlayerCurrencyStatsDto
    {
        /// <summary>
        /// 累计获得金币
        /// </summary>
        public long TotalGoldEarned { get; set; }

        /// <summary>
        /// 累计消耗金币
        /// </summary>
        public long TotalGoldSpent { get; set; }

        /// <summary>
        /// 累计获得经验
        /// </summary>
        public long TotalExpEarned { get; set; }
    }

    /// <summary>
    /// 创建玩家请求数据传输对象。
    /// </summary>
    public class CreatePlayerRequestDto
    {
        /// <summary>
        /// 道号
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 战斗职业。
        /// </summary>
        public string Profession { get; set; } = XXX.Entity.PlayerProfessionCatalog.Warrior;
    }

    /// <summary>
    /// 更新玩家请求数据传输对象。
    /// </summary>
    public class UpdatePlayerRequestDto
    {
        /// <summary>
        /// 道号
        /// </summary>
        public string? Name { get; set; }
    }

    /// <summary>
    /// 玩家自动出售装备设置。
    /// </summary>
    public class EquipmentAutoSellSettingsDto
    {
        public int MinEquipmentLevel { get; set; }
        public int MinEquipmentQuality { get; set; }
    }

    /// <summary>
    /// 更新玩家自动出售装备设置请求。
    /// </summary>
    public class UpdateEquipmentAutoSellSettingsRequestDto
    {
        public int MinEquipmentLevel { get; set; }
        public int MinEquipmentQuality { get; set; }
    }

    /// <summary>
    /// 自动出售装备结果。
    /// </summary>
    public class EquipmentAutoSellResultDto
    {
        public int SoldCount { get; set; }
        public long GoldGained { get; set; }
    }

    /// <summary>
    /// 添加经验请求数据传输对象。
    /// </summary>
    public class AddExpRequestDto
    {
        /// <summary>
        /// 经验值
        /// </summary>
        public long Exp { get; set; }

        /// <summary>
        /// 经验值别名。
        /// 仅用于兼容旧版使用 <c>amount</c> 作为字段名的请求体。
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 经验来源
        /// </summary>
        public string Source { get; set; } = string.Empty;
    }

    /// <summary>
    /// 玩家灵根展示数据传输对象。
    /// 这里输出的是已经可直接展示的结果，前端无需再维护一套灵根映射和克制算法。
    /// </summary>
    public class PlayerSpiritRootDto
    {
        /// <summary>
        /// 灵根英文类型键。
        /// </summary>
        public string Type { get; set; } = "none";

        /// <summary>
        /// 灵根中文名称。
        /// </summary>
        public string Name { get; set; } = "无";

        /// <summary>
        /// 灵根图标。
        /// </summary>
        public string Icon { get; set; } = "○";

        /// <summary>
        /// 灵根说明文案。
        /// </summary>
        public string Description { get; set; } = "尚未觉醒灵根。";

        /// <summary>
        /// 当前灵根相关的克制关系。
        /// </summary>
        public List<PlayerSpiritRootCounterDto> Counters { get; set; } = [];
    }

    /// <summary>
    /// 灵根克制关系中的单条展示项。
    /// <c>Value</c> 统一使用百分比整数，前端可以直接渲染为 +15% / -10%。
    /// </summary>
    public class PlayerSpiritRootCounterDto
    {
        /// <summary>
        /// 对应灵根英文类型键。
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 对应灵根中文名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 对应灵根图标。
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// 克制修正值，单位百分比。
        /// </summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// 玩家属性加点总览数据传输对象。
    /// 这里集中返回总点数、已用点数、剩余点数以及每个属性的投入明细，避免前端自行推导导致刷新后不一致。
    /// </summary>
    public class PlayerAttributePointOverviewDto
    {
        /// <summary>
        /// 当前可分配点数。
        /// </summary>
        public int AvailablePoints { get; set; }

        /// <summary>
        /// 角色累计已获得的总点数。
        /// </summary>
        public int TotalEarnedPoints { get; set; }

        /// <summary>
        /// 已经投入到各属性中的总点数。
        /// </summary>
        public int TotalUsedPoints { get; set; }

        /// <summary>
        /// 各属性的当前投入明细。
        /// </summary>
        public List<PlayerAttributePointItemDto> Attributes { get; set; } = [];
    }

    /// <summary>
    /// 单条属性加点明细数据传输对象。
    /// 一个条目只表示一种可加点属性，例如气血、物攻、速度。
    /// </summary>
    public class PlayerAttributePointItemDto
    {
        /// <summary>
        /// 属性键。
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 属性中文名。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 对应底层属性类型名称，例如 Type1 / Type3。
        /// </summary>
        public string AttributeType { get; set; } = string.Empty;

        /// <summary>
        /// 当前已经投入的点数。
        /// </summary>
        public int AllocatedPoints { get; set; }

        /// <summary>
        /// 这些点数按整数规则换算出来的属性加成值。
        /// </summary>
        public int BonusValue { get; set; }

        /// <summary>
        /// 每次触发时增加的整数属性值。
        /// </summary>
        public int BonusPerPoint { get; set; }

        /// <summary>
        /// 累计多少个属性点触发一次收益。
        /// </summary>
        public int PointsPerBonus { get; set; } = 1;
    }

    /// <summary>
    /// 属性加点请求数据传输对象。
    /// 前端只需要传一个属性键，后端会自行完成合法性校验、点数校验和数据库落库。
    /// </summary>
    public class AdjustPlayerAttributePointRequestDto
    {
        /// <summary>
        /// 属性键，例如 type1 / type3。
        /// </summary>
        public string AttributeKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// 玩家突破状态数据传输对象。
    /// 这里把前端展示突破按钮所需的阶段名、当前层数、下一次需求和是否可突破一次性返回。
    /// </summary>
    public class PlayerBreakthroughDto
    {
        /// <summary>
        /// 当前境界排序。
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// 当前大境界名称。
        /// </summary>
        public string CurrentRealmName { get; set; } = "练气";

        /// <summary>
        /// 当前境界层数。
        /// </summary>
        public int CurrentRealmLayer { get; set; } = 1;

        /// <summary>
        /// 当前境界别名，例如练气一层。
        /// </summary>
        public string CurrentAlias { get; set; } = "练气一层";

        /// <summary>
        /// 下一境界名称。
        /// </summary>
        public string NextRealmName { get; set; } = string.Empty;

        /// <summary>
        /// 下一境界别名。
        /// </summary>
        public string NextAlias { get; set; } = string.Empty;

        /// <summary>
        /// 当前角色等级。
        /// </summary>
        public int CurrentLevel { get; set; }

        /// <summary>
        /// 下次突破所需等级。
        /// </summary>
        public int NextRequiredLevel { get; set; }

        /// <summary>
        /// 距离下次突破还差多少级。
        /// </summary>
        public int RemainingLevels { get; set; }

        /// <summary>
        /// 当前是否可突破。
        /// </summary>
        public bool CanBreakthrough { get; set; }

        /// <summary>
        /// 当前等级是否为突破关口。
        /// </summary>
        public bool IsBreakthroughPoint { get; set; }

        /// <summary>
        /// 突破成功率，百分比整数。
        /// </summary>
        public int BreakthroughSuccessRate { get; set; }

        /// <summary>
        /// 突破失败时扣除的经验百分比。
        /// </summary>
        public int BreakthroughExpLossPercent { get; set; }

        /// <summary>
        /// 当前突破需要的材料。
        /// </summary>
        public List<PlayerBreakthroughMaterialDto> RequiredMaterials { get; set; } = [];

        /// <summary>
        /// 当前属性加成百分比。
        /// </summary>
        public int AttributeBonusPercent { get; set; }

        /// <summary>
        /// 突破条件提示文案。
        /// </summary>
        public string RequirementText { get; set; } = string.Empty;
    }

    /// <summary>
    /// 玩家突破动作结果数据传输对象。
    /// 成功后除了提示文案，还会把最新角色信息和突破状态一起返回，方便前端一次刷新。
    /// </summary>
    public class PlayerBreakthroughResultDto
    {
        /// <summary>
        /// 本次突破是否成功。
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// 结果提示文案。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 突破前等级。
        /// </summary>
        public int PreviousLevel { get; set; }

        /// <summary>
        /// 突破后等级。
        /// </summary>
        public int CurrentLevel { get; set; }

        /// <summary>
        /// 本次掷出的随机值。
        /// </summary>
        public int RollValue { get; set; }

        /// <summary>
        /// 本次突破的目标成功率。
        /// </summary>
        public int SuccessRate { get; set; }

        /// <summary>
        /// 最新玩家快照。
        /// </summary>
        public PlayerDto Player { get; set; } = new();

        /// <summary>
        /// 最新突破状态。
        /// </summary>
        public PlayerBreakthroughDto Breakthrough { get; set; } = new();
    }

    /// <summary>
    /// 单条突破材料展示 DTO。
    /// </summary>
    public class PlayerBreakthroughMaterialDto
    {
        /// <summary>
        /// 材料物品 ID。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 材料名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 需求数量。
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 当前拥有数量。
        /// </summary>
        public int OwnedCount { get; set; }

        /// <summary>
        /// 当前是否满足数量要求。
        /// </summary>
        public bool IsEnough { get; set; }
    }
}
