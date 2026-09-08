using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XXX.Application.DTOs;
using XXX.Application.DTOs.Responses;
using XXX.Application.Interfaces;
using XXX.WebApi.Extensions;

namespace XXX.WebApi.Controllers
{
    /// <summary>
    /// 聊天控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        /// <summary>
        /// 初始化聊天控制器。
        /// </summary>
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private string? GetPlayerId() => User.GetCurrentPlayerId();

        /// <summary>
        /// 获取历史消息
        /// </summary>
        [HttpGet("history/{channelType}")]
        public async Task<ActionResult<ApiResponse<List<ChatMessageDto>>>> GetHistory(string channelType, [FromQuery] int count = 50)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var messages = await _chatService.GetHistoryAsync(playerId, channelType, count);
            return Ok(ApiResponse<List<ChatMessageDto>>.Ok(messages));
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        [HttpPost("send")]
        public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage([FromBody] SendMessageRequestDto request)
        {
            var playerId = GetPlayerId();
            if (string.IsNullOrEmpty(playerId)) return Unauthorized(ApiResponse.Fail("未登录"));

            var message = await _chatService.SendMessageAsync(playerId, request);
            return Ok(ApiResponse<ChatMessageDto>.Ok(message, "发送成功"));
        }
    }
}
