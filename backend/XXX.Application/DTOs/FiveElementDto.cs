namespace XXX.Application.DTOs
{
    /// <summary>
    /// 五行聚灵阵总览数据。
    /// </summary>
    public class FiveElementDto
    {
        /// <summary>
        /// 聚灵阵总等级。
        /// </summary>
        public int ArrayLevel { get; set; }

        /// <summary>
        /// 金系等级。
        /// </summary>
        public int MetalLevel { get; set; }

        /// <summary>
        /// 木系等级。
        /// </summary>
        public int WoodLevel { get; set; }

        /// <summary>
        /// 水系等级。
        /// </summary>
        public int WaterLevel { get; set; }

        /// <summary>
        /// 火系等级。
        /// </summary>
        public int FireLevel { get; set; }

        /// <summary>
        /// 土系等级。
        /// </summary>
        public int EarthLevel { get; set; }
        /// <summary>
        /// 金系经验。
        /// </summary>
        public long MetalExp { get; set; }
        /// <summary>
        /// 木系经验。
        /// </summary>
        public long WoodExp { get; set; }
        /// <summary>
        /// 水系经验。
        /// </summary>
        public long WaterExp { get; set; }
        /// <summary>
        /// 火系经验。
        /// </summary>
        public long FireExp { get; set; }
        /// <summary>
        /// 土系经验。
        /// </summary>
        public long EarthExp { get; set; }
        /// <summary>
        /// 当前总灵力。
        /// </summary>
        public long TotalSpiritPower { get; set; }
        /// <summary>
        /// 灵力产出速率。
        /// </summary>
        public int SpiritPowerRate { get; set; }
        /// <summary>
        /// 今日收取次数。
        /// </summary>
        public int TodayCollectCount { get; set; }
        /// <summary>
        /// 历史累计收取灵力。
        /// </summary>
        public long TotalCollectedSpiritPower { get; set; }
        /// <summary>
        /// 下一级聚灵阵升级消耗。
        /// </summary>
        public int NextArrayUpgradeCost { get; set; }

        /// <summary>
        /// 聚灵阵升级成本详情。
        /// </summary>
        public FiveElementUpgradeCostDto ArrayUpgradeCost { get; set; } = new();

        /// <summary>
        /// 五行分支升级成本详情。
        /// </summary>
        public List<FiveElementUpgradeCostDto> ElementUpgradeCosts { get; set; } = [];

        /// <summary>
        /// 聚灵阵对灵田产量的加成百分比。
        /// </summary>
        public int SpiritFieldYieldBonusPercent { get; set; }

        /// <summary>
        /// 聚灵阵对战斗经验的加成百分比。
        /// </summary>
        public int BattleExpBonusPercent { get; set; }

        /// <summary>
        /// 聚灵阵对炼丹师/锻造师提供的等级上限。
        /// </summary>
        public int ProfessionLevelCap { get; set; }

        /// <summary>
        /// 当前激活组合效果列表。
        /// </summary>
        public List<string> ActiveCombinations { get; set; } = [];

        /// <summary>
        /// 五行分支当前实际加成列表。
        /// </summary>
        public List<FiveElementBonusDto> ElementBonuses { get; set; } = [];
    }

    /// <summary>
    /// 五行分支当前实际加成数据。
    /// </summary>
    public class FiveElementBonusDto
    {
        /// <summary>五行类型键。</summary>
        public string ElementType { get; set; } = string.Empty;
        /// <summary>五行中文名称。</summary>
        public string ElementName { get; set; } = string.Empty;
        /// <summary>当前五行等级。</summary>
        public int Level { get; set; }
        /// <summary>当前聚灵阵允许的五行等级上限。</summary>
        public int MaxLevel { get; set; }
        /// <summary>对应角色属性类型。</summary>
        public string AttributeType { get; set; } = string.Empty;
        /// <summary>对应角色属性名称。</summary>
        public string AttributeName { get; set; } = string.Empty;
        /// <summary>当前累计属性加成。</summary>
        public int CurrentBonus { get; set; }
        /// <summary>当前等级区间每级加成。</summary>
        public int BonusPerLevel { get; set; }
    }

    /// <summary>
    /// 升级单个五行分支的请求。
    /// </summary>
    public class UpgradeElementRequestDto
    {
        /// <summary>
        /// 要升级的五行分支类型。
        /// </summary>
        public string ElementType { get; set; } = string.Empty;
    }

    /// <summary>
    /// 收取灵力后的返回结果。
    /// </summary>
    public class CollectSpiritPowerResultDto
    {
        /// <summary>
        /// 本次收取是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 本次收取数量。
        /// </summary>
        public long CollectedAmount { get; set; }

        /// <summary>
        /// 收取后的灵力总量。
        /// </summary>
        public long TotalSpiritPower { get; set; }

        /// <summary>
        /// 结果提示文案。
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 五行升级成本。
    /// </summary>
    public class FiveElementUpgradeCostDto
    {
        /// <summary>
        /// 成本对应的分支类型。聚灵阵主等级时为 array。
        /// </summary>
        public string ElementType { get; set; } = string.Empty;

        /// <summary>
        /// 金币消耗。
        /// </summary>
        public long GoldCost { get; set; }

        /// <summary>
        /// 灵石消耗。
        /// </summary>
        public long SpiritStoneCost { get; set; }

        /// <summary>
        /// 道具消耗。
        /// </summary>
        public List<FiveElementUpgradeMaterialDto> Materials { get; set; } = [];
    }

    /// <summary>
    /// 五行升级材料。
    /// </summary>
    public class FiveElementUpgradeMaterialDto
    {
        /// <summary>材料物品编号。</summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>材料物品名称。</summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>所需数量。</summary>
        public int Count { get; set; }
    }
}
