using XXX.Entity;

namespace XXX.Application.DTOs
{
    /// <summary>
    /// 装备DTO
    /// </summary>
    public class EquipmentDto
    {
        /// <summary>
        /// 装备实例ID
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// 装备模板ID
        /// </summary>
        public string TemplateId { get; set; } = string.Empty;

        /// <summary>
        /// 装备名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 装备描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 装备部位
        /// </summary>
        public EquipmentSlot Slot { get; set; }

        /// <summary>
        /// 装备部位名称
        /// </summary>
        public string SlotName => Slot.ToString();

        /// <summary>
        /// 装备等级（来自模板）。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 品质等级
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 品质名称
        /// </summary>
        public string QualityName => GetQualityName(Quality);

        /// <summary>
        /// 强化等级
        /// </summary>
        public int EnhanceLevel { get; set; }

        /// <summary>
        /// 是否已装备
        /// </summary>
        public bool IsEquipped { get; set; }

        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// 基础物理攻击
        /// </summary>
        public int BasePhysicalAttack { get; set; }

        /// <summary>
        /// 基础法术攻击
        /// </summary>
        public int BaseMagicAttack { get; set; }

        /// <summary>
        /// 基础物理防御
        /// </summary>
        public int BasePhysicalDefense { get; set; }

        /// <summary>
        /// 基础法术防御
        /// </summary>
        public int BaseMagicDefense { get; set; }

        /// <summary>
        /// 基础生命值
        /// </summary>
        public int BaseHP { get; set; }

        /// <summary>
        /// 基础法力值
        /// </summary>
        public int BaseMP { get; set; }

        /// <summary>
        /// 战斗流派
        /// </summary>
        public CombatStyle CombatStyle { get; set; }

        /// <summary>
        /// 战斗流派名称
        /// </summary>
        public string CombatStyleName => GetCombatStyleName(CombatStyle);

        /// <summary>
        /// 武器类别
        /// </summary>
        public WeaponCategory WeaponCategory { get; set; }

        /// <summary>
        /// 武器类别名称
        /// </summary>
        public string WeaponCategoryName => GetWeaponCategoryName(WeaponCategory);

        /// <summary>
        /// 额外属性加成
        /// </summary>
        public List<EquipmentBonusDto> BonusStats { get; set; } = [];

        /// <summary>
        /// 洗练词条（已接受）。
        /// </summary>
        public List<EquipmentBonusDto> RerollStats { get; set; } = [];

        /// <summary>
        /// 候选洗练词条（未接受）。
        /// </summary>
        public List<EquipmentBonusDto>? RerollCandidate { get; set; }

        /// <summary>
        /// 已执行洗练次数。
        /// </summary>
        public int RerollCount { get; set; }

        /// <summary>
        /// 装备图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 宝石孔位列表
        /// </summary>
        public List<EquipmentGemSlotDto> GemSlots { get; set; } = [];

        /// <summary>
        /// 最大宝石孔位数（由品质决定）
        /// </summary>
        public int MaxGemSlots { get; set; }

        /// <summary>
        /// 获得时间
        /// </summary>
        public DateTime AcquiredTime { get; set; }

        /// <summary>
        /// 获取品质名称
        /// </summary>
        private static string GetQualityName(int quality)
        {
            return quality switch
            {
                1 => "普通",
                2 => "优秀",
                3 => "精良",
                4 => "史诗",
                5 => "传说",
                _ => "未知"
            };
        }

        private static string GetCombatStyleName(CombatStyle combatStyle)
        {
            return combatStyle switch
            {
                CombatStyle.Physical => "物理",
                CombatStyle.Magic => "法术",
                _ => "通用"
            };
        }

        private static string GetWeaponCategoryName(WeaponCategory weaponCategory)
        {
            return weaponCategory switch
            {
                WeaponCategory.Sword => "剑",
                WeaponCategory.Blade => "刀",
                WeaponCategory.Axe => "斧",
                WeaponCategory.Spear => "枪",
                WeaponCategory.Qin => "琴",
                WeaponCategory.Chess => "棋",
                WeaponCategory.Book => "书",
                WeaponCategory.Brush => "画",
                _ => "无"
            };
        }
    }

