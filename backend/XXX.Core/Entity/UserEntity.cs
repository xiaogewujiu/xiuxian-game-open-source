using SqlSugar;
using System.Text.Json.Serialization;
namespace XXX.Entity
{
    /// <summary>
    /// 玩家实体。
    /// 对应数据库中的 Users 表。
    /// </summary>
    [SugarTable("Users")]
    public class UserEntity : BaseAttributes

    {
        /// <summary>
        /// 主键，玩家唯一编号。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 道号，也就是玩家展示名称。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_name" })]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 玩家战斗职业。
        /// 支持：warrior / mage / body。
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, DefaultValue = "warrior")]
        public string Profession { get; set; } = PlayerProfessionCatalog.Warrior;

        /// <summary>
        /// 当前佩戴的玩家称号。
        /// 主要由成就奖励、排行奖励等展示型系统写入。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? CurrentTitle { get; set; }

        /// <summary>
        /// 玩家头像图片路径。
        /// 未上传时保持为空，前端继续回退到默认头像图标。
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? AvatarImagePath { get; set; }

        /// <summary>
        /// 登录账号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, IndexGroupNameList = new[] { "idx_account" })]
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 密码哈希。
        /// </summary>
        [SugarColumn(Length = 256, IsNullable = false)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// 等级。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Level { get; set; } = 1;

        /// <summary>
        /// 当前经验。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Exp { get; set; } = 0;

        /// <summary>
        /// 升级所需经验。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "100")]
        public long XExp { get; set; } = 100;

        /// <summary>
        /// 第二经验槽当前值。
        /// 当前主要作为旧成长链兼容字段保留。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int Exp2 { get; set; } = 0;

        /// <summary>
        /// 第二经验槽所需值。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int XExp2 { get; set; } = 0;

        /// <summary>
        /// 已装备技能 ID 列表，使用 JSON 持久化。
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        [JsonIgnore]
        public string? SkillIdsJson { get; set; }

        /// <summary>
        /// 已装备技能 ID 列表，非持久化访问器。
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<string> SkillIds

        {
            get => string.IsNullOrEmpty(SkillIdsJson)

                ? []

                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(SkillIdsJson) ?? [];

            set => SkillIdsJson = System.Text.Json.JsonSerializer.Serialize(value);

        }
        /// <summary>
        /// 已掌握技能 ID 列表，使用 JSON 持久化。
        /// `SkillIds` 只表示当前装备技能；这里单独保留已掌握集合，避免卸下技能后丢失记录。
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        [JsonIgnore]
        public string? OwnedSkillIdsJson { get; set; }

        /// <summary>
        /// 已掌握技能 ID 列表，非持久化访问器。
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<string> OwnedSkillIds

        {
            get => string.IsNullOrEmpty(OwnedSkillIdsJson)

                ? []

                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(OwnedSkillIdsJson) ?? [];

            set => OwnedSkillIdsJson = System.Text.Json.JsonSerializer.Serialize(value);

        }
        /// <summary>
        /// 当前出战灵宠实例 ID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? PetId { get; set; }

        /// <summary>
        /// 公会 ID。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, IndexGroupNameList = new[] { "idx_guild" })]
        public string? GuildId { get; set; }

        /// <summary>
        /// 被动 Buff ID 列表，使用 JSON 持久化。
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        [JsonIgnore]
        public string? PassiveIdsJson { get; set; }

        /// <summary>
        /// 被动 Buff ID 列表，非持久化访问器。
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<string> PassiveIds

        {
            get => string.IsNullOrEmpty(PassiveIdsJson)

                ? []

                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(PassiveIdsJson) ?? [];

            set => PassiveIdsJson = System.Text.Json.JsonSerializer.Serialize(value);

        }
        /// <summary>
        /// 额外属性列表，使用 JSON 持久化。
        /// 当前主要保留给非属性点类的额外成长数据；属性点分配已迁到独立表。
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        [JsonIgnore]
        public string? AttributesJson { get; set; }

        /// <summary>
        /// 额外属性列表，非持久化访问器。
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<AttributeProperty> Attributes

        {
            get => string.IsNullOrEmpty(AttributesJson)

                ? []

                : System.Text.Json.JsonSerializer.Deserialize<List<AttributeProperty>>(AttributesJson) ?? [];

            set => AttributesJson = System.Text.Json.JsonSerializer.Serialize(value);

        }

        /// <summary>
        /// 下次突破额外成功率加成。
        /// 主要由突破类丹药提供，成功或失败后都会清空。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int BreakthroughBonusPercent { get; set; } = 0;

        /// <summary>
        /// 丹药属性加成列表，JSON 持久化。
        /// 单独存储丹药提供的属性加成，在属性点分配之后叠加，
        /// 避免被 RecalculatePlayerAttributesAsync 中的属性点重算覆盖。
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        [JsonIgnore]
        public string? PillBonusAttributesJson { get; set; }

        /// <summary>
        /// 丹药属性加成列表，非持久化访问器。
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<AttributeProperty> PillBonusAttributes
        {
            get => string.IsNullOrEmpty(PillBonusAttributesJson)
                ? []
                : System.Text.Json.JsonSerializer.Deserialize<List<AttributeProperty>>(PillBonusAttributesJson) ?? [];
            set => PillBonusAttributesJson = System.Text.Json.JsonSerializer.Serialize(value);
        }

        // 货币字段
        /// <summary>
        /// 金币，基础货币。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1000")]
        public long Gold { get; set; } = 1000;

        /// <summary>
        /// 装备背包容量。未扩容玩家默认为 100 格。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "100")]
        public int EquipmentInventoryCapacity { get; set; } = 100;

        /// <summary>
        /// 道具背包容量。未扩容玩家默认为 100 格。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "100")]
        public int ItemInventoryCapacity { get; set; } = 100;

        /// <summary>
        /// 荣誉点，主要用于 PVP 相关兑换。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int Honor { get; set; } = 0;

        /// <summary>
        /// 公会贡献。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int GuildContribution { get; set; } = 0;

        /// <summary>
        /// 灵石，修仙体系货币。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long SpiritStone { get; set; } = 0;

        // 时间字段
        /// <summary>
        /// 创建时间。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "getdate()")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后登录时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastLoginTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 战斗冷却截止时间（UTC）。
        /// 为空表示当前没有战斗冷却。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? BattleCooldownUntilUtc { get; set; }

        /// <summary>
        /// 当前账号战斗模式。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public BattleMode BattleMode { get; set; } = BattleMode.Normal;

        /// <summary>
        /// 当前离线挂机地图编号。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? OfflineBattleMapId { get; set; }

        /// <summary>
        /// 当前离线挂机开始时间（UTC）。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? OfflineBattleStartedAtUtc { get; set; }

        /// <summary>
        /// 当前离线挂机最后一次推进时间（UTC）。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? OfflineBattleLastTickAtUtc { get; set; }

        /// <summary>
        /// 当前离线挂机结束时间（UTC）。单次最长 48 小时。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? OfflineBattleEndAtUtc { get; set; }

        /// <summary>
        /// 当前离线挂机会话的累计汇总 JSON。
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? OfflineBattleSummaryJson { get; set; }

        /// <summary>
        /// 当前活跃的秘境实例 ID。null 表示不在秘境中。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true)]
        public string? ActiveDungeonInstanceId { get; set; }

        /// <summary>
        /// 自动出售装备的最低保留等级。0 表示不按等级限制。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int EquipmentAutoSellMinLevel { get; set; } = 0;

        /// <summary>
        /// 自动出售装备的最低保留品质。0 表示不按品质限制。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int EquipmentAutoSellMinQuality { get; set; } = 0;

        /// <summary>
        /// 是否已被封禁。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBanned { get; set; } = false;

        /// <summary>
        /// 封禁原因。
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? BanReason { get; set; }

        /// <summary>
        /// 封禁截止时间。
        /// 为空表示长期封禁。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? BanExpiresAt { get; set; }

        // 统计字段
        /// <summary>
        /// 总战斗场次。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TotalBattles { get; set; } = 0;

        /// <summary>
        /// 胜利场次。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int WinBattles { get; set; } = 0;

        /// <summary>
        /// 总击杀数。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int TotalKills { get; set; } = 0;

        /// <summary>
        /// 最大连击数。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int MaxCombo { get; set; } = 0;

        /// <summary>
        /// 累计获得金币。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1000")]
        public long TotalGoldEarned { get; set; } = 1000;

        /// <summary>
        /// 累计消耗金币。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long TotalGoldSpent { get; set; } = 0;

        /// <summary>
        /// 累计获得经验。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long TotalExpEarned { get; set; } = 0;

        /// <summary>
        /// 是否删除，使用软删除标记。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsDeleted { get; set; } = false;

    }
}
