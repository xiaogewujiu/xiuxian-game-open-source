using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 玩家建议反馈服务接口。
    /// </summary>
    public interface IFeedbackService
    {
        /// <summary>获取当前玩家的反馈列表。</summary>
        Task<List<FeedbackListItemDto>> GetMyListAsync(string playerId);
        /// <summary>获取当前玩家自己的反馈详情。</summary>
        Task<FeedbackDetailDto?> GetMyDetailAsync(string playerId, long feedbackId);
        /// <summary>提交一条玩家反馈。</summary>
        Task<FeedbackDetailDto> CreateAsync(string playerId, CreateFeedbackRequestDto request);
        /// <summary>获取后台反馈列表。</summary>
        Task<(List<FeedbackListItemDto> Items, int Total)> AdminGetListAsync(string? keyword, string? type, string? status, int pageIndex, int pageSize);
        /// <summary>获取后台反馈详情。</summary>
        Task<FeedbackDetailDto?> AdminGetDetailAsync(long feedbackId);
        /// <summary>处理反馈。</summary>
        Task<FeedbackDetailDto?> ProcessAsync(long feedbackId, string adminId, ProcessFeedbackRequestDto request);
    }
}
