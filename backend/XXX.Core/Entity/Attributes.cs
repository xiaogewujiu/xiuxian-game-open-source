namespace XXX.Entity
{
    /// <summary>
    /// 基础属性类
    /// 定义角色的基础属性（血量、蓝量、攻击、防御、速度等）和高级属性（命中、闪避、暴击等）
    /// </summary>
    public class BaseAttributes
    {
        #region 基础属性
        /// <summary>
        /// 最大血量
        /// </summary>
        public int Type1 { get; set; }
        /// <summary>
        /// 最大蓝量
        /// </summary>
        public int Type2 { get; set; }
        /// <summary>
        /// 物理攻击
        /// </summary>
        public int Type3 { get; set; }
        /// <summary>
        /// 法术攻击
        /// </summary>
        public int Type4 { get; set; }
        /// <summary>
        /// 物理防御
        /// </summary>
        public int Type5 { get; set; }
        /// <summary>
        /// 法术防御
        /// </summary>
        public int Type6 { get; set; }
        /// <summary>
        /// 速度
        /// </summary>
        public int Type7 { get; set; }

        #endregion

        #region 高级属性
        /// <summary>
        /// 命中率 0-1
        /// </summary>
        public float Type8 { get; set; }
        /// <summary>
        /// 闪避 0-1
        /// </summary>
        public float Type9 { get; set; }
        /// <summary>
        /// 暴击率 0-1
        /// </summary>
        public float Type10 { get; set; }
        /// <summary>
        /// 暴击伤害倍率 默认1.5
        /// </summary>
        public float Type11 { get; set; } = 1.5f;
        /// <summary>
        /// 连击率 0-1
        /// </summary>
        public float Type12 { get; set; }
        /// <summary>
        /// 反击率 0-1
        /// </summary>
        public float Type13 { get; set; }
        /// <summary>
        /// 破甲率 0-1
        /// </summary>
        public float Type14 { get; set; }
        /// <summary>
        /// 额外伤害 倍率
        /// </summary>
        public float Type15 { get; set; }

        #endregion

        /// <summary>
        /// 灵根属性
        /// </summary>
        public Element Element { get; set; } = Element.None;
    }

    /// <summary>
    /// 基础属性模板类
    /// 定义属性的最小值和最大值范围，用于配置生成
    /// </summary>
    public class BaseAttributesTemplate
    {
        #region 基础属性
        /// <summary>
        /// 最小血量
        /// </summary>
        public int? MinType1 { get; set; }
        /// <summary>
        /// 最大血量
        /// </summary>
        public int? MaxType1 { get; set; }


        /// <summary>
        /// 最小蓝量
        /// </summary>
        public int? MinType2 { get; set; }
        /// <summary>
        /// 最大蓝量
        /// </summary>
        public int? MaxType2 { get; set; }



        /// <summary>
        /// 最小物理攻击
        /// </summary>
        public int? MinType3 { get; set; }
        /// <summary>
        /// 最大物理攻击
        /// </summary>
        public int? MaxType3 { get; set; }

        /// <summary>
        /// 最小法术攻击
        /// </summary>
        public int? MinType4 { get; set; }
        /// <summary>
        /// 最大法术攻击
        /// </summary>
        public int? MaxType4 { get; set; }

        /// <summary>
        /// 最小物理防御
        /// </summary>
        public int? MinType5 { get; set; }
        /// <summary>
        /// 最大物理防御
        /// </summary>
        public int? MaxType5 { get; set; }

        /// <summary>
        /// 最小法术防御
        /// </summary>
        public int? MinType6 { get; set; }
        /// <summary>
        /// 最大法术防御
        /// </summary>
        public int? MaxType6 { get; set; }

        /// <summary>
        /// 最小速度
        /// </summary>
        public int? MinType7 { get; set; }
        /// <summary>
        /// 最大速度
        /// </summary>
        public int? MaxType7 { get; set; }
        #endregion

        #region 高级属性

        /// <summary>
        /// 最小命中率
        /// </summary>
        public int? MinType8 { get; set; }
        /// <summary>
        /// 最大命中率
        /// </summary>
        public int? MaxType8 { get; set; }

        /// <summary>
        /// 最小闪避
        /// </summary>
        public int? MinType9 { get; set; }
        /// <summary>
        /// 最大闪避
        /// </summary>
        public int? MaxType9 { get; set; }

        /// <summary>
        /// 最小暴击率
        /// </summary>
        public int? MinType10 { get; set; }
        /// <summary>
        /// 最大暴击率
        /// </summary>
        public int? MaxType10 { get; set; }

        /// <summary>
        /// 最小暴击伤害倍率
        /// </summary>
        public int? MinType11 { get; set; }
        /// <summary>
        /// 最大暴击伤害倍率
        /// </summary>
        public int? MaxType11 { get; set; }

        /// <summary>
        /// 最小连击率
        /// </summary>
        public int? MinType12 { get; set; }
        /// <summary>
        /// 最大连击率
        /// </summary>
        public int? MaxType12 { get; set; }

        /// <summary>
        /// 最小反击率
        /// </summary>
        public int? MinType13 { get; set; }
        /// <summary>
        /// 最大反击率
        /// </summary>
        public int? MaxType13 { get; set; }

        /// <summary>
        /// 最小破甲率
        /// </summary>
        public int? MinType14 { get; set; }
        /// <summary>
        /// 最大破甲率
        /// </summary>
        public int? MaxType14 { get; set; }

        /// <summary>
        /// 最小额外伤害
        /// </summary>
        public int? MinType15 { get; set; }
        /// <summary>
        /// 最大额外伤害
        /// </summary>
        public int? MaxType15 { get; set; }
        #endregion

        /// <summary>
        /// 灵根类型（可选）
        /// </summary>
        public Element? Element { get; set; }
    }
}
