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
    [Route("api/admin/lottery-pools")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class LotteryPoolsController : ControllerBase
    {
        private readonly IAdminLotteryService _service;

        public LotteryPoolsController(IAdminLotteryService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminLotteryPoolListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminLotteryPoolListItemDto>>>> GetList([FromQuery] string? keyword = null)
        {
            var list = await _service.GetPoolListAsync(keyword);
            return Ok(ApiResponse<List<AdminLotteryPoolListItemDto>>.Ok(list));
        }

        [HttpGet("{poolId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminLotteryPoolDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminLotteryPoolDetailDto>>> GetDetail(string poolId)
        {
            var detail = await _service.GetPoolDetailAsync(poolId);
            if (detail == null) return NotFound(ApiResponse<AdminLotteryPoolDetailDto>.Fail("抽奖池不存在。"));
            return Ok(ApiResponse<AdminLotteryPoolDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminLotteryPoolDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminLotteryPoolDetailDto>>> Save([FromBody] AdminLotteryPoolDetailDto request)
        {
            try
            {
                var result = await _service.SavePoolAsync(request);
                return Ok(ApiResponse<AdminLotteryPoolDetailDto>.Ok(result, "保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminLotteryPoolDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{poolId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> Delete(string poolId)
        {
            try
            {
                var deleted = await _service.DeletePoolAsync(poolId);
                if (!deleted) return NotFound(ApiResponse.Fail("抽奖池不存在。"));
                return Ok(ApiResponse.Ok("删除成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
#pragma warning restore CS1591
