using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 背包物品实体
    /// </summary>
    [SugarTable("InventoryItems")]
    public class InventoryItemEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_player" })]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_item" })]
        public string ItemId { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Quantity { get; set; } = 1;

        [SugarColumn(IsNullable = false, DefaultValue = "0", ColumnName = "IsBound")]
        public bool IsLocked { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime AcquiredTime { get; set; } = DateTime.Now;

        [SugarColumn(IsNullable = true)]
        public DateTime? ExpireTime { get; set; }
    }

    /// <summary>
    /// 装备实例实体
    /// </summary>
    [SugarTable("EquipmentInstances")]
    public class EquipmentInstanceEntity
    {
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string InstanceId { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false, IndexGroupNameList = new[] { "idx_player" })]
        public string PlayerId { get; set; } = string.Empty;

        [SugarColumn(Length = 50, IsNullable = false)]
        public string TemplateId { get; set; } = string.Empty;

        [SugarColumn(Length = 100, IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        [SugarColumn(IsNullable = false)]
        public EquipmentSlot Slot { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public int Quality { get; set; } = 1;

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int EnhanceLevel { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsEquipped { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0", ColumnName = "IsBound")]
        public bool IsLocked { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int BasePhysicalAttack { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int BaseMagicAttack { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int BasePhysicalDefense { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int BaseMagicDefense { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int BaseHP { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int BaseMP { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? BonusStatsJson { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? RerollStatsJson { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? RerollCandidateJson { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = "nvarchar(max)")]
        public string? GemSlotsJson { get; set; }

        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public int RerollCount { get; set; }

        [SugarColumn(IsNullable = true)]
        public DateTime? RerollCandidateCreatedAt { get; set; }

        [SugarColumn(IsNullable = false)]
        public DateTime AcquiredTime { get; set; } = DateTime.Now;

        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
