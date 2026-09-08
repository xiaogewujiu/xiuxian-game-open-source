namespace XXX.Battle
{
    /// <summary>
    /// 战斗结果
    /// </summary>
    public class BattleResult
    {
        /// <summary>
        /// 地图名称
        /// </summary>
        public string MapName { get; set; } = string.Empty;

        /// <summary>
        /// 是否胜利
        /// </summary>
        public bool IsVictory { get; set; }

        /// <summary>
        /// 总回合数
        /// </summary>
        public int TotalRounds { get; set; }

        /// <summary>
        /// 回合日志列表
        /// </summary>
        public List<RoundLog> RoundLogs { get; set; } = [];

        /// <summary>
        /// 获得的经验
        /// </summary>
        public int ExpGained { get; set; }

        /// <summary>
        /// 获得的金币
        /// </summary>
        public int GoldGained { get; set; }

        /// <summary>
        /// 掉落的道具
        /// </summary>
        public List<XXX.Entity.ItemTable> DroppedItems { get; set; } = [];

        /// <summary>
        /// 掉落的装备
        /// </summary>
        public List<XXX.Entity.EquipmentInstance> DroppedEquipments { get; set; } = [];

        /// <summary>
        /// 掉落的图鉴系列（SeriesId, CollectionType）
        /// </summary>
        public List<(string SeriesId, int CollectionType)> DroppedCollections { get; set; } = [];

        /// <summary>
        /// 战斗单位技能统计
        /// </summary>
        public Dictionary<string, FighterSkillStats> FighterSkillStats { get; set; } = [];
    }

    /// <summary>
    /// 战斗单位技能统计
    /// </summary>
    public class FighterSkillStats
    {
        /// <summary>
        /// 战斗单位ID
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 战斗单位名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 技能使用次数统计
        /// </summary>
        public Dictionary<int, int> SkillUsageCount { get; set; } = [];

        // ========== 伤害统计 ==========

        /// <summary>
        /// 总伤害输出
        /// </summary>
        public int TotalDamageDealt { get; set; } = 0;

        /// <summary>
        /// 总受到的伤害
        /// </summary>
        public int TotalDamageTaken { get; set; } = 0;

        /// <summary>
        /// 总治疗量（自己造成的治疗）
        /// </summary>
        public int TotalHealingDone { get; set; } = 0;

        /// <summary>
        /// 总受到的治疗
        /// </summary>
        public int TotalHealingReceived { get; set; } = 0;

        /// <summary>
        /// 总吸血量
        /// </summary>
        public int TotalLifesteal { get; set; } = 0;

        /// <summary>
        /// 护盾吸收的伤害
        /// </summary>
        public int TotalShieldAbsorbed { get; set; } = 0;

        /// <summary>
        /// 反伤造成的伤害
        /// </summary>
        public int TotalReflectDamage { get; set; } = 0;

        // ========== 击杀统计 ==========

        /// <summary>
        /// 击杀数
        /// </summary>
        public int Kills { get; set; } = 0;

        /// <summary>
        /// 本单位击杀的怪物模板ID列表。
        /// </summary>
        public List<string> KilledMonsterTemplateIds { get; set; } = [];

        /// <summary>
        /// 死亡次数
        /// </summary>
        public int Deaths { get; set; } = 0;


        // ========== 战斗行为统计 ==========

        /// <summary>
        /// 普通攻击次数
        /// </summary>
        public int NormalAttackCount { get; set; } = 0;

        /// <summary>
        /// 连击次数
        /// </summary>
        public int ComboCount { get; set; } = 0;

        /// <summary>
        /// 反击次数
        /// </summary>
        public int CounterCount { get; set; } = 0;

        /// <summary>
        /// 闪避次数
        /// </summary>
        public int DodgeCount { get; set; } = 0;

        /// <summary>
        /// 攻击被闪避次数
        /// </summary>
        public int MissedCount { get; set; } = 0;

        /// <summary>
        /// 暴击次数
        /// </summary>
        public int CritCount { get; set; } = 0;

        /// <summary>
        /// 暴击伤害总量
        /// </summary>
        public int TotalCritDamage { get; set; } = 0;

        // ========== 资源消耗统计 ==========

        /// <summary>
        /// 蓝量消耗总和
        /// </summary>
        public int TotalMpConsumed { get; set; } = 0;

        /// <summary>
        /// 生命消耗总和（如血祭技能）
        /// </summary>
        public int TotalHpConsumed { get; set; } = 0;

        // ========== 技能伤害明细（可选） ==========

        /// <summary>
        /// 每个技能造成的伤害量
        /// </summary>
        public Dictionary<int, int> SkillDamageDealt { get; set; } = [];
    }
}
