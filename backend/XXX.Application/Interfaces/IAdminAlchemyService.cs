using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台炼丹配置服务接口。
    /// </summary>
    public interface IAdminAlchemyService
    {
        /// <summary>
        /// 获取炼丹配方列表。
        /// </summary>
        Task<List<AdminAlchemyRecipeListItemDto>> GetRecipesAsync(string? keyword = null);

        /// <summary>
        /// 获取炼丹配方详情。
        /// </summary>
        Task<AdminAlchemyRecipeDetailDto?> GetRecipeDetailAsync(string recipeId);

        /// <summary>
        /// 保存炼丹配方。
        /// </summary>
        Task<AdminAlchemyRecipeDetailDto> SaveRecipeAsync(AdminAlchemyRecipeDetailDto request);

        /// <summary>
        /// 删除炼丹配方。
        /// </summary>
        Task<bool> DeleteRecipeAsync(string recipeId);
    }
}
