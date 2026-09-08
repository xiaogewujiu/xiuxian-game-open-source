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
    [Route("api/admin/equipments")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class EquipmentsController : ControllerBase
    {
        private readonly IAdminEquipmentService _adminEquipmentService;

        public EquipmentsController(IAdminEquipmentService adminEquipmentService)
        {
            _adminEquipmentService = adminEquipmentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminEquipmentListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminEquipmentListItemDto>>>> GetList([FromQuery] string? keyword = null, [FromQuery] int? slot = null)
        {
            var equipments = await _adminEquipmentService.GetListAsync(keyword, slot);
            return Ok(ApiResponse<List<AdminEquipmentListItemDto>>.Ok(equipments));
        }

        [HttpGet("{equipmentId:int}")]
        [ProducesResponseType(typeof(ApiResponse<AdminEquipmentDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminEquipmentDetailDto>>> GetDetail(int equipmentId)
        {
            var equipment = await _adminEquipmentService.GetDetailAsync(equipmentId);
            if (equipment == null)
            {
                return NotFound(ApiResponse<AdminEquipmentDetailDto>.Fail("装备模板不存在。"));
            }

            return Ok(ApiResponse<AdminEquipmentDetailDto>.Ok(equipment));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminEquipmentDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminEquipmentDetailDto>>> Save([FromBody] AdminEquipmentDetailDto request)
        {
            try
            {
                var equipment = await _adminEquipmentService.SaveAsync(request);
                return Ok(ApiResponse<AdminEquipmentDetailDto>.Ok(equipment, "装备模板保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminEquipmentDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{equipmentId:int}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Delete(int equipmentId)
        {
            try
            {
                var deleted = await _adminEquipmentService.DeleteAsync(equipmentId);
                if (!deleted)
                {
                    return NotFound(ApiResponse.Fail("装备模板不存在或删除失败。"));
                }

                return Ok(ApiResponse.Ok("装备模板删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}

#pragma warning restore CS1591

