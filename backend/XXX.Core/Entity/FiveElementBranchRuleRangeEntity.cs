using System;
using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 五行分支等级区间规则。
    /// 同一元素在区间内复用属性映射、每级加成和升级消耗，减少规则维护量。
    /// </summary>
    [SugarTable("FiveElementBranchRuleRanges")]
    public class FiveElementBranchRuleRangeEntity
    {
        /// <summary>区间规则主键。</summary>
        [SugarColumn(IsPrimaryKey = true, Length = 80)]
        public string GID { get; set; } = string.Empty;
        /// <summary>五行类型键。</summary>
        [SugarColumn(Length = 20)]
        public string ElementType { get; set; } = string.Empty;
        /// <summary>区间起始等级。</summary>
        [SugarColumn] public int MinLevel { get; set; }
        /// <summary>区间结束等级。</summary>
        [SugarColumn] public int MaxLevel { get; set; }
        /// <summary>对应角色属性类型，例如 Type3。</summary>
        [SugarColumn(Length = 20)] public string AttributeType { get; set; } = string.Empty;
        /// <summary>每提升一级增加的固定属性值。</summary>
        [SugarColumn] public int BonusPerLevel { get; set; }
        /// <summary>区间内每次升级所需金币。</summary>
        [SugarColumn] public long GoldCost { get; set; }
        /// <summary>区间内每次升级所需灵石。</summary>
        [SugarColumn] public long SpiritStoneCost { get; set; }
        /// <summary>区间内每次升级所需材料 JSON。</summary>
        [SugarColumn(Length = 1000, IsNullable = true)] public string? MaterialsJson { get; set; }
        /// <summary>后台显示顺序。</summary>
        [SugarColumn] public int SortOrder { get; set; }
        /// <summary>规则是否启用。</summary>
        [SugarColumn] public bool IsEnabled { get; set; } = true;
        /// <summary>内置种子键。</summary>
        [SugarColumn(Length = 120, IsNullable = true)] public string? SeedKey { get; set; }
        /// <summary>是否为内置规则。</summary>
        [SugarColumn] public bool IsBuiltIn { get; set; }
        /// <summary>内置规则版本号。</summary>
        [SugarColumn(Length = 100, IsNullable = true)] public string? BuiltInVersion { get; set; }
        /// <summary>最近更新时间。</summary>
        [SugarColumn] public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
