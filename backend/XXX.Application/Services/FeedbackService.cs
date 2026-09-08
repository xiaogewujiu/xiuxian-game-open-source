using Microsoft.Extensions.Logging;
using SqlSugar;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Data;

namespace XXX.Application.Services
{
    /// <summary>
    /// 建议反馈业务服务。
    /// 统一处理玩家隔离、反馈校验、附件关联和管理员状态流转。
    /// </summary>
    public sealed class FeedbackService : IFeedbackService
    {
        private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Suggestion", "Bug", "Gameplay", "Account", "Other"
        };

        private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pending", "Processing", "Resolved", "Rejected", "Closed"
        };

        private readonly DbContext _dbContext;
        private readonly ILogger<FeedbackService> _logger;

        /// <summary>初始化反馈服务。</summary>
        public FeedbackService(DbContext dbContext, ILogger<FeedbackService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<List<FeedbackListItemDto>> GetMyListAsync(string playerId)
        {
            var feedbacks = await _dbContext.Db.Queryable<PlayerFeedbackEntity>()
                .Where(x => x.PlayerId == playerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return await MapListAsync(feedbacks);
        }

        /// <inheritdoc />
        public async Task<FeedbackDetailDto?> GetMyDetailAsync(string playerId, long feedbackId)
        {
            var feedback = await _dbContext.Db.Queryable<PlayerFeedbackEntity>()
                .Where(x => x.Id == feedbackId && x.PlayerId == playerId)
                .FirstAsync();
            return feedback == null ? null : await MapDetailAsync(feedback, false);
        }

        /// <inheritdoc />
        public async Task<FeedbackDetailDto> CreateAsync(string playerId, CreateFeedbackRequestDto request)
        {
            ValidateContent(request.Type, request.Title, request.Content);
            ValidateAttachments(request.Attachments);

            var now = DateTime.Now;
            var feedback = new PlayerFeedbackEntity
            {
                PlayerId = playerId,
                Type = request.Type.Trim(),
                Title = request.Title.Trim(),
                Content = request.Content.Trim(),
                Status = "Pending",
                CreatedAt = now,
                UpdatedAt = now
            };

            var attachmentRows = request.Attachments.Select((attachment, index) => new PlayerFeedbackAttachmentEntity
            {
                RelativePath = attachment.RelativePath.Trim(),
                OriginalFileName = attachment.OriginalFileName,
                ContentType = attachment.ContentType.Trim(),
                FileSize = attachment.FileSize,
                SortOrder = index,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList();

            await _dbContext.ExecuteInTransactionAsync(async () =>
            {
                feedback.Id = await _dbContext.Db.Insertable(feedback).ExecuteReturnIdentityAsync();
                foreach (var attachment in attachmentRows)
                {
                    attachment.FeedbackId = feedback.Id;
                }

                if (attachmentRows.Count > 0)
                {
                    await _dbContext.Db.Insertable(attachmentRows).ExecuteCommandAsync();
                }
            });

            return (await GetMyDetailAsync(playerId, feedback.Id))!;
        }

        /// <inheritdoc />
        public async Task<(List<FeedbackListItemDto> Items, int Total)> AdminGetListAsync(
            string? keyword, string? type, string? status, int pageIndex, int pageSize)
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _dbContext.Db.Queryable<PlayerFeedbackEntity, UserEntity>(
                (feedback, player) => new JoinQueryInfos(JoinType.Left, feedback.PlayerId == player.GID));

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var value = keyword.Trim();
                query = query.Where((feedback, player) =>
                    feedback.Title.Contains(value) || feedback.Content.Contains(value) ||
                    feedback.PlayerId.Contains(value) || player.Name.Contains(value));
            }

            if (!string.IsNullOrWhiteSpace(type)) query = query.Where((feedback, player) => feedback.Type == type.Trim());
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where((feedback, player) => feedback.Status == status.Trim());

            RefAsync<int> total = 0;
            var rows = await query
                .OrderBy((feedback, player) => feedback.CreatedAt, OrderByType.Desc)
                .Select((feedback, player) => new FeedbackListItemDto
                {
                    Id = feedback.Id,
                    PlayerId = feedback.PlayerId,
                    PlayerName = player.Name,
                    Type = feedback.Type,
                    Title = feedback.Title,
                    Status = feedback.Status,
                    CreatedAt = feedback.CreatedAt,
                    HandledByAdminId = feedback.HandledByAdminId,
                    HandledAt = feedback.HandledAt,
                    AttachmentCount = SqlFunc.Subqueryable<PlayerFeedbackAttachmentEntity>()
                        .Where(a => a.FeedbackId == feedback.Id).Count()
                })
                .ToPageListAsync(pageIndex, pageSize, total);

            return (rows, total.Value);
        }

        /// <inheritdoc />
        public async Task<FeedbackDetailDto?> AdminGetDetailAsync(long feedbackId)
        {
            var feedback = await _dbContext.Db.Queryable<PlayerFeedbackEntity>()
                .Where(x => x.Id == feedbackId)
                .FirstAsync();
            return feedback == null ? null : await MapDetailAsync(feedback, true);
        }

        /// <inheritdoc />
        public async Task<FeedbackDetailDto?> ProcessAsync(long feedbackId, string adminId, ProcessFeedbackRequestDto request)
        {
            if (!AllowedStatuses.Contains(request.Status)) throw new InvalidOperationException("反馈状态不合法。");
            if (request.AdminReply?.Length > 5000 || request.InternalNote?.Length > 5000)
                throw new InvalidOperationException("处理回复或内部备注不能超过 5000 个字符。");

            var feedback = await _dbContext.Db.Queryable<PlayerFeedbackEntity>()
                .Where(x => x.Id == feedbackId).FirstAsync();
            if (feedback == null) return null;

            var oldStatus = feedback.Status;
            var now = DateTime.Now;
            feedback.Status = request.Status.Trim();
            feedback.AdminReply = string.IsNullOrWhiteSpace(request.AdminReply) ? null : request.AdminReply.Trim();
            feedback.InternalNote = string.IsNullOrWhiteSpace(request.InternalNote) ? null : request.InternalNote.Trim();
            feedback.HandledByAdminId = adminId;
            feedback.HandledAt = now;
            feedback.UpdatedAt = now;

            await _dbContext.ExecuteInTransactionAsync(async () =>
            {
                await _dbContext.Db.Updateable(feedback)
                    .UpdateColumns(x => new
                    {
                        x.Status, x.AdminReply, x.InternalNote, x.HandledByAdminId, x.HandledAt, x.UpdatedAt
                    }).ExecuteCommandAsync();

                await _dbContext.Db.Insertable(new PlayerFeedbackStatusHistoryEntity
                {
                    FeedbackId = feedback.Id,
                    FromStatus = oldStatus,
                    ToStatus = feedback.Status,
                    AdminId = adminId,
                    ReplySnapshot = feedback.AdminReply,
                    NoteSnapshot = feedback.InternalNote,
                    CreatedAt = now,
                    UpdatedAt = now
                }).ExecuteCommandAsync();
            });

            _logger.LogInformation("管理员处理建议反馈。FeedbackId={FeedbackId}, AdminId={AdminId}, Status={Status}", feedbackId, adminId, feedback.Status);
            return await AdminGetDetailAsync(feedbackId);
        }

        private async Task<List<FeedbackListItemDto>> MapListAsync(List<PlayerFeedbackEntity> feedbacks)
        {
            if (feedbacks.Count == 0) return [];
            var ids = feedbacks.Select(x => x.Id).ToList();
            var counts = await _dbContext.Db.Queryable<PlayerFeedbackAttachmentEntity>()
                .Where(x => ids.Contains(x.FeedbackId))
                .GroupBy(x => x.FeedbackId)
                .Select(x => new { FeedbackId = x.FeedbackId, Count = SqlFunc.AggregateCount(x.Id) })
                .ToListAsync();
            var countMap = counts.ToDictionary(x => x.FeedbackId, x => x.Count);
            return feedbacks.Select(x => new FeedbackListItemDto
            {
                Id = x.Id, PlayerId = x.PlayerId, Type = x.Type, Title = x.Title,
                Status = x.Status, CreatedAt = x.CreatedAt, HandledByAdminId = x.HandledByAdminId,
                HandledAt = x.HandledAt, AttachmentCount = countMap.GetValueOrDefault(x.Id)
            }).ToList();
        }

        private async Task<FeedbackDetailDto> MapDetailAsync(PlayerFeedbackEntity feedback, bool includeAdminFields)
        {
            var player = await _dbContext.Db.Queryable<UserEntity>().Where(x => x.GID == feedback.PlayerId).FirstAsync();
            var attachments = await _dbContext.Db.Queryable<PlayerFeedbackAttachmentEntity>()
                .Where(x => x.FeedbackId == feedback.Id).OrderBy(x => x.SortOrder).ToListAsync();
            var histories = includeAdminFields
                ? await _dbContext.Db.Queryable<PlayerFeedbackStatusHistoryEntity>()
                    .Where(x => x.FeedbackId == feedback.Id).OrderByDescending(x => x.CreatedAt).ToListAsync()
                : [];

            return new FeedbackDetailDto
            {
                Id = feedback.Id, PlayerId = feedback.PlayerId, PlayerName = player?.Name ?? feedback.PlayerId,
                Type = feedback.Type, Title = feedback.Title, Content = feedback.Content, Status = feedback.Status,
                CreatedAt = feedback.CreatedAt, HandledByAdminId = feedback.HandledByAdminId, HandledAt = feedback.HandledAt,
                AdminReply = feedback.AdminReply,
                InternalNote = includeAdminFields ? feedback.InternalNote : null,
                Attachments = attachments.Select(x => new FeedbackAttachmentDto
                {
                    Id = x.Id, RelativePath = x.RelativePath, OriginalFileName = x.OriginalFileName,
                    ContentType = x.ContentType, FileSize = x.FileSize, SortOrder = x.SortOrder
                }).ToList(),
                StatusHistory = histories.Select(x => new FeedbackStatusHistoryDto
                {
                    FromStatus = x.FromStatus, ToStatus = x.ToStatus, AdminId = x.AdminId,
                    ReplySnapshot = x.ReplySnapshot, NoteSnapshot = x.NoteSnapshot, CreatedAt = x.CreatedAt
                }).ToList()
            };
        }

        private static void ValidateContent(string type, string title, string content)
        {
            if (!AllowedTypes.Contains(type?.Trim() ?? string.Empty)) throw new InvalidOperationException("反馈类型不合法。");
            if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 80) throw new InvalidOperationException("反馈标题不能为空且不能超过 80 个字符。");
            if (string.IsNullOrWhiteSpace(content) || content.Trim().Length > 2000) throw new InvalidOperationException("反馈内容不能为空且不能超过 2000 个字符。");
        }

        private static void ValidateAttachments(List<CreateFeedbackAttachmentDto>? attachments)
        {
            if (attachments == null || attachments.Count == 0) return;
            if (attachments.Count > 5) throw new InvalidOperationException("最多上传 5 张图片。");
            if (attachments.Any(x => string.IsNullOrWhiteSpace(x.RelativePath) || x.FileSize <= 0 || x.FileSize > 5 * 1024 * 1024))
                throw new InvalidOperationException("反馈图片信息不合法。");
            if (attachments.Sum(x => x.FileSize) > 20 * 1024 * 1024) throw new InvalidOperationException("反馈图片总大小不能超过 20MB。");
            if (attachments.Any(x => !x.RelativePath.StartsWith("/uploads/feedback/", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("反馈图片路径不合法。");
        }
    }
}
