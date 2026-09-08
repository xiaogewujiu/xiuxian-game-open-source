#nullable enable
namespace XXX.Entity
{
    /// <summary>
    /// 道具掉落配置。
    /// </summary>
    public class DropItem
    {
        public string ItemId { get; set; } = string.Empty;
        public int Rate { get; set; }
    }

    /// <summary>
    /// 装备掉落配置。
    /// </summary>
    public class DropEquipment
    {
        public string EquipmentId { get; set; } = string.Empty;
        public int Rate { get; set; }
    }

    /// <summary>
    /// 图鉴掉落配置。
    /// </summary>
    public class DropCollection
    {
        public string SeriesId { get; set; } = string.Empty;
        public int CollectionType { get; set; }  // 0=文字图鉴, 1=图片图鉴
        public int Rate { get; set; }
    }

    /// <summary>
    /// 地图怪物生成规则。
    /// </summary>
    public class MapMonsterSpawnRule
    {
        /// <summary>
        /// 怪物模板 ID。
        /// </summary>
        public string MonsterTemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 加权随机权重，值越大越容易被抽中。
        /// </summary>
        public int Weight { get; set; } = 1;

        /// <summary>
        /// 单场战斗内该怪物最多可生成数量。
        /// </summary>
        public int MaxCount { get; set; } = 1;
    }

    /// <summary>
    /// 地图模板。
    /// </summary>
    public class Map
    {
        /// <summary>
        /// 地图 ID。
        /// </summary>
        public string MapGId { get; set; } = string.Empty;

        /// <summary>
        /// 地图名称。
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 地图等级限制。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 地图描述。
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 下一层地图 ID。
        /// </summary>
        public string? NextMapId { get; set; }

        /// <summary>
        /// 进入下一层时是否继承当前状态。
        /// </summary>
        public bool Carrying { get; set; } = false;

        /// <summary>
        /// 随机生成怪物数量区间。
        /// </summary>
        public (int min, int max) MonsterCount { get; set; }

        /// <summary>
        /// 当前地图的刷怪规则列表。
        /// </summary>
        public List<MapMonsterSpawnRule> SpawnRules { get; set; } = [];
    }
}
