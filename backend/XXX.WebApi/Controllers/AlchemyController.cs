using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 炼丹系统接口。
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlchemyController : ControllerBase
    {
        private readonly IAlchemyService _alchemyService;

        /// <summary>
        /// 初始化炼丹控制器。
        /// </summary>
        public AlchemyController(IAlchemyService alchemyService) => _alchemyService = alchemyService;

        /// <summary>
        /// 从当前登录上下文中读取玩家编号。
        /// </summary>
        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取当前玩家的炼丹总览。
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<AlchemyDto>>> GetInfo()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var info = await _alchemyService.GetAlchemyInfoAsync(playerId);
            return Ok(ApiResponse<AlchemyDto>.Ok(info));
        }

        /// <summary>
        /// 获取当前玩家已学习的丹方。
        /// </summary>
        [HttpGet("recipes/learned")]
        public async Task<ActionResult<ApiResponse<List<AlchemyRecipeDto>>>> GetLearnedRecipes()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var recipes = await _alchemyService.GetLearnedRecipesAsync(playerId);
            return Ok(ApiResponse<List<AlchemyRecipeDto>>.Ok(recipes));
        }

        /// <summary>
        /// 获取当前玩家已解锁的丹方。
        /// </summary>
        [HttpGet("recipes/available")]
        public async Task<ActionResult<ApiResponse<List<AlchemyRecipeDto>>>> GetAvailableRecipes()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var recipes = await _alchemyService.GetAvailableRecipesAsync(playerId);
            return Ok(ApiResponse<List<AlchemyRecipeDto>>.Ok(recipes));
        }

        /// <summary>
        /// 学习指定丹方。
        /// </summary>
        [HttpPost("recipes/{recipeId}/learn")]
        public async Task<ActionResult<ApiResponse<bool>>> LearnRecipe(string recipeId)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _alchemyService.LearnRecipeAsync(playerId, recipeId);
            if (!result)
            {
                return BadRequest(ApiResponse<bool>.Fail("学习失败"));
            }

            return Ok(ApiResponse<bool>.Ok(true, "学习成功"));
        }

        /// <summary>
        /// 发起炼丹。
        /// </summary>
        [HttpPost("craft")]
        public async Task<ActionResult<ApiResponse<AlchemyResultDto>>> StartAlchemy([FromBody] StartAlchemyRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _alchemyService.StartAlchemyAsync(playerId, request);
            return Ok(ApiResponse<AlchemyResultDto>.Ok(result, result.Message));
        }

        /// <summary>
        /// 领取已完成的炼丹结果。
        /// </summary>
        [HttpPost("collect")]
        public async Task<ActionResult<ApiResponse<AlchemyResultDto>>> Collect()
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(ApiResponse.Fail("未登录"));
            }

            var result = await _alchemyService.CollectPillAsync(playerId);
            if (result == null)
            {
                return BadRequest(ApiResponse<AlchemyResultDto>.Fail("当前没有可领取的炼丹结果"));
            }

            return Ok(ApiResponse<AlchemyResultDto>.Ok(result, result.Message));
        }
    }
}
