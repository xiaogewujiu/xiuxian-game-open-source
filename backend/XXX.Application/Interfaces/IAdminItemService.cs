using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台道具模板服务接口。
    /// </summary>
    public interface IAdminItemService
    {
        /// <summary>
        /// 获取道具模板列表。
        /// </summary>
        Task<List<AdminItemListItemDto>> GetListAsync(string? keyword = null, int? type = null);

        /// <summary>
        /// 获取道具模板详情。
        /// </summary>
        Task<AdminItemDetailDto?> GetDetailAsync(string itemId);

        /// <summary>
        /// 保存道具模板。
        /// </summary>
        Task<AdminItemDetailDto> SaveAsync(AdminItemDetailDto request);

        /// <summary>
        /// 删除道具模板。
        /// </summary>
        Task<bool> DeleteAsync(string itemId);
    }
}
