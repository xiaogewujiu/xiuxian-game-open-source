using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 后台秘境实例管理服务接口。
    /// </summary>
    public interface IAdminDungeonInstanceService
    {
        /// <summary>
        /// 获取秘境模板列表。
        /// </summary>
        Task<List<AdminDungeonInstanceTemplateListItemDto>> GetTemplateListAsync(string? keyword = null);

        /// <summary>
        /// 获取秘境模板详情。
        /// </summary>
        Task<AdminDungeonInstanceTemplateDetailDto?> GetTemplateDetailAsync(string dungeonId);

        /// <summary>
        /// 保存秘境模板（新增或更新）。
        /// </summary>
        Task<AdminDungeonInstanceTemplateDetailDto> SaveTemplateAsync(AdminDungeonInstanceTemplateDetailDto request);

        /// <summary>
        /// 删除秘境模板。
        /// </summary>
        Task<bool> DeleteTemplateAsync(string dungeonId);

        /// <summary>
        /// 获取事件配置列表。
        /// </summary>
        Task<List<AdminDungeonEventConfigListItemDto>> GetEventListAsync(string? dungeonId = null, int? eventType = null, string? keyword = null);

        /// <summary>
        /// 获取事件配置详情。
        /// </summary>
        Task<AdminDungeonEventConfigDetailDto?> GetEventDetailAsync(string eventId);

        /// <summary>
        /// 保存事件配置（新增或更新）。
        /// </summary>
        Task<AdminDungeonEventConfigDetailDto> SaveEventAsync(AdminDungeonEventConfigDetailDto request);

        /// <summary>
        /// 删除事件配置。
        /// </summary>
        Task<bool> DeleteEventAsync(string eventId);

        /// <summary>
        /// 切换事件启用/禁用状态。
        /// </summary>
        Task<bool> ToggleEventAsync(string eventId, bool enabled);

        /// <summary>
        /// 获取事件组列表。
        /// </summary>
        Task<List<AdminDungeonEventGroupListItemDto>> GetGroupListAsync(string? keyword = null);

        /// <summary>
        /// 获取事件组详情。
        /// </summary>
        Task<AdminDungeonEventGroupDetailDto?> GetGroupDetailAsync(string groupId);

        /// <summary>
        /// 保存事件组（新增或更新）。
        /// </summary>
        Task<AdminDungeonEventGroupDetailDto> SaveGroupAsync(AdminDungeonEventGroupDetailDto request);

        /// <summary>
        /// 删除事件组。
        /// </summary>
        Task<bool> DeleteGroupAsync(string groupId);

        /// <summary>
        /// 重新加载运行时缓存。
        /// </summary>
        Task ReloadRuntimeAsync();
    }
}
