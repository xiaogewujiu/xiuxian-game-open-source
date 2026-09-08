using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 装备模板落库实体
    /// </summary>
    [SugarTable("EquipmentTemplates")]
    public class EquipmentTemplateEntity : BaseAttributesTemplate
    {
        [SugarColumn(IsPrimaryKey = true)]
        public int EquipmentId { get; set; }

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public int Level { get; set; }

        [SugarColumn(IsNullable = false)]
        public int Quality { get; set; }

        [SugarColumn(IsNullable = false)]
        public int Slot { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int CombatStyle { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int WeaponCategory { get; set; }

        [SugarColumn(Length = 2000, IsNullable = false)]
        public string Description { get; set; } = string.Empty;

        [SugarColumn(Length = 500, IsNullable = true)]
        public string? IconPath { get; set; }

        [SugarColumn(Length = 200, IsNullable = true)]
        public string? SeedKey { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; } = false;

        [SugarColumn(Length = 100, IsNullable = true)]
        public string? BuiltInVersion { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsTradeable { get; set; } = true;
    }
}
