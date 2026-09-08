using XXX.Entity;

namespace XXX.Application.DTOs
{
    /// <summary>
    /// 后台地图列表项。
    /// </summary>
    public class AdminMapListItemDto
    {
        /// <summary>
        /// 地图编号。
        /// </summary>
        public string MapId { get; set; } = string.Empty;

        /// <summary>
        /// 地图名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 下一层地图编号。
        /// </summary>
        public string? NextMapId { get; set; }

        /// <summary>
        /// 最小怪物数量。
        /// </summary>
        public int MonsterCountMin { get; set; }

        /// <summary>
        /// 最大怪物数量。
        /// </summary>
        public int MonsterCountMax { get; set; }

        /// <summary>
        /// 刷怪规则数量。
        /// </summary>
        public int SpawnRuleCount { get; set; }

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }
    }

    /// <summary>
    /// 后台地图详情。
    /// </summary>
    public class AdminMapDetailDto
    {
        /// <summary>
        /// 地图编号。
        /// </summary>
        public string MapId { get; set; } = string.Empty;

        /// <summary>
        /// 地图名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 地图描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 下一层地图编号。
        /// </summary>
        public string? NextMapId { get; set; }

        /// <summary>
        /// 是否继承上一层状态。
        /// </summary>
        public bool Carrying { get; set; }

        /// <summary>
        /// 最小怪物数量。
        /// </summary>
        public int MonsterCountMin { get; set; }

        /// <summary>
        /// 最大怪物数量。
        /// </summary>
        public int MonsterCountMax { get; set; }

        /// <summary>
        /// 刷怪规则列表。
        /// </summary>
        public List<MapMonsterSpawnRule> SpawnRules { get; set; } = [];

        /// <summary>
        /// 是否为内置模板。
        /// </summary>
        public bool IsBuiltIn { get; set; }

        /// <summary>
        /// 内置种子键。
        /// </summary>
        public string? SeedKey { get; set; }

        /// <summary>
        /// 内置版本号。
        /// </summary>
        public string? BuiltInVersion { get; set; }

        /// <summary>
        /// 副本地图链是否有效。
        /// </summary>
        public bool ChainValid { get; set; }

        /// <summary>
        /// 副本链错误信息。
        /// </summary>
        public string? ChainError { get; set; }

        /// <summary>
        /// 当前副本引用该地图的入口列表。
        /// </summary>
        public List<string> ReferencingDungeonIds { get; set; } = [];

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 保存地图请求。
    /// </summary>
    public class AdminSaveMapRequestDto
    {
        /// <summary>
        /// 地图编号。
        /// </summary>
        public string MapId { get; set; } = string.Empty;

        /// <summary>
        /// 地图名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 推荐等级。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 地图描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 下一层地图编号。
        /// </summary>
        public string? NextMapId { get; set; }

        /// <summary>
        /// 是否继承上一层状态。
        /// </summary>
        public bool Carrying { get; set; }

        /// <summary>
        /// 最小怪物数量。
        /// </summary>
        public int MonsterCountMin { get; set; }

        /// <summary>
        /// 最大怪物数量。
        /// </summary>
        public int MonsterCountMax { get; set; }

        /// <summary>
        /// 刷怪规则。
        /// </summary>
        public List<MapMonsterSpawnRule> SpawnRules { get; set; } = [];
    }
}
