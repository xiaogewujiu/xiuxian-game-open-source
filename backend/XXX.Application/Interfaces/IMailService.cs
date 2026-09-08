using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 玩家邮件服务接口。
    /// </summary>
    public interface IMailService
    {
        /// <summary>
        /// 获取邮件列表（个人邮件 + 未过期全服邮件）。
        /// </summary>
        Task<MailPagedResultDto> GetMailsAsync(string playerId, int page = 1, int pageSize = 20);

        /// <summary>
        /// 获取邮件详情。
        /// </summary>
        Task<MailDetailDto?> GetMailDetailAsync(string playerId, long mailId);

        /// <summary>
        /// 标记邮件已读。
        /// </summary>
        Task<bool> MarkAsReadAsync(string playerId, long mailId);

        /// <summary>
        /// 领取邮件附件。
        /// </summary>
        Task<MailClaimResultDto> ClaimAttachmentsAsync(string playerId, long mailId);

        /// <summary>
        /// 一键领取所有邮件附件。
        /// </summary>
        Task<MailClaimAllResultDto> ClaimAllAsync(string playerId);

        /// <summary>
        /// 删除邮件。
        /// </summary>
        Task<bool> DeleteMailAsync(string playerId, long mailId);

        /// <summary>
        /// 全部标为已读。
        /// </summary>
        Task<int> MarkAllAsReadAsync(string playerId);

        /// <summary>
        /// 发送邮件（系统内部调用）。
        /// </summary>
        Task<long> SendMailAsync(string? recipientId, string senderType, string senderName, string title, string content, string? attachmentsJson = null, bool isGlobal = false);

        /// <summary>
        /// 清理过期邮件。
        /// </summary>
        Task<int> CleanupExpiredMailsAsync();
    }
}
