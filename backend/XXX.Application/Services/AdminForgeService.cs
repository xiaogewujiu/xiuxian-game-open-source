#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminForgeService : IAdminForgeService
    {
        private readonly IRepository<ForgeRecipeEntity> _forgeRepository;

        public AdminForgeService(IRepository<ForgeRecipeEntity> forgeRepository)
        {
            _forgeRepository = forgeRepository;
        }

        public async Task<List<AdminForgeRecipeListItemDto>> GetListAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _forgeRepository.Db.Queryable<ForgeRecipeEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(recipe => recipe.RecipeId.Contains(normalizedKeyword) || recipe.Name.Contains(normalizedKeyword));
            }

            var recipes = await query.OrderBy(recipe => recipe.Level).OrderBy(recipe => recipe.RecipeId).ToListAsync();
            return recipes.Select(recipe => new AdminForgeRecipeListItemDto
            {
                RecipeId = recipe.RecipeId,
                Name = recipe.Name,
                TemplateId = recipe.TemplateId,
                Level = recipe.Level,
                IsBuiltIn = recipe.IsBuiltIn,
                BuiltInVersion = recipe.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminForgeRecipeDetailDto?> GetDetailAsync(string recipeId)
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return null;
            }

            var recipe = await _forgeRepository.GetByIdAsync(recipeId.Trim());
            if (recipe == null)
            {
                return null;
            }

            return new AdminForgeRecipeDetailDto
            {
                RecipeId = recipe.RecipeId,
                TemplateId = recipe.TemplateId,
                Name = recipe.Name,
                Description = recipe.Description,
                SlotName = recipe.SlotName,
                Quality = recipe.Quality,
                Level = recipe.Level,
                Icon = recipe.Icon,
                CostGold = recipe.CostGold,
                SuccessRate = recipe.SuccessRate,
                MaterialsJson = recipe.MaterialsJson,
                IsEnabled = recipe.IsEnabled,
                IsBuiltIn = recipe.IsBuiltIn,
                SeedKey = recipe.SeedKey,
                BuiltInVersion = recipe.BuiltInVersion,
                LastUpdateTime = recipe.LastUpdateTime
            };
        }

        public async Task<AdminForgeRecipeDetailDto> SaveAsync(AdminForgeRecipeDetailDto request)
        {
            var recipeId = (request.RecipeId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                throw new InvalidOperationException("锻造配方编号不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.TemplateId))
            {
                throw new InvalidOperationException("锻造配方名称和模板编号不能为空。");
            }

            var existing = await _forgeRepository.GetByIdAsync(recipeId);
            if (existing == null)
            {
                existing = new ForgeRecipeEntity { RecipeId = recipeId };
                await _forgeRepository.AddAsync(ApplyRecipe(existing, request));
                return (await GetDetailAsync(recipeId))!;
            }

            ApplyRecipe(existing, request);
            await _forgeRepository.UpdateAsync(existing);
            return (await GetDetailAsync(recipeId))!;
        }

        public async Task<bool> DeleteAsync(string recipeId)
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return false;
            }

            return await _forgeRepository.DeleteAsync(recipeId.Trim()) > 0;
        }

        private static ForgeRecipeEntity ApplyRecipe(ForgeRecipeEntity entity, AdminForgeRecipeDetailDto request)
        {
            entity.TemplateId = request.TemplateId.Trim();
            entity.Name = request.Name.Trim();
            entity.Description = (request.Description ?? string.Empty).Trim();
            entity.SlotName = (request.SlotName ?? string.Empty).Trim();
            entity.Quality = Math.Max(1, request.Quality);
            entity.Level = Math.Max(1, request.Level);
            entity.Icon = (request.Icon ?? string.Empty).Trim();
            entity.CostGold = Math.Max(0, request.CostGold);
            entity.SuccessRate = Math.Max(0, request.SuccessRate);
            entity.MaterialsJson = string.IsNullOrWhiteSpace(request.MaterialsJson) ? "[]" : request.MaterialsJson.Trim();
            entity.IsEnabled = request.IsEnabled;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }
    }
}
#pragma warning restore CS1591
