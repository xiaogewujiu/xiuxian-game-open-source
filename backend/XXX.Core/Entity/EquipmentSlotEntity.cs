using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 装备槽位实体
    /// 记录玩家各装备槽位的装备情况
    /// </summary>
    [SugarTable("equipment_slot")]
    public class EquipmentSlotEntity
    {
        /// <summary>
        /// 记录唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "记录ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(ColumnDescription = "玩家ID")]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 槽位类型：0-武器，1-头盔，2-衣服，3-鞋子，4-饰品等
        /// </summary>
        [SugarColumn(ColumnDescription = "槽位类型")]
        public int SlotType { get; set; } = 0;

        /// <summary>
        /// 已装备的装备实例ID
        /// </summary>
        [SugarColumn(ColumnDescription = "装备实例ID", IsNullable = true)]
        public string? EquipmentInstanceId { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 货币变动记录实体
    /// 记录玩家货币的变动历史，用于审计和统计
    /// </summary>
    [SugarTable("currency_record")]
    public class CurrencyRecordEntity
    {
        /// <summary>
        /// 记录唯一标识符
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "记录ID")]
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(ColumnDescription = "玩家ID")]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 货币类型：0-金币，5-灵石，3-荣誉，4-公会贡献
        /// </summary>
        [SugarColumn(ColumnDescription = "货币类型")]
        public int CurrencyType { get; set; } = 0;

        /// <summary>
        /// 变动金额（正数为获得，负数为消耗）
        /// </summary>
        [SugarColumn(ColumnDescription = "变动金额")]
        public long Amount { get; set; } = 0;

        /// <summary>
        /// 变动前的金额
        /// </summary>
        [SugarColumn(ColumnDescription = "变动前金额")]
        public long BeforeAmount { get; set; } = 0;

        /// <summary>
        /// 变动后的金额
        /// </summary>
        [SugarColumn(ColumnDescription = "变动后金额")]
        public long AfterAmount { get; set; } = 0;

        /// <summary>
        /// 操作类型：0-系统赠送，1-战斗获得，2-任务奖励，3-商店购买，4-出售物品，5-其他
        /// </summary>
        [SugarColumn(ColumnDescription = "操作类型")]
        public int OperationType { get; set; } = 0;

        /// <summary>
        /// 变动原因描述
        /// </summary>
        [SugarColumn(ColumnDescription = "原因", Length = 100)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 记录时间
        /// </summary>
        [SugarColumn(ColumnDescription = "记录时间")]
        public DateTime RecordTime { get; set; } = DateTime.Now;
    }
}
