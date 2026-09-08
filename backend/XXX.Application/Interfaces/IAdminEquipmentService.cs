using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台装备模板服务接口。
    /// </summary>
    public interface IAdminEquipmentService
    {
        /// <summary>
        /// 获取装备模板列表。
        /// </summary>
        Task<List<AdminEquipmentListItemDto>> GetListAsync(string? keyword = null, int? slot = null);

        /// <summary>
        /// 获取装备模板详情。
        /// </summary>
        Task<AdminEquipmentDetailDto?> GetDetailAsync(int equipmentId);

        /// <summary>
        /// 保存装备模板。
        /// </summary>
        Task<AdminEquipmentDetailDto> SaveAsync(AdminEquipmentDetailDto request);

        /// <summary>
        /// 删除装备模板。
        /// </summary>
        Task<bool> DeleteAsync(int equipmentId);
    }
}
