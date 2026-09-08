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
    [Route("api/admin/pets")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class PetsController : ControllerBase
    {
        private readonly IAdminPetService _adminPetService;

        public PetsController(IAdminPetService adminPetService)
        {
            _adminPetService = adminPetService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminPetListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminPetListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var pets = await _adminPetService.GetListAsync(keyword);
            return Ok(ApiResponse<List<AdminPetListItemDto>>.Ok(pets));
        }

        [HttpGet("{templateId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminPetDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminPetDetailDto>>> GetDetail(string templateId)
        {
            var pet = await _adminPetService.GetDetailAsync(templateId);
            if (pet == null)
            {
                return NotFound(ApiResponse<AdminPetDetailDto>.Fail("灵宠模板不存在。"));
            }

            return Ok(ApiResponse<AdminPetDetailDto>.Ok(pet));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminPetDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminPetDetailDto>>> Save([FromBody] AdminPetDetailDto request)
        {
            try
            {
                var pet = await _adminPetService.SaveAsync(request);
                return Ok(ApiResponse<AdminPetDetailDto>.Ok(pet, "灵宠模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminPetDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{templateId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(string templateId)
        {
            try
            {
                var deleted = await _adminPetService.DeleteAsync(templateId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("灵宠模板不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("灵宠模板删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

