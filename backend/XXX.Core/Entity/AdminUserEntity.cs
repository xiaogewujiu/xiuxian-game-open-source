using SqlSugar;

namespace XXX.Entity
{
    /// <summary>
    /// 管理员账号实体。
    /// 与玩家账号分离，专门用于管理后台登录与权限控制。
    /// </summary>
    [SugarTable("AdminUsers")]
    public class AdminUserEntity
    {
        /// <summary>
        /// 管理员唯一编号。
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, Length = 50, IsNullable = false)]
        public string AdminId { get; set; } = string.Empty;

        /// <summary>
        /// 登录账号。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, IndexGroupNameList = new[] { "idx_admin_account" })]
        public string Account { get; set; } = string.Empty;

        /// <summary>
        /// 后台显示名。
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 密码哈希。
        /// </summary>
        [SugarColumn(Length = 256, IsNullable = false)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// 管理员角色。
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false)]
        public string Role { get; set; } = "super_admin";

        /// <summary>
        /// 是否启用。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "1")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 是否软删除。
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 创建时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后登录时间。
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 最后更新时间。
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public DateTime LastUpdateTime { get; set; } = DateTime.Now;
    }
}
