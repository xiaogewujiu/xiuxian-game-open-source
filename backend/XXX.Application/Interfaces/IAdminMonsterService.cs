using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台怪物模板服务接口。
    /// </summary>
    public interface IAdminMonsterService
    {
        /// <summary>
        /// 获取怪物列表。
        /// </summary>
        Task<List<AdminMonsterListItemDto>> GetListAsync(string? keyword = null);

        /// <summary>
        /// 获取怪物详情。
        /// </summary>
        Task<AdminMonsterDetailDto?> GetDetailAsync(string monsterId);

        /// <summary>
        /// 保存怪物模板。
        /// </summary>
        Task<AdminMonsterDetailDto> SaveAsync(AdminSaveMonsterRequestDto request);

        /// <summary>
        /// 删除怪物模板。
        /// </summary>
        Task<bool> DeleteAsync(string monsterId);
    }
}
