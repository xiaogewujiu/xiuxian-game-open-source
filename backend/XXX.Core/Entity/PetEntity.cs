using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 灵宠模板实体
    /// 对应数据库灵宠模板表
    /// </summary>
    [SugarTable("PetTemplates")]
    public class PetTemplateEntity
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 灵宠名称
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 灵宠描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 灵宠类型
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public PetType Type { get; set; }

        /// <summary>
        /// 初始品质下限。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int InitialQualityMin { get; set; } = 1;

        /// <summary>
        /// 初始品质上限。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int InitialQualityMax { get; set; } = 1;

        /// <summary>
        /// 最高品质
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "5")]
        public int MaxQuality { get; set; } = 5;

        /// <summary>
        /// 成长率下限。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1.0")]
        public double GrowthRateMin { get; set; } = 1.0;

        /// <summary>
        /// 成长率上限。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1.0")]
        public double GrowthRateMax { get; set; } = 1.0;

        /// <summary>
        /// 初始技能数量。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int InitialSkillCount { get; set; } = 1;

        /// <summary>
        /// 元素。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public Element Element { get; set; } = Element.None;

        /// <summary>
        /// 生命下限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType1 { get; set; }

        /// <summary>
        /// 生命上限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType1 { get; set; }

        /// <summary>
        /// 法力下限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType2 { get; set; }

        /// <summary>
        /// 法力上限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType2 { get; set; }

        /// <summary>
        /// 物攻下限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType3 { get; set; }

        /// <summary>
        /// 物攻上限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType3 { get; set; }

        /// <summary>
        /// 法攻下限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType4 { get; set; }

        /// <summary>
        /// 法攻上限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType4 { get; set; }

        /// <summary>
        /// 物防下限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType5 { get; set; }

        /// <summary>
        /// 物防上限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType5 { get; set; }

        /// <summary>
        /// 法防下限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType6 { get; set; }

        /// <summary>
        /// 法防上限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType6 { get; set; }

        /// <summary>
        /// 速度下限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType7 { get; set; }

        /// <summary>
        /// 速度上限。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType7 { get; set; }

        /// <summary>
        /// 命中率下限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType8 { get; set; }

        /// <summary>
        /// 命中率上限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType8 { get; set; }

        /// <summary>
        /// 闪避率下限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType9 { get; set; }

        /// <summary>
        /// 闪避率上限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType9 { get; set; }

        /// <summary>
        /// 暴击率下限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType10 { get; set; }

        /// <summary>
        /// 暴击率上限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType10 { get; set; }

        /// <summary>
        /// 暴击伤害下限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType11 { get; set; }

        /// <summary>
        /// 暴击伤害上限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType11 { get; set; }

        /// <summary>
        /// 连击率下限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType12 { get; set; }

        /// <summary>
        /// 连击率上限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType12 { get; set; }

        /// <summary>
        /// 反击率下限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType13 { get; set; }

        /// <summary>
        /// 反击率上限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType13 { get; set; }

        /// <summary>
        /// 破甲率下限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType14 { get; set; }

        /// <summary>
        /// 破甲率上限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType14 { get; set; }

        /// <summary>
        /// 额外伤害下限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MinType15 { get; set; }

        /// <summary>
        /// 额外伤害上限，按 0-100 存模板值。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? MaxType15 { get; set; }

        /// <summary>
        /// 技能ID列表（JSON格式）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? SkillIdsJson { get; set; }

        /// <summary>
        /// 技能ID列表（非持久化）
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
        /// 获取方式
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? ObtainMethod { get; set; }

        /// <summary>
        /// 是否可交易
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsTradable { get; set; } = true;

        /// <summary>
        /// 内置种子键。
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        /// <summary>
        /// 内置版本号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 灵宠实例实体
    /// 对应数据库灵宠实例表
    /// </summary>
    [SugarTable("PetInstances")]
    public class PetInstanceEntity
    {
        /// <summary>
        /// 实例ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_player" })]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 模板ID
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 灵宠名称
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 当前等级
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Level { get; set; } = 1;

        /// <summary>
        /// 当前经验
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long Exp { get; set; } = 0;

        /// <summary>
        /// 升级所需经验
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "100")]
        public long XExp { get; set; } = 100;

        /// <summary>
        /// 当前品质
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Quality { get; set; } = 1;

        /// <summary>
        /// 当前成长率。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1.0")]
        public double GrowthRate { get; set; } = 1.0;

        /// <summary>
        /// 生命值。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "100")]
        public int Type1 { get; set; } = 100;

        /// <summary>
        /// 法力值。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "50")]
        public int Type2 { get; set; } = 50;

        /// <summary>
        /// 物理攻击。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "10")]
        public int Type3 { get; set; } = 10;

        /// <summary>
        /// 法术攻击。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "5")]
        public int Type4 { get; set; } = 5;

        /// <summary>
        /// 物理防御。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "5")]
        public int Type5 { get; set; } = 5;

        /// <summary>
        /// 法术防御。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "3")]
        public int Type6 { get; set; } = 3;

        /// <summary>
        /// 速度。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "10")]
        public int Type7 { get; set; } = 10;

        /// <summary>
        /// 命中率。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0.9")]
        public float Type8 { get; set; } = 0.9f;

        /// <summary>
        /// 闪避率。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public float Type9 { get; set; }

        /// <summary>
        /// 暴击率。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public float Type10 { get; set; }

        /// <summary>
        /// 暴击伤害倍率。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1.5")]
        public float Type11 { get; set; } = 1.5f;

        /// <summary>
        /// 连击率。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public float Type12 { get; set; }

        /// <summary>
        /// 反击率。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public float Type13 { get; set; }

        /// <summary>
        /// 破甲率。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public float Type14 { get; set; }

        /// <summary>
        /// 额外伤害。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public float Type15 { get; set; }

        /// <summary>
        /// 元素。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public Element Element { get; set; } = Element.None;

        /// <summary>
        /// 忠诚度（0-100）
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "100")]
        public int Loyalty { get; set; } = 100;

        /// <summary>
        /// 是否出战
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// 是否绑定
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsBound { get; set; } = true;

        /// <summary>
        /// 技能ID列表（JSON格式）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? SkillIdsJson { get; set; }

        /// <summary>
        /// 技能ID列表（非持久化）
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
        /// 获得时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime AcquiredTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
