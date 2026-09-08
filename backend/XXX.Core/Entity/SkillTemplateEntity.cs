using SqlSugar;
using System.Text.Json;

namespace XXX.Entity
{
    /// <summary>
    /// 技能模板落库实体。
    /// </summary>
    [SugarTable("SkillTemplates")]
    public class SkillTemplateEntity
    {
        public const string LegacyCatalog = "legacy";
        public const string CurrentCatalog = "current";

        [SugarColumn(IsPrimaryKey = true)]
        public int SkillId { get; set; }

        /// <summary>
        /// 技能等级；现有技能默认都是 1 级。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int SkillLevel { get; set; } = 1;

        /// <summary>
        /// 技能目录：legacy 为历史技能，current 为当前版本技能。
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, DefaultValue = "legacy")]
        public string SkillCatalog { get; set; } = LegacyCatalog;

        /// <summary>
        /// 上一级技能编号；没有上一级时为空。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? PreviousSkillId { get; set; }

        /// <summary>
        /// 下一级技能编号；没有下一级时为空。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? NextSkillId { get; set; }

        /// <summary>
        /// 当前技能升级到下一级所需的条件 JSON。
        /// </summary>
        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? UpgradeConditionsJson { get; set; }

        /// <summary>
        /// 当前技能到下一级的升级条件列表，非持久化访问器。
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<SkillUpgradeCondition> UpgradeConditions
        {
            get => SkillUpgradeConditionSerializer.Deserialize(UpgradeConditionsJson);
            set => UpgradeConditionsJson = SkillUpgradeConditionSerializer.Serialize(value);
        }

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(Length = 1000, IsNullable = false)]
        public string Description { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int TargetType { get; set; }

        [SugarColumn(IsNullable = false)]
        public int ManaCost { get; set; }

        [SugarColumn(IsNullable = false)]
        public int Cooldown { get; set; }

        [SugarColumn(IsNullable = false)]
        public int DamageType { get; set; }

        [SugarColumn(IsNullable = false)]
        public int HitCount { get; set; }

        [SugarColumn(IsNullable = false)]
        public int RangeType { get; set; }

        [SugarColumn(IsNullable = false)]
        public double DamageMultiplier { get; set; }

        [SugarColumn(IsNullable = false)]
        public double TriggerChance { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? HitsJson { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? BuffIdsJson { get; set; }

        [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
        public string? AllowedProfessionsJson { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        [SugarColumn(IsIgnore = true)]
        public List<SkillHit> Hits
        {
            get => DeserializeList<SkillHit>(HitsJson);
            set => HitsJson = SerializeList(value);
        }

        [SugarColumn(IsIgnore = true)]
        public List<string> BuffIds
        {
            get => DeserializeList<string>(BuffIdsJson);
            set => BuffIdsJson = SerializeList(value);
        }

        [SugarColumn(IsIgnore = true)]
        public List<string> AllowedProfessions
        {
            get => PlayerProfessionCatalog.NormalizeAllowedProfessions(DeserializeList<string>(AllowedProfessionsJson));
            set => AllowedProfessionsJson = SerializeList(PlayerProfessionCatalog.NormalizeAllowedProfessions(value));
        }

        private static List<T> DeserializeList<T>(string? json)
        {
            return string.IsNullOrWhiteSpace(json)
                ? []
                : JsonSerializer.Deserialize<List<T>>(json) ?? [];
        }

        private static string SerializeList<T>(List<T>? items)
        {
            return JsonSerializer.Serialize(items ?? []);
        }
    }
}
