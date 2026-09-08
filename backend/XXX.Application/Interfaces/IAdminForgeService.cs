using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台锻造配方服务接口。
    /// </summary>
    public interface IAdminForgeService
    {
        /// <summary>
        /// 获取锻造配方列表。
        /// </summary>
        Task<List<AdminForgeRecipeListItemDto>> GetListAsync(string? keyword = null);

        /// <summary>
        /// 获取锻造配方详情。
        /// </summary>
        Task<AdminForgeRecipeDetailDto?> GetDetailAsync(string recipeId);

        /// <summary>
        /// 保存锻造配方。
        /// </summary>
        Task<AdminForgeRecipeDetailDto> SaveAsync(AdminForgeRecipeDetailDto request);

        /// <summary>
        /// 删除锻造配方。
        /// </summary>
        Task<bool> DeleteAsync(string recipeId);
    }
}
