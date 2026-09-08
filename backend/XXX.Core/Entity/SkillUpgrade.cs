using System.Text.Json;

namespace XXX.Entity
{
    /// <summary>
    /// 技能升级条件类型。
    /// </summary>
    public enum SkillUpgradeConditionType
    {
        /// <summary>
        /// 金币条件。
        /// </summary>
        Gold = 1,

        /// <summary>
        /// 灵石条件。
        /// </summary>
        SpiritStone = 2,

        /// <summary>
        /// 普通道具条件。
        /// </summary>
        Item = 3,

        /// <summary>
        /// 玩家等级条件。
        /// </summary>
        PlayerLevel = 4
    }

    /// <summary>
    /// 单条技能升级条件。
    /// </summary>
    public class SkillUpgradeCondition
    {
        /// <summary>
        /// 条件类型。
        /// </summary>
        public SkillUpgradeConditionType Type { get; set; }

        /// <summary>
        /// 条件数量或最低等级。
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 道具条件对应的物品模板编号。
        /// </summary>
        public string? ItemId { get; set; }
    }

    /// <summary>
    /// 技能升级条件 JSON 序列化辅助方法。
    /// </summary>
    public static class SkillUpgradeConditionSerializer
    {
        /// <summary>
        /// 从 JSON 读取升级条件列表。
        /// </summary>
        public static List<SkillUpgradeCondition> Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<SkillUpgradeCondition>>(json) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        /// <summary>
        /// 将升级条件列表写入 JSON。
        /// </summary>
        public static string Serialize(IEnumerable<SkillUpgradeCondition>? conditions)
        {
            return JsonSerializer.Serialize(conditions ?? []);
        }
    }
}
