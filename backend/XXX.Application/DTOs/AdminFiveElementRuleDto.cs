namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台聚灵阵主等级规则列表项。
    /// </summary>
    public class AdminFiveElementLevelRuleListItemDto
    {
        /// <summary>
        /// 聚灵阵主等级。
        /// </summary>
        public int ArrayLevel { get; set; }

        /// <summary>
        /// 升级所需金币。
        /// </summary>
        public long UpgradeGoldCost { get; set; }

        /// <summary>
        /// 升级所需灵石。
        /// </summary>
        public long UpgradeSpiritStoneCost { get; set; }

        /// <summary>
        /// 灵田产量加成百分比。
        /// </summary>
        public int SpiritFieldYieldBonusPercent { get; set; }

        /// <summary>
        /// 战斗经验加成百分比。
        /// </summary>
        public int BattleExpBonusPercent { get; set; }

        /// <summary>
        /// 当前聚灵阵允许达到的职业等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 当前规则是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子规则。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置规则版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台聚灵阵主等级规则详情。
    /// </summary>
    public class AdminFiveElementLevelRuleDetailDto
    {
        /// <summary>
        /// 聚灵阵主等级。
        /// </summary>
        public int ArrayLevel { get; set; }

        /// <summary>
        /// 升级所需金币。
        /// </summary>
        public long UpgradeGoldCost { get; set; }

        /// <summary>
        /// 升级所需灵石。
        /// </summary>
        public long UpgradeSpiritStoneCost { get; set; }

        /// <summary>
        /// 升级所需材料 JSON。
        /// </summary>
        public string UpgradeMaterialsJson { get; set; } = "[]";

        /// <summary>
        /// 灵田产量加成百分比。
        /// </summary>
        public int SpiritFieldYieldBonusPercent { get; set; }

        /// <summary>
        /// 战斗经验加成百分比。
        /// </summary>
        public int BattleExpBonusPercent { get; set; }

        /// <summary>
        /// 当前聚灵阵允许达到的职业等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 当前规则是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子规则。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 内置规则版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 后台五行等级区间规则列表项。
    /// </summary>
    public class AdminFiveElementBranchRuleRangeListItemDto
    {
        /// <summary>区间规则主键。</summary>
        public string GID { get; set; } = string.Empty;
        /// <summary>五行元素类型键。</summary>
        public string ElementType { get; set; } = string.Empty;
        /// <summary>区间起始等级。</summary>
        public int MinLevel { get; set; }
        /// <summary>区间结束等级。</summary>
        public int MaxLevel { get; set; }
        /// <summary>属性类型。</summary>
        public string AttributeType { get; set; } = string.Empty;
        /// <summary>每级属性加成。</summary>
        public int BonusPerLevel { get; set; }
        /// <summary>升级金币消耗。</summary>
        public long GoldCost { get; set; }
        /// <summary>升级灵石消耗。</summary>
        public long SpiritStoneCost { get; set; }
        /// <summary>规则是否启用。</summary>
        public bool IsEnabled { get; set; }
        /// <summary>是否为内置规则。</summary>
        public bool IsBuiltIn { get; set; }
        /// <summary>内置版本。</summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台五行等级区间规则详情。
    /// </summary>
    public class AdminFiveElementBranchRuleRangeDetailDto
    {
        /// <summary>区间规则主键。</summary>
        public string GID { get; set; } = string.Empty;
        /// <summary>五行元素类型键。</summary>
        public string ElementType { get; set; } = string.Empty;
        /// <summary>区间起始等级。</summary>
        public int MinLevel { get; set; }
        /// <summary>区间结束等级。</summary>
        public int MaxLevel { get; set; }
        /// <summary>属性类型。</summary>
        public string AttributeType { get; set; } = string.Empty;
        /// <summary>每级属性加成。</summary>
        public int BonusPerLevel { get; set; }
        /// <summary>升级金币消耗。</summary>
        public long GoldCost { get; set; }
        /// <summary>升级灵石消耗。</summary>
        public long SpiritStoneCost { get; set; }
        /// <summary>升级材料 JSON。</summary>
        public string MaterialsJson { get; set; } = "[]";
        /// <summary>展示排序。</summary>
        public int SortOrder { get; set; }
        /// <summary>规则是否启用。</summary>
        public bool IsEnabled { get; set; }
        /// <summary>是否为内置规则。</summary>
        public bool IsBuiltIn { get; set; }
        /// <summary>种子键。</summary>
        public string? SeedKey { get; set; }
        /// <summary>内置版本。</summary>
        public string? BuiltInVersion { get; set; }
        /// <summary>最后更新时间。</summary>
        public DateTime? LastUpdateTime { get; set; }
    }


    public class AdminFiveElementBranchRuleListItemDto
    {
        /// <summary>
        /// 分支规则主键。
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 五行元素类型键。
        /// </summary>
        public string ElementType { get; set; } = string.Empty;

        /// <summary>
        /// 目标分支等级。
        /// </summary>
        public int TargetLevel { get; set; }

        /// <summary>
        /// 升级所需金币。
        /// </summary>
        public long GoldCost { get; set; }

        /// <summary>
        /// 升级所需灵石。
        /// </summary>
        public long SpiritStoneCost { get; set; }

        /// <summary>
        /// 当前规则是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子规则。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置规则版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台五行分支升级规则详情。
    /// </summary>
    public class AdminFiveElementBranchRuleDetailDto
    {
        /// <summary>
        /// 分支规则主键。
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 五行元素类型键。
        /// </summary>
        public string ElementType { get; set; } = string.Empty;

        /// <summary>
        /// 目标分支等级。
        /// </summary>
        public int TargetLevel { get; set; }

        /// <summary>
        /// 升级所需金币。
        /// </summary>
        public long GoldCost { get; set; }

        /// <summary>
        /// 升级所需灵石。
        /// </summary>
        public long SpiritStoneCost { get; set; }

        /// <summary>
        /// 升级所需材料 JSON。
        /// </summary>
        public string MaterialsJson { get; set; } = "[]";

        /// <summary>
        /// 后台展示与加载顺序。
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 当前规则是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为内置种子规则。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 内置规则版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }
}
