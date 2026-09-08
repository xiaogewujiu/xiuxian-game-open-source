namespace XXX.Entity
{
    /// <summary>
    /// Buff状态快照
    /// </summary>
    public class ActiveBuffSnapshot
    {
        public string BuffId { get; set; } = string.Empty;
        public string BuffName { get; set; } = string.Empty;
        public int RemainingDuration { get; set; }
        public int Stack { get; set; }
    }

    /// <summary>
    /// 角色状态快照 - 包含完整属性信息
    /// </summary>
    public class FighterStateSnapshot
    {
        #region 基础信息
        public string FighterId { get; set; } = string.Empty;
        public string FighterName { get; set; } = string.Empty;
        public bool IsPlayerSide { get; set; }
        #endregion

        #region 血量蓝量（原始数值）
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }
        public int CurrentMp { get; set; }
        public int MaxMp { get; set; }
        #endregion

        #region 基础属性（原始数值）
        public int PhysicalAttack { get; set; }
        public int MagicAttack { get; set; }
        public int PhysicalDefense { get; set; }
        public int MagicDefense { get; set; }
        public int Speed { get; set; }
        public string ElementName { get; set; } = string.Empty;
        #endregion

        #region 高级属性（格式化为百分比字符串，如 "35.0%"）
        public string HitRate { get; set; } = string.Empty;
        public string DodgeRate { get; set; } = string.Empty;
        public string CritRate { get; set; } = string.Empty;
        public string CritDamage { get; set; } = string.Empty;
        public string ComboRate { get; set; } = string.Empty;
        public string CounterRate { get; set; } = string.Empty;
        public string ArmorBreak { get; set; } = string.Empty;
        public string ExtraDamage { get; set; } = string.Empty;
        #endregion

        #region Buff列表
        public List<ActiveBuffSnapshot> Buffs { get; set; } = [];
        #endregion
    }
}
