using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 灵宠系统接口。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PetController : ControllerBase
    {
        private readonly IPetService _petService;

        /// <summary>
        /// 初始化灵宠控制器。
        /// </summary>
        public PetController(IPetService petService) => _petService = petService;

        /// <summary>
        /// 从当前登录上下文中读取玩家编号。
        /// </summary>
        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取当前玩家的全部灵宠。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<PetDto>>>> GetAll()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var pets = await _petService.GetPlayerPetsAsync(playerId);
            return Ok(ApiResponse<List<PetDto>>.Ok(pets));
        }

        /// <summary>
        /// 获取指定灵宠详情。
        /// </summary>
        [HttpGet("{petId}")]
        public async Task<ActionResult<ApiResponse<PetDto>>> GetDetail(string petId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var pet = await _petService.GetPetDetailAsync(playerId, petId);
            if (pet == null)
            {
                return NotFound(ApiResponse<PetDto>.Fail("灵宠不存在"));
            }

            return Ok(ApiResponse<PetDto>.Ok(pet));
        }

        /// <summary>
        /// 获取当前出战灵宠。
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<PetDto>>> GetActive()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var pet = await _petService.GetActivePetAsync(playerId);
            if (pet == null)
            {
                return NotFound(ApiResponse<PetDto>.Fail("没有出战灵宠"));
            }

            return Ok(ApiResponse<PetDto>.Ok(pet));
        }

        /// <summary>
        /// 设置指定灵宠为出战状态。
        /// </summary>
        [HttpPost("active")]
        public async Task<ActionResult<ApiResponse<bool>>> SetActive([FromQuery] string petId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _petService.SetActivePetAsync(playerId, petId);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("设置失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "设置成功"));
        }

        /// <summary>
        /// 取消当前出战灵宠。
        /// </summary>
        [HttpDelete("active")]
        public async Task<ActionResult<ApiResponse<bool>>> ClearActive()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _petService.ClearActivePetAsync(playerId);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("召回失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "召回成功"));
        }

        /// <summary>
        /// 喂养指定灵宠。
        /// </summary>
        [HttpPost("feed")]
        public async Task<ActionResult<ApiResponse<bool>>> Feed([FromBody] PetFeedRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _petService.FeedPetAsync(playerId, request);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("喂养失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "喂养成功"));
        }

        /// <summary>
        /// 进化指定灵宠。
        /// </summary>
        [HttpPost("evolve")]
        public async Task<ActionResult<ApiResponse<bool>>> Evolve([FromBody] PetEvolveRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _petService.EvolvePetAsync(playerId, request);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("进化失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "进化成功"));
        }

        /// <summary>
        /// 放生指定灵宠。
        /// </summary>
        [HttpDelete("{petId}")]
        public async Task<ActionResult<ApiResponse<bool>>> Release(string petId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _petService.ReleasePetAsync(playerId, petId);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("放生失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "放生成功"));
        }
    }
}