    /// <summary>
    /// 装备属性加成DTO
    /// </summary>
    public class EquipmentBonusDto
    {
        /// <summary>
        /// 属性类型
        /// </summary>
        public string StatType { get; set; } = string.Empty;

        /// <summary>
        /// 中文注释：
        /// 供前端直接展示的值。
        /// 普通整数属性这里就是原值；
        /// 百分比属性这里会提前转换成 5、12 这种“百分数整数”，方便界面直接显示 5%。
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// 中文注释：
        /// 属性真实值。
        /// 例如暴击率 5% 会存成 0.05，物攻 +12 会存成 12。
        /// 后端在重算角色属性时必须使用这个字段，不能用已经格式化过的 Value。
        /// </summary>
        public double RawValue { get; set; }

        /// <summary>
        /// 是否为百分比属性
        /// </summary>
        public bool IsPercentage { get; set; }

        /// <summary>
        /// 属性描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 词条位置索引。
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 品阶数字。
        /// </summary>
        public int Tier { get; set; }

        /// <summary>
        /// 品阶名称。
        /// </summary>
        public string TierName { get; set; } = string.Empty;

        /// <summary>
        /// 品阶颜色。
        /// </summary>
        public string TierColor { get; set; } = "#9ca3af";
    }

    /// <summary>
    /// 穿戴装备请求DTO
    /// </summary>
    public class EquipItemRequestDto
    {
        /// <summary>
        /// 装备实例ID
        /// </summary>
        public string EquipmentId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 卸下装备请求DTO
    /// </summary>
    public class UnequipItemRequestDto
    {
        /// <summary>
        /// 装备部位
        /// </summary>
        public EquipmentSlot Slot { get; set; }
    }

    /// <summary>
    /// 强化装备请求DTO
    /// </summary>
    public class EnhanceEquipmentRequestDto
    {
        /// <summary>
        /// 装备实例ID
        /// </summary>
        public string EquipmentId { get; set; } = string.Empty;

        /// <summary>
        /// 使用的强化材料
        /// </summary>
        public List<EnhanceMaterialDto>? Materials { get; set; }
    }

    /// <summary>
    /// 强化材料DTO
    /// </summary>
    public class EnhanceMaterialDto
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 使用数量
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// 强化结果DTO
    /// </summary>
    public class EnhanceResultDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 新的强化等级
        /// </summary>
        public int NewEnhanceLevel { get; set; }

        /// <summary>
        /// 消耗的货币
        /// </summary>
        public long CostGold { get; set; }

        /// <summary>
        /// 消耗的强化石数量
        /// </summary>
        public int CostEnhanceStones { get; set; }
    }

    /// <summary>
    /// 绑定装备请求 DTO
    /// </summary>
    public class BindEquipmentRequestDto
    {
        /// <summary>
        /// 装备实例ID
        /// </summary>
        public string EquipmentId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 绑定装备结果 DTO
    /// </summary>
    public class BindEquipmentResultDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// 洗练结果 DTO
    /// </summary>
    public class RerollEquipmentResultDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 消耗金币
        /// </summary>
        public long CostGold { get; set; }

        /// <summary>
        /// 消耗材料数量
        /// </summary>
        public int CostMaterialCount { get; set; }

        /// <summary>
        /// 洗练后的装备数据
        /// </summary>
        public EquipmentDto? Equipment { get; set; }
    }

