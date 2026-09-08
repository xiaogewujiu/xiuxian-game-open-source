namespace XXX.Entity
{
    /// <summary>
    /// 灵根类型
    /// </summary>
    public enum Element
    {
        /// <summary>
        /// 无灵根
        /// </summary>
        None,
        /// <summary>
        /// 金
        /// </summary>
        Metal,
        /// <summary>
        /// 木
        /// </summary>
        Wood,
        /// <summary>
        /// 水
        /// </summary>
        Water,
        /// <summary>
        /// 火
        /// </summary>
        Fire,
        /// <summary>
        /// 土
        /// </summary>
        Earth,
        /// <summary>
        /// 风
        /// </summary>
        Wind,
        /// <summary>
        /// 冰
        /// </summary>
        Ice,
        /// <summary>
        /// 雷
        /// </summary>
        Thunder
    }

    /// <summary>
    /// 功能词条
    /// </summary>
    public class AttributeProperty
    {
        /// <summary>
        /// 加成的属性
        /// </summary>
        public AttributeType type { get; set; }
        /// <summary>
        /// 加成的值
        /// </summary>
        public float value { get; set; }
    }

    /// <summary>
    /// 词条类型
    /// </summary>
    public enum AttributeType
    {
        #region 基础属性
        /// <summary>
        /// 最大血量
        /// </summary>
        Type1,
        /// <summary>
        /// 最大蓝量
        /// </summary>
        Type2,
        /// <summary>
        /// 物理攻击
        /// </summary>
        Type3,
        /// <summary>
        /// 法术攻击
        /// </summary>
        Type4,
        /// <summary>
        /// 物理防御
        /// </summary>
        Type5,
        /// <summary>
        /// 法术防御
        /// </summary>
        Type6,
        /// <summary>
        /// 速度
        /// </summary>
        Type7,

        #endregion

        #region 高级属性
        /// <summary>
        /// 命中率 0-1
        /// </summary>
        Type8,
        /// <summary>
        /// 闪避 0-1
        /// </summary>
        Type9,
        /// <summary>
        /// 暴击率 0-1
        /// </summary>
        Type10,
        /// <summary>
        /// 暴击伤害倍率 默认1.5
        /// </summary>
        Type11,
        /// 连击率 0-1
        /// </summary>
        Type12,
        /// <summary>
        /// 反击率 0-1
        /// </summary>
        Type13,
        /// <summary>
        /// 破甲率 0-1
        /// </summary>
        Type14,
        /// <summary>
        /// 额外伤害 倍率
        /// </summary>
        Type15

        #endregion
    }
}
