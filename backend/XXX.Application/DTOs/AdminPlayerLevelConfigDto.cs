namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台等级成长配置列表项。
    /// </summary>
    public class AdminPlayerLevelConfigListItemDto
    {
        /// <summary>
        /// 玩家等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 升到下一级所需经验。
        /// </summary>
        public long RequiredExp { get; set; }

        /// <summary>
        /// 该等级基础气血值。
        /// </summary>
        public int BaseHp { get; set; }

        /// <summary>
        /// 该等级基础灵力值。
        /// </summary>
        public int BaseMp { get; set; }

        /// <summary>
        /// 该等级基础物攻。
        /// </summary>
        public int BasePhysicalAttack { get; set; }

        /// <summary>
        /// 该等级基础法攻。
        /// </summary>
        public int BaseMagicAttack { get; set; }

        /// <summary>
        /// 该等级基础物防。
        /// </summary>
        public int BasePhysicalDefense { get; set; }

        /// <summary>
        /// 该等级基础法防。
        /// </summary>
        public int BaseMagicDefense { get; set; }

        /// <summary>
        /// 该等级基础速度。
        /// </summary>
        public int BaseSpeed { get; set; }

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
    /// 后台等级成长配置详情。
    /// </summary>
    public class AdminPlayerLevelConfigDetailDto
    {
        /// <summary>
        /// 玩家等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 升到下一级所需经验。
        /// </summary>
        public long RequiredExp { get; set; }

        /// <summary>
        /// 该等级基础气血值。
        /// </summary>
        public int BaseHp { get; set; }

        /// <summary>
        /// 该等级基础灵力值。
        /// </summary>
        public int BaseMp { get; set; }

        /// <summary>
        /// 该等级基础物攻。
        /// </summary>
        public int BasePhysicalAttack { get; set; }

        /// <summary>
        /// 该等级基础法攻。
        /// </summary>
        public int BaseMagicAttack { get; set; }

        /// <summary>
        /// 该等级基础物防。
        /// </summary>
        public int BasePhysicalDefense { get; set; }

        /// <summary>
        /// 该等级基础法防。
        /// </summary>
        public int BaseMagicDefense { get; set; }

        /// <summary>
        /// 该等级基础速度。
        /// </summary>
        public int BaseSpeed { get; set; }

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
    }
}
