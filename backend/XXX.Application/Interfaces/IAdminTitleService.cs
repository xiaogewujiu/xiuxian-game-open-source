using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台称号服务接口。
    /// </summary>
    public interface IAdminTitleService
    {
        /// <summary>
        /// 获取称号模板列表。
        /// </summary>
        Task<List<AdminTitleListItemDto>> GetListAsync(string? keyword = null, int take = 200);

        /// <summary>
        /// 获取称号模板详情。
        /// </summary>
        Task<AdminTitleDetailDto?> GetDetailAsync(long id);

        /// <summary>
        /// 创建称号模板。
        /// </summary>
        Task<AdminTitleDetailDto> CreateAsync(AdminTitleDetailDto dto);

        /// <summary>
        /// 更新称号模板。
        /// </summary>
        Task<AdminTitleDetailDto?> UpdateAsync(long id, AdminTitleDetailDto dto);

        /// <summary>
        /// 删除称号模板。
        /// </summary>
        Task<bool> DeleteAsync(long id);

        /// <summary>
        /// 授予玩家称号。
        /// </summary>
        Task<bool> GrantTitleAsync(AdminGrantTitleDto dto);

        /// <summary>
        /// 回收玩家称号。
        /// </summary>
        Task<bool> RevokeTitleAsync(AdminGrantTitleDto dto);
    }
}
