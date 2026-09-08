using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 元素克制矩阵后台服务。
    /// </summary>
    public interface IAdminElementRelationRuleService
    {
        /// <summary>
        /// 获取全部元素克制矩阵条目。
        /// </summary>
        /// <returns>元素克制矩阵条目集合。</returns>
        Task<List<AdminElementRelationEntryDto>> GetEntriesAsync();

        /// <summary>
        /// 保存整套元素克制矩阵条目。
        /// </summary>
        /// <param name="entries">待保存的矩阵条目集合。</param>
        /// <returns>保存后的矩阵条目集合。</returns>
        Task<List<AdminElementRelationEntryDto>> SaveEntriesAsync(List<AdminElementRelationEntryDto> entries);
    }
}
