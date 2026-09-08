using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台任务配置服务接口。
    /// </summary>
    public interface IAdminQuestService
    {
        /// <summary>
        /// 获取任务配置列表。
        /// </summary>
        Task<List<AdminQuestListItemDto>> GetListAsync(string? keyword = null, int? questType = null, int? resetCycle = null);

        /// <summary>
        /// 获取任务配置详情。
        /// </summary>
        Task<AdminQuestDetailDto?> GetDetailAsync(string questId);

        /// <summary>
        /// 保存任务配置。
        /// </summary>
        Task<AdminQuestDetailDto> SaveAsync(AdminQuestDetailDto request);

        /// <summary>
        /// 删除任务配置。
        /// </summary>
        Task<bool> DeleteAsync(string questId);
    }
}
