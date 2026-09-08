namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台境界配置列表项。
    /// </summary>
    public class AdminRealmLevelConfigListItemDto
    {
        /// <summary>
        /// 玩家等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 大境界名称。
        /// </summary>
        public string RealmName { get; set; } = string.Empty;

        /// <summary>
        /// 境界别名。
        /// 例如“练气一层”。
        /// </summary>
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// 大境界顺序。
        /// </summary>
        public int RealmOrder { get; set; }

        /// <summary>
        /// 当前大境界下的层数。
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// 达到该等级所需经验。
        /// </summary>
        public long RequiredExp { get; set; }

        /// <summary>
        /// 该境界提供的属性加成百分比。
        /// </summary>
        public int AttributeBonusPercent { get; set; }

        /// <summary>
        /// 该等级是否为突破关口。
        /// </summary>
        public bool IsBreakthroughPoint { get; set; }

        /// <summary>
        /// 是否为内置种子配置。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置配置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台单条突破材料配置。
    /// </summary>
    public class AdminRealmBreakthroughMaterialDto
    {
        /// <summary>
        /// 突破材料物品编号。
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 所需数量。
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 后台境界配置详情。
    /// </summary>
    public class AdminRealmLevelConfigDetailDto
    {
        /// <summary>
        /// 玩家等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 大境界名称。
        /// </summary>
        public string RealmName { get; set; } = string.Empty;

        /// <summary>
        /// 境界别名。
        /// </summary>
        public string Alias { get; set; } = string.Empty;

        /// <summary>
        /// 大境界顺序。
        /// </summary>
        public int RealmOrder { get; set; }

        /// <summary>
        /// 当前大境界下的层数。
        /// </summary>
        public int Layer { get; set; }

        /// <summary>
        /// 达到该等级所需经验。
        /// </summary>
        public long RequiredExp { get; set; }

        /// <summary>
        /// 该境界提供的属性加成百分比。
        /// </summary>
        public int AttributeBonusPercent { get; set; }

        /// <summary>
        /// 该等级是否为突破关口。
        /// </summary>
        public bool IsBreakthroughPoint { get; set; }

        /// <summary>
        /// 突破成功率，百分比整数。
        /// </summary>
        public int BreakthroughSuccessRate { get; set; }

        /// <summary>
        /// 突破失败时扣除的经验百分比。
        /// </summary>
        public int BreakthroughExpLossPercent { get; set; }

        /// <summary>
        /// 境界说明文案。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否为内置种子配置。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 内置配置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }

        /// <summary>
        /// 突破所需材料清单。
        /// </summary>
        public List<AdminRealmBreakthroughMaterialDto> BreakthroughMaterials { get; set; } = [];
    }
}
