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
    [Route("api/admin/lottery-prizes")]
    [Authorize(Policy = AdminRoleCatalog.AdminOnlyPolicy)]
    public class LotteryPrizesController : ControllerBase
    {
        private readonly IAdminLotteryService _service;

        public LotteryPrizesController(IAdminLotteryService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<AdminLotteryPrizeListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<AdminLotteryPrizeListItemDto>>>> GetList([FromQuery] string? poolId = null)
        {
            var list = await _service.GetPrizeListAsync(poolId);
            return Ok(ApiResponse<List<AdminLotteryPrizeListItemDto>>.Ok(list));
        }

        [HttpGet("{prizeId}")]
        [ProducesResponseType(typeof(ApiResponse<AdminLotteryPrizeDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<AdminLotteryPrizeDetailDto>>> GetDetail(string prizeId)
        {
            var detail = await _service.GetPrizeDetailAsync(prizeId);
            if (detail == null) return NotFound(ApiResponse<AdminLotteryPrizeDetailDto>.Fail("奖项不存在。"));
            return Ok(ApiResponse<AdminLotteryPrizeDetailDto>.Ok(detail));
        }

        [HttpPost]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse<AdminLotteryPrizeDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminLotteryPrizeDetailDto>>> Save([FromBody] AdminLotteryPrizeDetailDto request)
        {
            try
            {
                var result = await _service.SavePrizeAsync(request);
                return Ok(ApiResponse<AdminLotteryPrizeDetailDto>.Ok(result, "保存成功"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<AdminLotteryPrizeDetailDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{prizeId}")]
        [Authorize(Policy = AdminPermissionCatalog.ConfigWritePolicy)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> Delete(string prizeId)
        {
            var deleted = await _service.DeletePrizeAsync(prizeId);
            if (!deleted) return NotFound(ApiResponse.Fail("奖项不存在。"));
            return Ok(ApiResponse.Ok("删除成功"));
        }
    }
}
#pragma warning restore CS1591
