namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台属性点配置总览。
    /// </summary>
    public class AdminAttributePointConfigBundleDto
    {
        /// <summary>
        /// 当前配置是否来自内置种子数据。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置配置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 最近一次编辑时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }

        /// <summary>
        /// 三职业的属性倍率定义集合。
        /// </summary>
        public List<AdminAttributePointDefinitionDto> Attributes { get; set; } = [];

        /// <summary>
        /// 不同等级区间的给点规则集合。
        /// </summary>
        public List<AdminAttributePointLevelRangeDto> LevelRanges { get; set; } = [];
    }

    /// <summary>
    /// 后台属性点整数收益定义。
    /// </summary>
    public class AdminAttributePointDefinitionDto
    {
        /// <summary>
        /// 职业键。
        /// 例如 <c>warrior</c>、<c>mage</c>、<c>body</c>。
        /// </summary>
        public string Profession { get; set; } = XXX.Entity.PlayerProfessionCatalog.Warrior;

        /// <summary>
        /// 属性键。
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 属性中文名。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 对应底层属性类型编号。
        /// </summary>
        public int AttributeType { get; set; }

        /// <summary>
        /// 每次触发时增加的整数属性值。
        /// </summary>
        public int BonusPerPoint { get; set; }

        /// <summary>
        /// 累计多少个属性点触发一次收益。
        /// </summary>
        public int PointsPerBonus { get; set; } = 1;
    }

    /// <summary>
    /// 后台属性点等级区间给点规则。
    /// </summary>
    public class AdminAttributePointLevelRangeDto
    {
        /// <summary>
        /// 区间起始等级。
        /// </summary>
        public int LevelStart { get; set; }

        /// <summary>
        /// 区间结束等级。
        /// </summary>
        public int LevelEnd { get; set; }

        /// <summary>
        /// 玩家在该等级区间内每级获得的属性点数。
        /// </summary>
        public int PointsGained { get; set; }
    }
}
