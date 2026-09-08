using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 宝石模板实体
    /// </summary>
    [SugarTable("GemTemplates")]
    public class GemTemplateEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(Length = 50, IsNullable = false)]
        public string GemId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Level { get; set; } = 1;

        [SugarColumn(Length = 20, IsNullable = false)]
        public string AttributeType { get; set; } = "Type1";

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int BonusValue { get; set; }

        [SugarColumn(Length = 10, IsNullable = false, DefaultValue = "'Flat'")]
        public string BonusMode { get; set; } = "Flat";

        [SugarColumn(Length = 200, IsNullable = true)]
        public string? IconPath { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Quality { get; set; } = 1;

        [SugarColumn(IsNullable = false, DefaultValue = "3")]
        public int SynthCount { get; set; } = 3;

        [SugarColumn(Length = 50, IsNullable = true)]
        public string? SynthFromGemId { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsBuiltIn { get; set; }

        /// <summary>合成成功率（百分比，100=必定成功）</summary>
        [SugarColumn(IsNullable = false, DefaultValue = "100")]
        public int SynthSuccessRate { get; set; } = 100;
    }
}
