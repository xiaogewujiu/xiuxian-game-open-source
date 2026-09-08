using SqlSugar;
using System.Text.Json;

namespace XXX.Entity
{
    /// <summary>
    /// 等级到境界映射配置。
    /// 一行代表一个玩家等级，对应当前境界展示、属性加成与突破规则。
    /// </summary>
    [SugarTable("RealmLevelConfigs")]
    public class RealmLevelConfigEntity
    {
        /// <summary>
        /// 对应玩家等级，直接作为主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsNullable = false)]
        public int Level { get; set; }

        /// <summary>
        /// 大境界名称，例如练气、筑基。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string RealmName { get; set; } = string.Empty;

        /// <summary>
        /// 前端直接显示的别名，例如练气一层。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// 大境界排序。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int RealmOrder { get; set; }

        /// <summary>
        /// 当前境界中的层数。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int Layer { get; set; }

        /// <summary>
        /// 当前等级升到下一级所需经验。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long RequiredExp { get; set; }

        /// <summary>
        /// 当前等级对应的总属性加成百分比。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int AttributeBonusPercent { get; set; }

        /// <summary>
        /// 当前等级是否为突破关口。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsBreakthroughPoint { get; set; }

        /// <summary>
        /// 突破成功率，百分比整数。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int BreakthroughSuccessRate { get; set; } = 100;

        /// <summary>
        /// 突破失败时扣除的当前等级经验百分比。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int BreakthroughExpLossPercent { get; set; } = 0;

        /// <summary>
        /// 突破材料 JSON。
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT")]
        public string? BreakthroughMaterialsJson { get; set; }

        /// <summary>
        /// 说明文案。
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string? Description { get; set; }

        /// <summary>
        /// 内置稳定键。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string? SeedKey { get; set; }

        /// <summary>
        /// 是否为内置配置。
        /// </summary>
        [SugarColumn(IsNullable = false)]
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

        /// <summary>
        /// 运行时使用的突破材料列表。
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<RealmBreakthroughMaterial> BreakthroughMaterials
        {
            get => string.IsNullOrWhiteSpace(BreakthroughMaterialsJson)
                ? []
                : JsonSerializer.Deserialize<List<RealmBreakthroughMaterial>>(BreakthroughMaterialsJson) ?? [];
            set => BreakthroughMaterialsJson = JsonSerializer.Serialize(value ?? []);
        }
    }

    /// <summary>
    /// 单条突破材料需求。
    /// </summary>
    public class RealmBreakthroughMaterial
    {
        public string ItemId { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
