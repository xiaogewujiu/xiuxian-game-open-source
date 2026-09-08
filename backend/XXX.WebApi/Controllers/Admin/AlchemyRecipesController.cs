#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.Application.Security;

namespace XXX.WebApi.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/alchemy-recipes")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class AlchemyRecipesController : ControllerBase
    {
        private readonly IAdminAlchemyService _adminAlchemyService;

        public AlchemyRecipesController(IAdminAlchemyService adminAlchemyService)
        {
            _adminAlchemyService = adminAlchemyService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminAlchemyRecipeListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminAlchemyRecipeListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var recipes = await _adminAlchemyService.GetRecipesAsync(keyword);
            return Ok(ApiResponse<List<AdminAlchemyRecipeListItemDto>>.Ok(recipes));
        }

        [HttpGet("{recipeId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminAlchemyRecipeDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminAlchemyRecipeDetailDto>>> GetDetail(string recipeId)
        {
            var recipe = await _adminAlchemyService.GetRecipeDetailAsync(recipeId);
            if (recipe == null)
            {
                return NotFound(ApiResponse<AdminAlchemyRecipeDetailDto>.Fail("炼丹配方不存在。"));
            }

            return Ok(ApiResponse<AdminAlchemyRecipeDetailDto>.Ok(recipe));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminAlchemyRecipeDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminAlchemyRecipeDetailDto>>> Save([FromBody] AdminAlchemyRecipeDetailDto request)
        {
            try
            {
                var recipe = await _adminAlchemyService.SaveRecipeAsync(request);
                return Ok(ApiResponse<AdminAlchemyRecipeDetailDto>.Ok(recipe, "炼丹配方保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminAlchemyRecipeDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{recipeId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string recipeId)
        {
            try
            {
                var deleted = await _adminAlchemyService.DeleteRecipeAsync(recipeId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("炼丹配方不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("炼丹配方删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

