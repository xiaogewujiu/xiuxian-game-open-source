using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台邮件服务接口。
    /// </summary>
    public interface IAdminMailService
    {
        /// <summary>
        /// 获取邮件列表。
        /// </summary>
        Task<List<AdminMailListItemDto>> GetListAsync(string? keyword = null, bool? isGlobal = null, int take = 200);

        /// <summary>
        /// 获取邮件详情。
        /// </summary>
        Task<AdminMailDetailDto?> GetDetailAsync(long mailId);

        /// <summary>
        /// 发送邮件。
        /// </summary>
        Task<AdminMailDetailDto> SendMailAsync(AdminSendMailDto dto);

        /// <summary>
        /// 删除邮件。
        /// </summary>
        Task<bool> DeleteMailAsync(long mailId);

        /// <summary>
        /// 撤回全服邮件。
        /// </summary>
        Task<bool> RecallGlobalMailAsync(long mailId);
    }
}
