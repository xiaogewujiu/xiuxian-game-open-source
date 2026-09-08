using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Repositories;

namespace XXX.Application.Services
{
    /// <summary>
    /// 管理后台用户服务。
    /// </summary>
    public class AdminUserService : IAdminUserService
    {
        private readonly IRepository<AdminUserEntity> _adminUserRepository;

        /// <summary>
        /// 初始化管理员用户服务。
        /// </summary>
        public AdminUserService(IRepository<AdminUserEntity> adminUserRepository)
        {
            _adminUserRepository = adminUserRepository;
        }

        /// <summary>
        /// 获取管理员列表。
        /// </summary>
        public async Task<List<AdminUserListItemDto>> GetListAsync(string? keyword = null, bool? isActive = null)
        {
            var normalizedKeyword = (keyword ?? string.Empty).Trim();
            var query = _adminUserRepository.Db.Queryable<AdminUserEntity>()
                .Where(admin => !admin.IsDeleted);

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(admin =>
                    admin.Account.Contains(normalizedKeyword) ||
                    admin.DisplayName.Contains(normalizedKeyword) ||
                    admin.Role.Contains(normalizedKeyword));
            }

            query = query.WhereIF(isActive.HasValue, admin => admin.IsActive == isActive!.Value);

            var users = await query.OrderBy(admin => admin.Account).ToListAsync();
            return users.Select(user => new AdminUserListItemDto
            {
                AdminId = user.AdminId,
                Account = user.Account,
                DisplayName = user.DisplayName,
                Role = user.Role,
                IsActive = user.IsActive
            }).ToList();
        }

        /// <summary>
        /// 获取管理员详情。
        /// </summary>
        public async Task<AdminUserDetailDto?> GetDetailAsync(string adminId)
        {
            if (string.IsNullOrWhiteSpace(adminId))
            {
                return null;
            }

            var user = await _adminUserRepository.GetByIdAsync(adminId.Trim());
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            return MapDetail(user);
        }

        /// <summary>
        /// 创建管理员。
        /// </summary>
        public async Task<AdminUserDetailDto> CreateAsync(AdminCreateUserRequestDto request)
        {
            var account = (request.Account ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(account))
            {
                throw new InvalidOperationException("管理员账号不能为空。");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new InvalidOperationException("管理员密码不能为空。");
            }

            var exists = await _adminUserRepository.ExistsAsync(admin => admin.Account == account && !admin.IsDeleted);
            if (exists)
            {
                throw new InvalidOperationException("管理员账号已存在。");
            }

            var user = new AdminUserEntity
            {
                AdminId = Guid.NewGuid().ToString("N"),
                Account = account,
                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? account : request.DisplayName.Trim(),
                Role = string.IsNullOrWhiteSpace(request.Role) ? "admin" : request.Role.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, BCrypt.Net.BCrypt.GenerateSalt(12)),
                IsActive = true,
                IsDeleted = false,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };

            await _adminUserRepository.AddAsync(user);
            return MapDetail(user);
        }

        /// <summary>
        /// 更新管理员。
        /// </summary>
        public async Task<AdminUserDetailDto> UpdateAsync(string adminId, AdminUpdateUserRequestDto request)
        {
            var user = await _adminUserRepository.GetByIdAsync(adminId);
            if (user == null || user.IsDeleted)
            {
                throw new InvalidOperationException("管理员不存在。");
            }

            user.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? user.DisplayName : request.DisplayName.Trim();
            user.Role = string.IsNullOrWhiteSpace(request.Role) ? user.Role : request.Role.Trim();
            user.IsActive = request.IsActive;
            user.LastUpdateTime = DateTime.Now;
            await _adminUserRepository.UpdateAsync(user);
            return MapDetail(user);
        }

        /// <summary>
        /// 重置管理员密码。
        /// </summary>
        public async Task<bool> ResetPasswordAsync(string adminId, AdminResetPasswordRequestDto request)
        {
            var user = await _adminUserRepository.GetByIdAsync(adminId);
            if (user == null || user.IsDeleted)
            {
                throw new InvalidOperationException("管理员不存在。");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new InvalidOperationException("新密码不能为空。");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
            user.LastUpdateTime = DateTime.Now;
            await _adminUserRepository.UpdateAsync(user);
            return true;
        }

        private static AdminUserDetailDto MapDetail(AdminUserEntity user)
        {
            return new AdminUserDetailDto
            {
                AdminId = user.AdminId,
                Account = user.Account,
                DisplayName = user.DisplayName,
                Role = user.Role,
                IsActive = user.IsActive,
                LastLoginTime = user.LastLoginTime
            };
        }
    }
}
