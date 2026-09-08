#pragma warning disable CS1591
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    public class AdminAlchemyService : IAdminAlchemyService
    {
        private readonly IRepository<AlchemyRecipeEntity> _recipeRepository;
        private readonly IRepository<AlchemySystemEntity> _alchemySystemRepository;

        public AdminAlchemyService(
            IRepository<AlchemyRecipeEntity> recipeRepository,
            IRepository<AlchemySystemEntity> alchemySystemRepository)
        {
            _recipeRepository = recipeRepository;
            _alchemySystemRepository = alchemySystemRepository;
        }

        public async Task<List<AdminAlchemyRecipeListItemDto>> GetRecipesAsync(string? keyword = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _recipeRepository.Db.Queryable<AlchemyRecipeEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(recipe => recipe.RecipeId.Contains(normalizedKeyword) || recipe.Name.Contains(normalizedKeyword));
            }

            var recipes = await query.OrderBy(recipe => recipe.RecipeId).ToListAsync();
            return recipes.Select(recipe => new AdminAlchemyRecipeListItemDto
            {
                RecipeId = recipe.RecipeId,
                Name = recipe.Name,
                PillTemplateId = recipe.PillTemplateId,
                RequiredFurnaceLevel = recipe.RequiredFurnaceLevel,
                IsBuiltIn = recipe.IsBuiltIn,
                BuiltInVersion = recipe.BuiltInVersion
            }).ToList();
        }

        public async Task<AdminAlchemyRecipeDetailDto?> GetRecipeDetailAsync(string recipeId)
        {
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                return null;
            }

            var recipe = await _recipeRepository.GetByIdAsync(recipeId.Trim());
            if (recipe == null)
            {
                return null;
            }

            return new AdminAlchemyRecipeDetailDto
            {
                RecipeId = recipe.RecipeId,
                PillTemplateId = recipe.PillTemplateId,
                Name = recipe.Name,
                Description = recipe.Description,
                RequiredFurnaceLevel = recipe.RequiredFurnaceLevel,
                BaseSuccessRate = recipe.BaseSuccessRate,
                BaseCraftTime = recipe.BaseCraftTime,
                MaterialsJson = recipe.MaterialsJson ?? "[]",
                UnlockCondition = recipe.UnlockCondition,
                IsDefaultLearned = recipe.IsDefaultLearned,
                IsBuiltIn = recipe.IsBuiltIn,
                SeedKey = recipe.SeedKey,
                BuiltInVersion = recipe.BuiltInVersion,
                LastUpdateTime = recipe.LastUpdateTime
            };
        }

        public async Task<AdminAlchemyRecipeDetailDto> SaveRecipeAsync(AdminAlchemyRecipeDetailDto request)
        {
            var recipeId = (request.RecipeId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(recipeId))
            {
                throw new InvalidOperationException("炼丹配方编号不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.PillTemplateId))
            {
                throw new InvalidOperationException("炼丹配方名称和丹药道具编号不能为空。");
            }

            if (!GameData.Items.TryGetValue(request.PillTemplateId.Trim(), out var itemTemplate) || itemTemplate.Type != ItemType.Pill)
            {
                throw new InvalidOperationException("丹药道具编号对应的道具不存在或不是丹药类型。");
            }

            var existing = await _recipeRepository.GetByIdAsync(recipeId);
            if (existing == null)
            {
                existing = new AlchemyRecipeEntity { RecipeId = recipeId };
                await _recipeRepository.AddAsync(ApplyRecipe(existing, request));
                return (await GetRecipeDetailAsync(recipeId))!;
            }

            ApplyRecipe(existing, request);
            await _recipeRepository.UpdateAsync(existing);
            return (await GetRecipeDetailAsync(recipeId))!;
        }

        public async Task<bool> DeleteRecipeAsync(string recipeId)
        {
            var normalizedRecipeId = (recipeId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedRecipeId))
            {
                return false;
            }

            var systems = await _alchemySystemRepository.Db.Queryable<AlchemySystemEntity>()
                .Where(system => system.LearnedRecipesJson != null && system.LearnedRecipesJson.Contains(normalizedRecipeId))
                .ToListAsync();
            if (systems.Any(system => system.LearnedRecipes.Any(recipe => string.Equals(recipe, normalizedRecipeId, StringComparison.OrdinalIgnoreCase))))
            {
                throw new InvalidOperationException("当前炼丹配方仍被玩家炼丹系统引用，不能直接删除。");
            }

            return await _recipeRepository.DeleteAsync(normalizedRecipeId) > 0;
        }

        private static AlchemyRecipeEntity ApplyRecipe(AlchemyRecipeEntity entity, AdminAlchemyRecipeDetailDto request)
        {
            entity.PillTemplateId = request.PillTemplateId.Trim();
            entity.Name = request.Name.Trim();
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            entity.RequiredFurnaceLevel = Math.Max(1, request.RequiredFurnaceLevel);
            entity.BaseSuccessRate = Math.Max(0, request.BaseSuccessRate);
            entity.BaseCraftTime = Math.Max(0, request.BaseCraftTime);
            entity.MaterialsJson = string.IsNullOrWhiteSpace(request.MaterialsJson) ? "[]" : request.MaterialsJson.Trim();
            entity.UnlockCondition = string.IsNullOrWhiteSpace(request.UnlockCondition) ? null : request.UnlockCondition.Trim();
            entity.IsDefaultLearned = request.IsDefaultLearned;
            entity.SeedKey = null;
            entity.IsBuiltIn = false;
            entity.BuiltInVersion = null;
            entity.LastUpdateTime = DateTime.Now;
            return entity;
        }
    }
}
#pragma warning restore CS1591
