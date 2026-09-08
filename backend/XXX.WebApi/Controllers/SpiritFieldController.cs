using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 灵田系统接口。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SpiritFieldController : ControllerBase
    {
        private readonly ISpiritFieldService _spiritFieldService;

        /// <summary>
        /// 初始化灵田控制器。
        /// </summary>
        public SpiritFieldController(ISpiritFieldService spiritFieldService) => _spiritFieldService = spiritFieldService;

        /// <summary>
        /// 从当前登录上下文中读取玩家编号。
        /// </summary>
        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取灵田总览。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<SpiritFieldDto>>> GetInfo()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var info = await _spiritFieldService.GetSpiritFieldAsync(playerId);
            return Ok(ApiResponse<SpiritFieldDto>.Ok(info));
        }

        /// <summary>
        /// 获取当前可种植的作物模板。
        /// </summary>
        [HttpGet("crops")]
        public async Task<ActionResult<ApiResponse<List<CropTemplateDto>>>> GetAvailableCrops()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var crops = await _spiritFieldService.GetAvailableCropsAsync(playerId);
            return Ok(ApiResponse<List<CropTemplateDto>>.Ok(crops));
        }

        /// <summary>
        /// 在指定地块播种。
        /// </summary>
        [HttpPost("plant")]
        public async Task<ActionResult<ApiResponse<bool>>> Plant([FromBody] PlantCropRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _spiritFieldService.PlantCropAsync(playerId, request);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("种植失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "种植成功"));
        }

        /// <summary>
        /// 收获指定地块的作物。
        /// </summary>
        [HttpPost("harvest")]
        public async Task<ActionResult<ApiResponse<HarvestResultDto>>> Harvest([FromBody] HarvestCropRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _spiritFieldService.HarvestCropAsync(playerId, request);
            if (result == null)
            {
                return BadRequest(ApiResponse<HarvestResultDto>.Fail("收获失败"));
            }

            return Ok(ApiResponse<HarvestResultDto>.Ok(result, $"收获成功，获得 {result.CropName} x{result.Quantity}"));
        }

        /// <summary>
        /// 使用道具催熟指定地块。
        /// </summary>
        [HttpPost("speedup")]
        public async Task<ActionResult<ApiResponse<bool>>> SpeedUp([FromBody] SpeedUpCropRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _spiritFieldService.SpeedUpCropAsync(playerId, request);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("加速失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "加速成功"));
        }

        /// <summary>
        /// 升级指定地块。
        /// </summary>
        [HttpPost("upgrade/{plotNumber}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpgradePlot(int plotNumber)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _spiritFieldService.UpgradePlotAsync(playerId, plotNumber);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("升级失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "升级成功"));
        }
    }
}
