using XXX.Entity;

namespace XXX
{
    /// <summary>
    /// 运行时模板缓存容器。
    /// </summary>
    public static class GameData
    {
        /// <summary>
        /// 野怪模板缓存，Key 为怪物模板编号。
        /// </summary>
        public static Dictionary<string, MonsterTemplate> MonsterTemplates = [];

        /// <summary>
        /// 装备模板缓存，Key 为装备模板编号。
        /// </summary>
        public static Dictionary<int, EquipmentTemplate> EquipmentTemplates = [];

        /// <summary>
        /// 地图模板缓存，Key 为地图编号。
        /// </summary>
        public static Dictionary<string, Map> Maps = [];

        /// <summary>
        /// 道具模板缓存，Key 为道具编号。
        /// </summary>
        public static Dictionary<string, ItemTable> Items = [];

        /// <summary>
        /// 秘境实例模板缓存，Key 为秘境模板 ID。
        /// </summary>
        public static Dictionary<string, DungeonInstanceTemplateEntity> DungeonInstanceTemplates = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 秘境事件配置缓存（按事件 ID），Key 为事件 ID。
        /// </summary>
        public static Dictionary<string, DungeonEventConfigEntity> DungeonEventConfigsById = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 秘境事件配置缓存（按秘境 ID + 事件类型分组），Key 为 "DungeonId:EventType"。
        /// </summary>
        public static Dictionary<string, List<DungeonEventConfigEntity>> DungeonEventConfigsByDungeonAndType = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 秘境事件组缓存，Key 为事件组 ID。
        /// </summary>
        public static Dictionary<string, DungeonEventGroupEntity> DungeonEventGroups = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 清空运行时模板缓存。
        /// </summary>
        public static void ClearRuntimeCaches()
        {
            MonsterTemplates = new Dictionary<string, MonsterTemplate>(StringComparer.OrdinalIgnoreCase);
            EquipmentTemplates = [];
            Maps = new Dictionary<string, Map>(StringComparer.OrdinalIgnoreCase);
            Items = new Dictionary<string, ItemTable>(StringComparer.OrdinalIgnoreCase);
            DungeonInstanceTemplates = new Dictionary<string, DungeonInstanceTemplateEntity>(StringComparer.OrdinalIgnoreCase);
            DungeonEventConfigsById = new Dictionary<string, DungeonEventConfigEntity>(StringComparer.OrdinalIgnoreCase);
            DungeonEventConfigsByDungeonAndType = new Dictionary<string, List<DungeonEventConfigEntity>>(StringComparer.OrdinalIgnoreCase);
            DungeonEventGroups = new Dictionary<string, DungeonEventGroupEntity>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
