using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台副本模板服务接口。
    /// </summary>
    public interface IAdminDungeonService
    {
        /// <summary>
        /// 获取副本模板列表。
        /// </summary>
        Task<List<AdminDungeonListItemDto>> GetListAsync(string? keyword = null);

        /// <summary>
        /// 获取副本模板详情。
        /// </summary>
        Task<AdminDungeonDetailDto?> GetDetailAsync(string dungeonId);

        /// <summary>
        /// 保存副本模板。
        /// </summary>
        Task<AdminDungeonDetailDto> SaveAsync(AdminDungeonDetailDto request);

        /// <summary>
        /// 删除副本模板。
        /// </summary>
        Task<bool> DeleteAsync(string dungeonId);
    }
}
