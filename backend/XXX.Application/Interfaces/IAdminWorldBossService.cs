using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 后台世界 Boss 管理服务。
    /// </summary>
    public interface IAdminWorldBossService
    {
        /// <summary>
        /// 获取后台世界 Boss 模板列表。
        /// </summary>
        /// <returns>模板摘要列表。</returns>
        Task<List<AdminWorldBossTemplateListItemDto>> GetTemplatesAsync();

        /// <summary>
        /// 获取指定世界 Boss 模板详情。
        /// </summary>
        /// <param name="bossId">Boss 模板编号。</param>
        /// <returns>模板详情；不存在时返回空。</returns>
        Task<AdminWorldBossTemplateDetailDto?> GetTemplateAsync(string bossId);

        /// <summary>
        /// 新增或更新世界 Boss 模板。
        /// </summary>
        /// <param name="request">模板保存请求。</param>
        /// <returns>保存后的模板详情。</returns>
        Task<AdminWorldBossTemplateDetailDto> SaveTemplateAsync(AdminWorldBossTemplateDetailDto request);

        /// <summary>
        /// 删除指定世界 Boss 模板。
        /// </summary>
        /// <param name="bossId">Boss 模板编号。</param>
        /// <returns>删除成功返回真。</returns>
        Task<bool> DeleteTemplateAsync(string bossId);

        /// <summary>
        /// 获取当前世界 Boss 每日排期配置。
        /// </summary>
        /// <returns>后台排期配置。</returns>
        Task<AdminWorldBossScheduleDto> GetScheduleAsync();

        /// <summary>
        /// 保存世界 Boss 每日排期配置。
        /// </summary>
        /// <param name="request">排期保存请求。</param>
        /// <returns>保存后的排期配置。</returns>
        Task<AdminWorldBossScheduleDto> SaveScheduleAsync(AdminWorldBossScheduleDto request);

        /// <summary>
        /// 获取后台世界 Boss 运行时总览。
        /// </summary>
        /// <returns>当前实例、排行和最近日志。</returns>
        Task<AdminWorldBossRuntimeDto> GetRuntimeAsync();

        /// <summary>
        /// 立即生成一个世界 Boss 实例。
        /// </summary>
        /// <param name="bossId">指定的 Boss 模板编号；为空时按规则选择。</param>
        /// <returns>生成后的世界 Boss 当前状态。</returns>
        Task<WorldBossCurrentDto> SpawnNowAsync(string? bossId = null);

        /// <summary>
        /// 关闭当前运行中的世界 Boss 实例。
        /// </summary>
        /// <param name="reason">关闭原因。</param>
        /// <returns>存在可关闭实例时返回真。</returns>
        Task<bool> CloseCurrentAsync(string? reason = null);
    }
}
