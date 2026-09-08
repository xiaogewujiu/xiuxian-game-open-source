namespace XXX.Infrastructure.SeedData
{
    /// <summary>
    /// 启动阶段种子数据同步的开关配置。
    /// </summary>
    public sealed class SeedStartupOptions
    {
        /// <summary>
        /// 启动时是否同步装备洗练规则（池 + 属性值配置）。
        /// 默认关闭以加快启动速度。
        /// </summary>
        public bool SyncEquipmentRerollRules { get; set; }

        /// <summary>
        /// 启动时是否同步五行关系规则。
        /// 默认关闭以加快启动速度。
        /// </summary>
        public bool SyncElementRelationRules { get; set; }

        /// <summary>
        /// 设为 true 时跳过启动引导检查和全部种子数据初始化，直接启动服务。
        /// 适用于本地开发环境想快速重启的场景。
        /// </summary>
        public bool SkipBootstrapCheck { get; set; }
    }
}
