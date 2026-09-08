using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 数据库实体基类
    /// 所有数据库实体都应继承此类
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否删除（软删除）
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// 玩家关联实体基类
    /// 所有与玩家关联的实体都应继承此类
    /// </summary>
    public abstract class PlayerEntity : BaseEntity
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        [SugarColumn(IsNullable = false, IndexGroupNameList = new[] { "idx_player" })]
        public string PlayerId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 配置数据基类
    /// 所有配置表实体都应继承此类
    /// </summary>
    public abstract class ConfigEntity
    {
        /// <summary>
        /// 配置ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 配置名称
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 配置描述
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string? Description { get; set; }
    }
}
