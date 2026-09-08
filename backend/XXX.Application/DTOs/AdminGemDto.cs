namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台宝石模板列表项
    /// </summary>
    public class AdminGemListItemDto
    {
        public long Id { get; set; }
        public string GemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public string AttributeType { get; set; } = string.Empty;
        public int BonusValue { get; set; }
        public string BonusMode { get; set; } = "Flat";
        public int Quality { get; set; }
        public int SynthCount { get; set; }
        public string? SynthFromGemId { get; set; }
        public bool IsBuiltIn { get; set; }
        public int SynthSuccessRate { get; set; } = 100;
    }

    /// <summary>
    /// 后台宝石模板详情
    /// </summary>
    public class AdminGemDetailDto
    {
        public long Id { get; set; }
        public string GemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public string AttributeType { get; set; } = string.Empty;
        public int BonusValue { get; set; }
        public string BonusMode { get; set; } = "Flat";
        public string? IconPath { get; set; }
        public int Quality { get; set; }
        public int SynthCount { get; set; }
        public string? SynthFromGemId { get; set; }
        public bool IsBuiltIn { get; set; }
        public int SynthSuccessRate { get; set; } = 100;
    }

    /// <summary>
    /// 后台创建/编辑宝石模板
    /// </summary>
    public class AdminGemSaveDto
    {
        public string GemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public string AttributeType { get; set; } = "Type1";
        public int BonusValue { get; set; }
        public string BonusMode { get; set; } = "Flat";
        public string? IconPath { get; set; }
        public int Quality { get; set; } = 1;
        public int SynthCount { get; set; } = 3;
        public string? SynthFromGemId { get; set; }
        public int SynthSuccessRate { get; set; } = 100;
    }

    /// <summary>
    /// 后台批量生成宝石请求
    /// </summary>
    public class AdminGemBatchGenerateDto
    {
        /// <summary>属性类型</summary>
        public string AttributeType { get; set; } = "Type1";

        /// <summary>基础加成值</summary>
        public int BaseBonusValue { get; set; } = 10;

        /// <summary>每级加成增长</summary>
        public int BonusValueGrowth { get; set; } = 5;

        /// <summary>加成模式</summary>
        public string BonusMode { get; set; } = "Flat";

        /// <summary>生成等级数</summary>
        public int MaxLevel { get; set; } = 10;

        /// <summary>宝石名称前缀</summary>
        public string NamePrefix { get; set; } = "宝石";

        /// <summary>合成成功率（百分比，100=必定成功）</summary>
        public int SynthSuccessRate { get; set; } = 100;
    }
}
