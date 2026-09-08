using Microsoft.Extensions.Logging;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台邮件服务。
    /// </summary>
    public class AdminMailService : IAdminMailService
    {
        private readonly DbContext _dbContext;
        private readonly IRepository<MailMessageEntity> _mailRepository;
        private readonly IMailService _mailService;
        private readonly ILogger<AdminMailService> _logger;

        public AdminMailService(
            DbContext dbContext,
            IRepository<MailMessageEntity> mailRepository,
            IMailService mailService,
            ILogger<AdminMailService> logger)
        {
            _dbContext = dbContext;
            _mailRepository = mailRepository;
            _mailService = mailService;
            _logger = logger;
        }

        /// <summary>
        /// 获取邮件列表。
        /// </summary>
        public async Task<List<AdminMailListItemDto>> GetListAsync(string? keyword = null, bool? isGlobal = null, int take = 200)
        {
            var query = _dbContext.Db.Queryable<MailMessageEntity>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(m =>
                    m.Title.Contains(kw) ||
                    m.SenderName.Contains(kw) ||
                    (m.RecipientId != null && m.RecipientId.Contains(kw)));
            }

            if (isGlobal.HasValue)
            {
                query = query.Where(m => m.IsGlobal == isGlobal.Value);
            }

            var mails = await query
                .OrderByDescending(m => m.CreatedAt)
                .Take(Math.Max(1, take))
                .ToListAsync();

            return mails.Select(MapToListItem).ToList();
        }

        /// <summary>
        /// 获取邮件详情。
        /// </summary>
        public async Task<AdminMailDetailDto?> GetDetailAsync(long mailId)
        {
            var mail = await _mailRepository.GetByIdAsync(mailId);
            if (mail == null) return null;

            return new AdminMailDetailDto
            {
                Id = mail.Id,
                RecipientId = mail.RecipientId,
                SenderType = mail.SenderType,
                SenderName = mail.SenderName,
                Title = mail.Title,
                Content = mail.Content,
                AttachmentsJson = mail.AttachmentsJson,
                IsGlobal = mail.IsGlobal,
                CreatedAt = mail.CreatedAt,
                ExpireAt = mail.ExpireAt
            };
        }

        /// <summary>
        /// 发送邮件。
        /// </summary>
        public async Task<AdminMailDetailDto> SendMailAsync(AdminSendMailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new InvalidOperationException("邮件标题不能为空。");

            if (string.IsNullOrWhiteSpace(dto.Content))
                throw new InvalidOperationException("邮件正文不能为空。");

            if (!dto.IsGlobal && string.IsNullOrWhiteSpace(dto.RecipientId))
                throw new InvalidOperationException("非全服邮件必须指定收件人。");

            var mailId = await _mailService.SendMailAsync(
                dto.RecipientId,
                "GM",
                dto.SenderName,
                dto.Title,
                dto.Content,
                dto.AttachmentsJson,
                dto.IsGlobal);

            var detail = await GetDetailAsync(mailId);
            return detail!;
        }

        /// <summary>
        /// 删除邮件。
        /// </summary>
        public async Task<bool> DeleteMailAsync(long mailId)
        {
            var mail = await _mailRepository.GetByIdAsync(mailId);
            if (mail == null) return false;

            await _mailRepository.DeleteAsync(mailId);
            _logger.LogInformation("邮件已删除。MailId={MailId}", mailId);
            return true;
        }

        /// <summary>
        /// 撤回全服邮件。
        /// </summary>
        public async Task<bool> RecallGlobalMailAsync(long mailId)
        {
            var mail = await _mailRepository.GetByIdAsync(mailId);
            if (mail == null || !mail.IsGlobal) return false;

            _dbContext.BeginTransaction();
            try
            {
                // 删除领取记录
                await _dbContext.Db.Deleteable<MailGlobalClaimRecordEntity>()
                    .Where(r => r.MailId == mailId)
                    .ExecuteCommandAsync();

                // 删除邮件本身
                await _mailRepository.DeleteAsync(mailId);

                _dbContext.CommitTransaction();
                _logger.LogInformation("全服邮件已撤回。MailId={MailId}", mailId);
                return true;
            }
            catch
            {
                _dbContext.RollbackTransaction();
                throw;
            }
        }

        private static AdminMailListItemDto MapToListItem(MailMessageEntity mail)
        {
            return new AdminMailListItemDto
            {
                Id = mail.Id,
                RecipientId = mail.RecipientId,
                SenderType = mail.SenderType,
                SenderName = mail.SenderName,
                Title = mail.Title,
                IsGlobal = mail.IsGlobal,
                HasAttachments = !string.IsNullOrWhiteSpace(mail.AttachmentsJson),
                CreatedAt = mail.CreatedAt,
                ExpireAt = mail.ExpireAt
            };
        }
    }
}
