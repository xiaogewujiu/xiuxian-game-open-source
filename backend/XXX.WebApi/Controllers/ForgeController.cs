using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 中文注释：
    /// 锻造控制器。
    /// 当前只暴露两个最核心接口：
    /// 1. 获取锻造总览；
    /// 2. 按图纸执行锻造。
    /// 这样前端锻造弹窗就能彻底摆脱本地假数据。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ForgeController : ControllerBase
    {
        private readonly IForgeService _forgeService;

        /// <summary>
        /// 初始化锻造控制器。
        /// </summary>
        public ForgeController(IForgeService forgeService)
        {
            _forgeService = forgeService;
        }

        /// <summary>
        /// 获取锻造系统总览。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<ForgeOverviewDto>>> GetOverview()
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<ForgeOverviewDto>.Fail("未登录"));
            }

            var overview = await _forgeService.GetForgeOverviewAsync(playerId);
            return Ok(ApiResponse<ForgeOverviewDto>.Ok(overview));
        }

        /// <summary>
        /// 执行一次锻造。
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ForgeEquipmentResultDto>>> Forge([FromBody] ForgeEquipmentRequestDto request)
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<ForgeEquipmentResultDto>.Fail("未登录"));
            }

            var result = await _forgeService.ForgeAsync(playerId, request);
            return Ok(ApiResponse<ForgeEquipmentResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 领取已完成的锻造结果。
        /// </summary>
        [HttpPost("collect")]
        public async Task<ActionResult<ApiResponse<ForgeEquipmentResultDto>>> Collect()
        {
            var playerId = User.GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse<ForgeEquipmentResultDto>.Fail("未登录"));
            }

            var result = await _forgeService.CollectForgeResultAsync(playerId);
            if (result == null)
            {
                return BadRequest(ApiResponse<ForgeEquipmentResultDto>.Fail("当前没有可领取的锻造结果"));
            }

            return Ok(ApiResponse<ForgeEquipmentResultDto>.Ok(result, result.Message));
        }
    }
}
