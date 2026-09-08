using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 玩家属性点分配实体。
    /// 单条记录只表示一种属性当前已投入的整数点数。
    /// </summary>
    [SugarTable("PlayerAttributeAllocations")]
    public class PlayerAttributeAllocationEntity
    {
        /// <summary>
        /// 分配记录主键。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "分配记录ID")]
        public string AllocationId { get; set; } = string.Empty;

        /// <summary>
        /// 玩家ID。
        /// </summary>
        [SugarColumn(ColumnDescription = "玩家ID", Length = 50)]
        public string PlayerId { get; set; } = string.Empty;

        /// <summary>
        /// 属性键，例如 type1 / type3。
        /// </summary>
        [SugarColumn(ColumnDescription = "属性键", Length = 20)]
        public string AttributeKey { get; set; } = string.Empty;

        /// <summary>
        /// 当前已经投入的点数。
        /// </summary>
        [SugarColumn(ColumnDescription = "已投入点数")]
        public int AllocatedPoints { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        [SugarColumn(ColumnDescription = "创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(ColumnDescription = "最后更新时间")]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
