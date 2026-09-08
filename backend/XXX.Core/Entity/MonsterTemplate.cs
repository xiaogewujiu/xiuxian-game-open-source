namespace XXX.Entity
{
    /// <summary>
    /// 野怪的模版
    /// </summary>
    public class MonsterTemplate : BaseAttributesTemplate
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string GID { get; set; } = string.Empty;
        /// <summary>
        /// 道号
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 等级
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// 携带的技能ID
        /// </summary>
        public List<string> SkillIds { get; set; } = [];

        /// <summary>
        /// 被动buff
        /// </summary>

        public List<string> PassiveIds { get; set; } = [];

        /// <summary>
        /// 五行属性随机范围。
        /// 如果配置了多个元素，生成怪物实例时会从这里随机选一个；
        /// 如果为空，则回退到 BaseAttributesTemplate.Element。
        /// </summary>
        public List<Element> ElementPool { get; set; } = [];

        /// <summary>
        /// 掉落的道具
        /// </summary>
        public List<DropItem> ItemDrops { get; set; } = [];

        /// <summary>
        /// 掉落的装备
        /// </summary>
        public List<DropEquipment> EquipmentDrops { get; set; } = [];

        /// <summary>
        /// 掉落的图鉴系列
        /// </summary>
        public List<DropCollection> CollectionDrops { get; set; } = [];

        /// <summary>
        /// 胜利奖励经验区间
        /// </summary>
        public (int min, int max) ExpReward;

        /// <summary>
        /// 胜利奖励金币区间
        /// </summary>
        public (int min, int max) GoldReward;
    }
}
