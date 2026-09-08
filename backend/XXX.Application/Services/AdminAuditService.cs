using SqlSugar;
using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台审计日志服务。
    /// </summary>
    public class AdminAuditService : IAdminAuditService
    {
        private readonly IRepository<AdminAuditLogEntity> _auditRepository;

        /// <summary>
        /// 初始化审计日志服务。
        /// </summary>
        public AdminAuditService(IRepository<AdminAuditLogEntity> auditRepository)
        {
            _auditRepository = auditRepository;
        }

        /// <summary>
        /// 获取审计日志列表。
        /// </summary>
        public async Task<List<AdminAuditLogListItemDto>> GetListAsync(string? keyword = null, int take = 200, bool? success = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _auditRepository.Db.Queryable<AdminAuditLogEntity>();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(log =>
                    (log.OperatorName != null && log.OperatorName.Contains(normalizedKeyword)) ||
                    log.Path.Contains(normalizedKeyword) ||
                    (log.ResourceKey != null && log.ResourceKey.Contains(normalizedKeyword)) ||
                    (log.TargetId != null && log.TargetId.Contains(normalizedKeyword)));
            }

            query = query.WhereIF(success.HasValue, log => log.Success == success!.Value);

            var logs = await query
                .OrderBy(log => log.CreateTime, OrderByType.Desc)
                .Take(Math.Max(1, take))
                .ToListAsync();

            return logs.Select(MapListItem).ToList();
        }

        /// <summary>
        /// 获取单条审计日志详情。
        /// </summary>
        public async Task<AdminAuditLogDetailDto?> GetDetailAsync(string logId)
        {
            if (string.IsNullOrWhiteSpace(logId))
            {
                return null;
            }

            var log = await _auditRepository.GetByIdAsync(logId.Trim());
            if (log == null)
            {
                return null;
            }

            return new AdminAuditLogDetailDto
            {
                LogId = log.LogId,
                OperatorId = log.OperatorId,
                OperatorName = log.OperatorName,
                OperatorRole = log.OperatorRole,
                HttpMethod = log.HttpMethod,
                Path = log.Path,
                ResourceKey = log.ResourceKey,
                TargetId = log.TargetId,
                RequestJson = log.RequestJson,
                ResponseJson = log.ResponseJson,
                BeforeJson = log.BeforeJson,
                AfterJson = log.AfterJson,
                DiffJson = log.DiffJson,
                Success = log.Success,
                StatusCode = log.StatusCode,
                ErrorMessage = log.ErrorMessage,
                IpAddress = log.IpAddress,
                CreateTime = log.CreateTime
            };
        }

        private static AdminAuditLogListItemDto MapListItem(AdminAuditLogEntity log)
        {
            return new AdminAuditLogListItemDto
            {
                LogId = log.LogId,
                OperatorName = log.OperatorName,
                OperatorRole = log.OperatorRole,
                HttpMethod = log.HttpMethod,
                Path = log.Path,
                ResourceKey = log.ResourceKey,
                TargetId = log.TargetId,
                Success = log.Success,
                StatusCode = log.StatusCode,
                ErrorMessage = log.ErrorMessage,
                CreateTime = log.CreateTime
            };
        }
    }
}
