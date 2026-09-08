using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GemController : ControllerBase
    {
        private readonly IGemService _gemService;

        public GemController(IGemService gemService)
        {
            _gemService = gemService;
        }

        private string? GetCurrentPlayerId() => User.GetCurrentPlayerId();

        [HttpGet("templates")]
        public async Task<ActionResult<ApiResponse<List<GemTemplateDto>>>> GetTemplates()
        {
            var result = await _gemService.GetTemplatesAsync();
            return Ok(ApiResponse<List<GemTemplateDto>>.Ok(result));
        }

        [HttpGet("inventory")]
        public async Task<ActionResult<ApiResponse<List<GemInventoryDto>>>> GetInventory()
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var result = await _gemService.GetInventoryGemsAsync(playerId);
            return Ok(ApiResponse<List<GemInventoryDto>>.Ok(result));
        }

        [HttpPost("socket")]
        public async Task<ActionResult<ApiResponse>> Socket([FromBody] SocketGemDto dto)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                await _gemService.SocketGemAsync(playerId, dto);
                return Ok(ApiResponse.Ok("宝石镶嵌成功。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpPost("unsocket")]
        public async Task<ActionResult<ApiResponse>> Unsocket([FromBody] UnsocketGemDto dto)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                await _gemService.UnsocketGemAsync(playerId, dto);
                return Ok(ApiResponse.Ok("宝石取下成功。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpPost("synthesize")]
        public async Task<ActionResult<ApiResponse>> Synthesize([FromBody] SynthesizeGemDto dto)
        {
            var playerId = GetCurrentPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            try
            {
                await _gemService.SynthesizeGemAsync(playerId, dto);
                return Ok(ApiResponse.Ok("宝石合成成功。"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
