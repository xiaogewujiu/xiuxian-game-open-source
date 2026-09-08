namespace XXX.Application.DTOs
{
    /// <summary>
    /// 背包物品DTO
    /// </summary>
    public class InventoryItemDto
    {
        /// <summary>
        /// 物品实例ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 物品名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 物品描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 物品类型
        /// </summary>
        public string ItemType { get; set; } = string.Empty;

        /// <summary>
        /// 物品品质
        /// </summary>
        public int Quality { get; set; }

        /// <summary>
        /// 物品图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 是否可主动使用。
        /// </summary>
        public bool CanUse { get; set; }

        /// <summary>
        /// 主动使用按钮文案。
        /// </summary>
        public string UseActionText { get; set; } = string.Empty;

        /// <summary>
        /// 物品数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// 获得时间
        /// </summary>
        public DateTime AcquiredTime { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpireTime { get; set; }
    }

    /// <summary>
    /// 添加物品请求DTO
    /// </summary>
    public class AddItemRequestDto
    {
        /// <summary>
        /// 物品ID
        /// </summary>
        public string ItemId { get; set; } = string.Empty;

        /// <summary>
        /// 物品数量
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// 来源
        /// </summary>
        public string Source { get; set; } = string.Empty;
    }

    /// <summary>
    /// 使用物品请求DTO
    /// </summary>
    public class UseItemRequestDto
    {
        /// <summary>
        /// 物品实例ID
        /// </summary>
        public long ItemId { get; set; }

        /// <summary>
        /// 使用数量
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// 使用目标（如适用）
        /// </summary>
        public string? TargetId { get; set; }
    }

    /// <summary>
    /// 技能书分解请求。
    /// </summary>
    public class SkillBookDecomposeRequestDto
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// 技能书分解结果。
    /// </summary>
    public class SkillBookDecomposeResultDto
    {
        public int DecomposedQuantity { get; set; }
        public int FragmentItemQuantity { get; set; }
        public int RemainingBookQuantity { get; set; }
    }

    /// <summary>
    /// 丢弃物品请求DTO
    /// </summary>
    public class DiscardItemRequestDto
    {
        /// <summary>
        /// 物品实例ID
        /// </summary>
        public long ItemId { get; set; }

        /// <summary>
        /// 丢弃数量
        /// </summary>
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// 物品使用结果DTO
    /// </summary>
    public class ItemUseResultDto
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
        /// 获得的效果
        /// </summary>
        public List<ItemEffectDto> Effects { get; set; } = [];

        /// <summary>
        /// 剩余数量
        /// </summary>
        public int RemainingQuantity { get; set; }

        /// <summary>
        /// 当前已使用次数（丹药专用）。
        /// </summary>
        public int UsageCount { get; set; }

        /// <summary>
        /// 最大使用次数，0 表示不限制（丹药专用）。
        /// </summary>
        public int MaxUsageCount { get; set; }
    }

    /// <summary>
    /// 物品效果DTO
    /// </summary>
    public class ItemEffectDto
    {
        /// <summary>
        /// 效果类型
        /// </summary>
        public string EffectType { get; set; } = string.Empty;

        /// <summary>
        /// 效果值
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 效果描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 锁定/解锁道具请求DTO
    /// </summary>
    public class LockItemRequestDto
    {
        /// <summary>
        /// 物品实例ID
        /// </summary>
        public long ItemId { get; set; }
    }

    /// <summary>
    /// 锁定/解锁道具结果DTO
    /// </summary>
    public class LockItemResultDto
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
    /// 背包整理结果DTO
    /// </summary>
    public class InventoryOrganizeResultDto
    {
        /// <summary>
        /// 整理前物品数量
        /// </summary>
        public int BeforeCount { get; set; }

        /// <summary>
        /// 整理后物品数量
        /// </summary>
        public int AfterCount { get; set; }

        /// <summary>
        /// 合并的物品组数
        /// </summary>
        public int MergedGroups { get; set; }
    }

    /// <summary>
    /// 批量丢弃背包道具请求。
    /// </summary>
    public class BatchDiscardItemRequestDto
    {
        public List<BatchDiscardItemEntryDto> Items { get; set; } = [];
    }

    public class BatchDiscardItemEntryDto
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class BatchDiscardItemResultDto
    {
        public int DiscardedItemCount { get; set; }
        public int DiscardedQuantity { get; set; }
        public int FailedItemCount { get; set; }
    }
}
