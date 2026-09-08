using XXX.Entity;

namespace XXX.Battle
{
    /// <summary>
    /// 回合日志
    /// </summary>
    public class RoundLog
    {
        /// <summary>
        /// 回合数
        /// </summary>
        public int RoundNumber { get; set; }

        /// <summary>
        /// 结构化日志条目列表
        /// </summary>
        public List<BattleLogEntry> Entries { get; set; } = [];

        /// <summary>
        /// 回合开始时各角色状态快照
        /// </summary>
        public List<FighterStateSnapshot> FighterStates { get; set; } = [];

        /// <summary>
        /// 本回合所有行动（兼容旧代码）
        /// </summary>
        public List<string> Actions => ToLegacyStrings();

        /// <summary>
        /// 转换为旧版字符串列表（兼容过渡）
        /// </summary>
        private List<string> ToLegacyStrings()
        {
            var result = new List<string>();
            foreach (var entry in Entries)
            {
                result.Add(entry.Description ?? FormatEntry(entry));
            }
            return result;
        }

        /// <summary>
        /// 格式化条目为可读文本
        /// </summary>
        private string FormatEntry(BattleLogEntry entry)
        {
            return entry.Description ?? $"{entry.CasterName} 对 {entry.TargetName} 触发了 {entry.Type}。";
        }
    }
}
