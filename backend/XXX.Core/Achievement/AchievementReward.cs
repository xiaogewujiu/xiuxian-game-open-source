using XXX.Entity;
using XXX.Inventory;
using XXX.Player;

namespace XXX.Achievement
{
    /// <summary>
    /// 成就奖励类
    /// 定义完成成就后可获得的奖励
    /// </summary>
    public class AchievementReward
    {
        /// <summary>
        /// 成就称号
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 经验奖励
        /// </summary>
        public long Exp { get; set; } = 0;

        /// <summary>
        /// 金币奖励
        /// </summary>
        public long Gold { get; set; } = 0;

        /// <summary>
        /// 灵石奖励
        /// </summary>
        public long SpiritStone { get; set; } = 0;

        /// <summary>
        /// 荣誉点奖励
        /// </summary>
        public int Honor { get; set; } = 0;

        /// <summary>
        /// 公会贡献奖励
        /// </summary>
        public int GuildContribution { get; set; } = 0;

        /// <summary>
        /// 道具奖励列表
        /// Key: 道具ID, Value: 数量
        /// </summary>
        public Dictionary<string, int> Items { get; set; } = [];

        /// <summary>
        /// 装备奖励列表（装备模板ID列表）
        /// </summary>
        public List<int> EquipmentIds { get; set; } = [];

        /// <summary>
        /// 特殊Buff/特效ID列表
        /// </summary>
        public List<string> Buffs { get; set; } = [];

        /// <summary>
        /// 检查是否有任何奖励
        /// </summary>
        public bool HasReward()
        {
            return !string.IsNullOrEmpty(Title) || Exp > 0 || Gold > 0 || SpiritStone > 0 ||
                   Honor > 0 || GuildContribution > 0 ||
                   (Items != null && Items.Count > 0) ||
                   (EquipmentIds != null && EquipmentIds.Count > 0) ||
                   (Buffs != null && Buffs.Count > 0);
        }

        /// <summary>
        /// 获取奖励摘要文本
        /// </summary>
        /// <returns>奖励摘要</returns>
        public string GetRewardSummary()
        {
            if (!HasReward())
            {
                return "无奖励";
            }

            var parts = new List<string>();

            if (!string.IsNullOrEmpty(Title))
            {
                parts.Add($"称号：{Title}");
            }

            if (Exp > 0)
            {
                parts.Add($"{Exp} 经验");
            }

            if (Gold > 0)
            {
                parts.Add($"{Gold} 金币");
            }

            if (SpiritStone > 0)
            {
                parts.Add($"{SpiritStone} 灵石");
            }

            if (Honor > 0)
            {
                parts.Add($"{Honor} 荣誉点");
            }

            if (GuildContribution > 0)
            {
                parts.Add($"{GuildContribution} 公会贡献");
            }

            if (Items != null && Items.Count > 0)
            {
                foreach (var item in Items)
                {
                    string itemName = GetItemName(item.Key);
                    parts.Add($"{itemName} x{item.Value}");
                }
            }

            if (EquipmentIds != null && EquipmentIds.Count > 0)
            {
                foreach (var equipId in EquipmentIds)
                {
                    string equipName = GetEquipmentName(equipId);
                    parts.Add($"{equipName} x1");
                }
            }

            return string.Join(", ", parts);
        }

        /// <summary>
        /// 获取详细奖励文本（多行显示）
        /// </summary>
        /// <returns>详细奖励文本</returns>
        public string GetDetailedRewardText()
        {
            if (!HasReward())
            {
                return "无奖励";
            }

            var lines = new List<string>();

            if (!string.IsNullOrEmpty(Title))
            {
                lines.Add($"称号：{Title}");
            }

            if (Exp > 0)
            {
                lines.Add($"经验：{Exp}");
            }

            if (Gold > 0)
            {
                lines.Add($"金币：{Gold}");
            }

            if (SpiritStone > 0)
            {
                lines.Add($"灵石：{SpiritStone}");
            }

            if (Honor > 0)
            {
                lines.Add($"荣誉点：{Honor}");
            }

            if (GuildContribution > 0)
            {
                lines.Add($"公会贡献：{GuildContribution}");
            }

            if (Items != null && Items.Count > 0)
            {
                lines.Add("道具：");
                foreach (var item in Items)
                {
                    string itemName = GetItemName(item.Key);
                    lines.Add($"  - {itemName} x{item.Value}");
                }
            }

            if (EquipmentIds != null && EquipmentIds.Count > 0)
            {
                lines.Add("装备：");
                foreach (var equipId in EquipmentIds)
                {
                    string equipName = GetEquipmentName(equipId);
                    lines.Add($"  - {equipName}");
                }
            }

            if (Buffs != null && Buffs.Count > 0)
            {
                lines.Add("特效：");
                foreach (var buff in Buffs)
                {
                    lines.Add($"  - {buff}");
                }
            }

            return string.Join("\n", lines);
        }