    /// <summary>
    /// 捐献结果 DTO
    /// </summary>
    public class DonateEquipmentResultDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获得的贡献值
        /// </summary>
        public int ContributionGained { get; set; }
    }

    /// <summary>
    /// 装备对比DTO
    /// </summary>
    public class EquipmentCompareDto
    {
        /// <summary>
        /// 当前装备
        /// </summary>
        public EquipmentDto? CurrentEquipment { get; set; }

        /// <summary>
        /// 新装备
        /// </summary>
        public EquipmentDto NewEquipment { get; set; } = new();

        /// <summary>
        /// 属性差异
        /// </summary>
        public List<StatDifferenceDto> Differences { get; set; } = [];
    }

    /// <summary>
    /// 属性差异DTO
    /// </summary>
    public class StatDifferenceDto
    {
        /// <summary>
        /// 属性名称
        /// </summary>
        public string StatName { get; set; } = string.Empty;

        /// <summary>
        /// 当前值
        /// </summary>
        public int CurrentValue { get; set; }

        /// <summary>
        /// 新值
        /// </summary>
        public int NewValue { get; set; }

        /// <summary>
        /// 差异值
        /// </summary>
        public int Difference => NewValue - CurrentValue;

        /// <summary>
        /// 是否提升
        /// </summary>
        public bool IsImprovement => Difference > 0;
    }

    /// <summary>
    /// 装备洗练预览 DTO。
    /// </summary>
    public class EquipmentRerollPreviewDto
    {
        /// <summary>
        /// 装备实例 ID。
        /// </summary>
        public string EquipmentId { get; set; } = string.Empty;

        /// <summary>
        /// 当前生效词条。
        /// </summary>
        public List<EquipmentBonusDto> CurrentStats { get; set; } = [];

        /// <summary>
        /// 候选词条。
        /// </summary>
        public List<EquipmentBonusDto>? CandidateStats { get; set; }

        /// <summary>
        /// 当前默认消耗。
        /// </summary>
        public EquipmentRerollCostDto Cost { get; set; } = new();

        /// <summary>
        /// 最大锁定条数。
        /// </summary>
        public int MaxLockedLineCount { get; set; }

        /// <summary>
        /// 洗练石道具 ID。
        /// </summary>
        public string RerollStoneItemId { get; set; } = string.Empty;

        /// <summary>
        /// 已执行洗练次数。
        /// </summary>
        public int RerollCount { get; set; }

        /// <summary>
        /// 系统是否开启。
        /// </summary>
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// 装备洗练消耗 DTO。
    /// </summary>
    public class EquipmentRerollCostDto
    {
        /// <summary>
        /// 金币消耗。
        /// </summary>
        public long Gold { get; set; }

        /// <summary>
        /// 洗练石消耗。
        /// </summary>
        public int StoneCount { get; set; }

        /// <summary>
        /// 当前规则使用的材料道具 ID。
        /// </summary>
        public string MaterialItemId { get; set; } = string.Empty;

        /// <summary>
        /// 当前规则使用的材料名称。
        /// </summary>
        public string MaterialName { get; set; } = string.Empty;

        /// <summary>
        /// 锁定词条数。
        /// </summary>
        public int LockedCount { get; set; }
    }

    /// <summary>
    /// 执行洗练请求 DTO。
    /// </summary>
    public class RerollEquipmentRollRequestDto
    {
        /// <summary>
        /// 装备实例 ID。
        /// </summary>
        public string EquipmentId { get; set; } = string.Empty;

        /// <summary>
        /// 锁定的词条索引。
        /// </summary>
        public List<int> LockedIndices { get; set; } = [];
    }

    /// <summary>
    /// 接受候选结果请求 DTO。
    /// </summary>
    public class RerollEquipmentAcceptRequestDto
    {
        /// <summary>
        /// 装备实例 ID。
        /// </summary>
        public string EquipmentId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 丢弃候选结果请求 DTO。
    /// </summary>
    public class RerollEquipmentDiscardRequestDto
    {
        /// <summary>
        /// 装备实例 ID。
        /// </summary>
        public string EquipmentId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 批量出售装备请求。
    /// </summary>
    public class BatchSellEquipmentRequestDto
    {
        public List<string> EquipmentIds { get; set; } = [];
    }

    public class BatchSellEquipmentResultDto
    {
        public int SoldCount { get; set; }
        public int FailedCount { get; set; }
        public long GoldEarned { get; set; }
    }

    /// <summary>
    /// 装备分解请求。
    /// </summary>
    public class DecomposeEquipmentRequestDto
    {
        public List<string> EquipmentIds { get; set; } = [];
    }

    public class EquipmentDecomposeRewardDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class EquipmentDecomposeResultDto
    {
        public int DecomposedCount { get; set; }
        public int FailedCount { get; set; }
        public List<EquipmentDecomposeRewardDto> Rewards { get; set; } = [];
    }
}
