namespace XXX.Entity
{
    /// <summary>
    /// 怪物实体
    /// </summary>
    public class MonsterEntity : BaseAttributes
    {
        /// <summary>
        /// 使用的模板ID
        /// </summary>
        public string MonsterTempID { get; set; } = string.Empty;

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
        /// 掉落的道具
        /// </summary>
        public List<DropItem>? ItemDrops { get; set; }

        /// <summary>
        /// 掉落的装备
        /// </summary>
        public List<DropEquipment>? EquipmentDrops { get; set; }

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