        /// <summary>
        /// 发放成就奖励给玩家
        /// </summary>
        /// <param name="player">玩家实体</param>
        /// <param name="inventory">背包管理器</param>
        /// <returns>发放结果</returns>
        public AchievementRewardResult GrantRewards(UserEntity player, InventoryManager inventory)
        {
            var result = new AchievementRewardResult
            {
                Success = true,
                GrantedExp = 0,
                GrantedGold = 0,
                GrantedSpiritStone = 0,
                GrantedItems = [],
                GrantedEquipment = []
            };

            try
            {
                // 发放经验
                if (Exp > 0)
                {
                    var expResult = PlayerManager.AddExp(player, Exp);
                    if (expResult.Success)
                    {
                        result.GrantedExp = Exp;
                        result.Messages.Add($"获得 {Exp} 经验");

                        if (expResult.LeveledUp)
                        {
                            result.Messages.Add($"升级！从 Lv.{expResult.OldLevel} 升到 Lv.{expResult.NewLevel}");
                        }
                    }
                }

                // 发放金币
                if (Gold > 0)
                {
                    if (PlayerManager.AddGold(player, Gold, "成就奖励"))
                    {
                        result.GrantedGold = Gold;
                        result.Messages.Add($"获得 {Gold} 金币");
                    }
                }

                // 发放灵石
                if (SpiritStone > 0)
                {
                    if (PlayerManager.AddSpiritStone(player, SpiritStone, "成就奖励"))
                    {
                        result.GrantedSpiritStone = SpiritStone;
                        result.Messages.Add($"获得 {SpiritStone} 灵石");
                    }
                }

                // 发放荣誉点
                if (Honor > 0)
                {
                    if (PlayerManager.AddHonor(player, Honor))
                    {
                        result.GrantedHonor = Honor;
                        result.Messages.Add($"获得 {Honor} 荣誉点");
                    }
                }

                // 发放公会贡献
                if (GuildContribution > 0)
                {
                    if (PlayerManager.AddGuildContribution(player, GuildContribution))
                    {
                        result.GrantedGuildContribution = GuildContribution;
                        result.Messages.Add($"获得 {GuildContribution} 公会贡献");
                    }
                }

                // 发放道具
                if (Items != null && Items.Count > 0)
                {
                    foreach (var item in Items)
                    {
                        // TODO: 需要InventoryManager.AddItem
                        // if (inventory.AddItem(item.Key, item.Value))
                        // {
                        //     string itemName = GetItemName(item.Key);
                        //     result.GrantedItems.Add(item.Key);
                        //     result.Messages.Add($"获得 {itemName} x{item.Value}");
                        // }
                    }
                }

                // 发放装备
                if (EquipmentIds != null && EquipmentIds.Count > 0)
                {
                    foreach (var equipId in EquipmentIds)
                    {
                        if (GameData.EquipmentTemplates.ContainsKey(equipId))
                        {
                            var template = GameData.EquipmentTemplates[equipId];
                            var equipment = new EquipmentInstance
                            {
                                InstanceId = Guid.NewGuid().ToString(),
                                Template = template,
                                EnhanceLevel = 0,
                                RerolledAttrs = [],
                                RerollCount = 0
                            };

                            // TODO: 需要InventoryManager.AddEquipment
                            // if (inventory.AddEquipment(equipment))
                            // {
                            //     string equipName = GetEquipmentName(equipId);
                            //     result.GrantedEquipment.Add(equipment.InstanceId);
                            //     result.Messages.Add($"获得 {equipName}");
                            // }
                        }
                    }
                }

                // 称号奖励（单独记录）
                if (!string.IsNullOrEmpty(Title))
                {
                    result.GrantedTitle = Title;
                    result.Messages.Add($"获得称号：{Title}");
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Messages.Add($"发放奖励失败：{ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 获取道具名称
        /// </summary>
        /// <param name="itemId">道具ID</param>
        /// <returns>道具名称</returns>
        private string GetItemName(string itemId)
        {
            if (GameData.Items.ContainsKey(itemId))
            {
                return GameData.Items[itemId].Name;
            }
            return $"未知道具({itemId})";
        }

        /// <summary>
        /// 获取装备名称
        /// </summary>
        /// <param name="equipmentId">装备模板ID</param>
        /// <returns>装备名称</returns>
        private string GetEquipmentName(int equipmentId)
        {
            if (GameData.EquipmentTemplates.ContainsKey(equipmentId))
            {
                return GameData.EquipmentTemplates[equipmentId].Name;
            }
            return $"未知装备({equipmentId})";
        }
    }

    /// <summary>
    /// 成就奖励结果类
    /// </summary>
    public class AchievementRewardResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息列表
        /// </summary>
        public List<string> Messages { get; set; } = [];

        /// <summary>
        /// 实际发放的经验
        /// </summary>
        public long GrantedExp { get; set; }

        /// <summary>
        /// 实际发放的金币
        /// </summary>
        public long GrantedGold { get; set; }

        /// <summary>
        /// 实际发放的灵石
        /// </summary>
        public long GrantedSpiritStone { get; set; }

        /// <summary>
        /// 实际发放的荣誉点
        /// </summary>
        public int GrantedHonor { get; set; }

        /// <summary>
        /// 实际发放的公会贡献
        /// </summary>
        public int GrantedGuildContribution { get; set; }

        /// <summary>
        /// 实际发放的称号
        /// </summary>
        public string GrantedTitle { get; set; } = string.Empty;

        /// <summary>
        /// 实际发放的道具ID列表
        /// </summary>
        public List<string> GrantedItems { get; set; } = [];

        /// <summary>
        /// 实际发放的装备实例ID列表
        /// </summary>
        public List<string> GrantedEquipment { get; set; } = [];

        /// <summary>
        /// 获取完整的消息文本
        /// </summary>
        /// <returns>消息文本</returns>
        public string GetFullMessage()
        {
            return string.Join("\n", Messages);
        }
    }
}
