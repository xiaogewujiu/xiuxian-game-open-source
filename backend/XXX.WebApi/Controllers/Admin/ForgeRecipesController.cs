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
    [Route("api/admin/forge-recipes")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class ForgeRecipesController : ControllerBase
    {
        private readonly IAdminForgeService _adminForgeService;

        public ForgeRecipesController(IAdminForgeService adminForgeService)
        {
            _adminForgeService = adminForgeService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminForgeRecipeListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminForgeRecipeListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var recipes = await _adminForgeService.GetListAsync(keyword);
            return Ok(ApiResponse<List<AdminForgeRecipeListItemDto>>.Ok(recipes));
        }

        [HttpGet("{recipeId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminForgeRecipeDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminForgeRecipeDetailDto>>> GetDetail(string recipeId)
        {
            var recipe = await _adminForgeService.GetDetailAsync(recipeId);
            if (recipe == null)
            {
                return NotFound(ApiResponse<AdminForgeRecipeDetailDto>.Fail("锻造配方不存在。"));
            }

            return Ok(ApiResponse<AdminForgeRecipeDetailDto>.Ok(recipe));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminForgeRecipeDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminForgeRecipeDetailDto>>> Save([FromBody] AdminForgeRecipeDetailDto request)
        {
            try
            {
                var recipe = await _adminForgeService.SaveAsync(request);
                return Ok(ApiResponse<AdminForgeRecipeDetailDto>.Ok(recipe, "锻造配方保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminForgeRecipeDetailDto>.Fail(ex.Message));
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
                var deleted = await _adminForgeService.DeleteAsync(recipeId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("锻造配方不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("锻造配方删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
