namespace XXX.Application.DTOs
{
    /// <summary>
    /// 宝石模板概要
    /// </summary>
    public class GemTemplateDto
    {
        /// <summary>宝石ID</summary>
        public string GemId { get; set; } = string.Empty;

        /// <summary>宝石名称</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>宝石等级</summary>
        public int Level { get; set; }

        /// <summary>加成属性类型</summary>
        public string AttributeType { get; set; } = string.Empty;

        /// <summary>加成数值</summary>
        public int BonusValue { get; set; }

        /// <summary>加成模式: Flat=固定值, Percent=百分比</summary>
        public string BonusMode { get; set; } = "Flat";

        /// <summary>图标路径</summary>
        public string? IconPath { get; set; }

        /// <summary>品质</summary>
        public int Quality { get; set; }

        /// <summary>合成所需数量</summary>
        public int SynthCount { get; set; }

        /// <summary>合成来源宝石ID</summary>
        public string? SynthFromGemId { get; set; }

        /// <summary>合成成功率（百分比）</summary>
        public int SynthSuccessRate { get; set; } = 100;
    }

    /// <summary>
    /// 背包中的宝石
    /// </summary>
    public class GemInventoryDto
    {
        /// <summary>背包物品ID</summary>
        public long InventoryItemId { get; set; }

        /// <summary>宝石ID</summary>
        public string GemId { get; set; } = string.Empty;

        /// <summary>宝石名称</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>宝石等级</summary>
        public int Level { get; set; }

        /// <summary>加成属性类型</summary>
        public string AttributeType { get; set; } = string.Empty;

        /// <summary>加成数值</summary>
        public int BonusValue { get; set; }

        /// <summary>加成模式</summary>
        public string BonusMode { get; set; } = "Flat";

        /// <summary>品质</summary>
        public int Quality { get; set; }

        /// <summary>数量</summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// 装备宝石孔位信息
    /// </summary>
    public class EquipmentGemSlotDto
    {
        /// <summary>孔位索引</summary>
        public int SlotIndex { get; set; }

        /// <summary>已镶嵌宝石ID（null表示空）</summary>
        public string? GemId { get; set; }

        /// <summary>宝石名称</summary>
        public string? GemName { get; set; }

        /// <summary>宝石等级</summary>
        public int? GemLevel { get; set; }

        /// <summary>加成描述</summary>
        public string? BonusText { get; set; }
    }

    /// <summary>
    /// 镶嵌宝石请求
    /// </summary>
    public class SocketGemDto
    {
        /// <summary>装备实例ID</summary>
        public string EquipmentInstanceId { get; set; } = string.Empty;

        /// <summary>孔位索引</summary>
        public int SlotIndex { get; set; }

        /// <summary>背包中的宝石物品ID</summary>
        public long InventoryItemId { get; set; }
    }

    /// <summary>
    /// 取下宝石请求
    /// </summary>
    public class UnsocketGemDto
    {
        /// <summary>装备实例ID</summary>
        public string EquipmentInstanceId { get; set; } = string.Empty;

        /// <summary>孔位索引</summary>
        public int SlotIndex { get; set; }
    }

    /// <summary>
    /// 宝石合成请求
    /// </summary>
    public class SynthesizeGemDto
    {
        /// <summary>宝石ID（目标宝石）</summary>
        public string GemId { get; set; } = string.Empty;

        /// <summary>背包中的源宝石物品ID（后端自动按数量扣减）</summary>
        public long InventoryItemId { get; set; }
    }
}
