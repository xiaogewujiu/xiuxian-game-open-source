using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 聊天服务接口
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// 获取历史消息
        /// </summary>
        Task<List<ChatMessageDto>> GetHistoryAsync(string playerId, string channelType, int count = 50);

        /// <summary>
        /// 发送消息
        /// </summary>
        Task<ChatMessageDto> SendMessageAsync(string playerId, SendMessageRequestDto request);
    }
}
