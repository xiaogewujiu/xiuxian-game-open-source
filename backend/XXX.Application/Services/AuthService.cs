using XXX.Application.DTOs;
using XXX.Application.Interfaces;
using XXX.Entity;
using XXX.Infrastructure.Authentication;
using XXX.Infrastructure.Repositories;
using XXX.Player;

namespace XXX.Application.Services
{
    /// <summary>
    /// 认证服务实现。
    /// </summary>
    public class AuthService : IAuthService
    {
        /// <summary>
        /// 玩家仓储。
        /// </summary>
        private readonly IRepository<UserEntity> _userRepository;

        /// <summary>
        /// JWT 令牌服务。
        /// 负责签发、刷新、撤销与黑名单管理。
        /// </summary>
        private readonly IJwtService _jwtService;

        /// <summary>
        /// 玩家核心服务。
        /// 登录和注册完成后会用它读取最新玩家快照。
        /// </summary>
        private readonly IPlayerService _playerService;

        /// <summary>
        /// 游戏同步服务。
        /// 新注册角色会同步进运行态缓存。
        /// </summary>
        private readonly IGameSyncService _gameSyncService;

        /// <summary>
        /// 新手礼包服务。
        /// 注册成功后会按配置自动发放礼包。
        /// </summary>
        private readonly IStarterPackageService _starterPackageService;

        /// <summary>
        /// 初始化认证服务。
        /// </summary>
        /// <param name="userRepository">玩家仓储。</param>
        /// <param name="jwtService">JWT 令牌服务。</param>
        /// <param name="playerService">玩家核心服务。</param>
        /// <param name="gameSyncService">游戏同步服务。</param>
        /// <param name="starterPackageService">新手礼包服务。</param>
        public AuthService(
            IRepository<UserEntity> userRepository,
            IJwtService jwtService,
            IPlayerService playerService,
            IGameSyncService gameSyncService,
            IStarterPackageService starterPackageService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _playerService = playerService;
            _gameSyncService = gameSyncService;
            _starterPackageService = starterPackageService;
        }

        /// <summary>
        /// 处理账号登录。
        /// </summary>
        /// <param name="request">登录请求。</param>
        /// <returns>登录响应。</returns>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var normalizedAccount = NormalizeAccount(request.Account);
            if (string.IsNullOrEmpty(normalizedAccount))
            {
                throw new InvalidOperationException("账号不能为空");
            }

            var user = await _userRepository.GetFirstAsync(u => u.Account == normalizedAccount && !u.IsDeleted);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("账号或密码错误");
            }

            // 中文注释：
            // 玩家被后台封禁后，即使账号密码正确，也不允许继续登录游戏。
            // 这样 GM 的封禁操作会直接收口在登录入口，避免封禁状态只停留在数据库里却不生效。
            if (user.IsBanned && (!user.BanExpiresAt.HasValue || user.BanExpiresAt.Value > DateTime.Now))
            {
                var reason = string.IsNullOrWhiteSpace(user.BanReason)
                    ? "当前账号已被封禁。"
                    : $"当前账号已被封禁：{user.BanReason}";
                throw new InvalidOperationException(reason);
            }

            await _playerService.UpdateLastLoginTimeAsync(user.GID);

            var tokenModel = _jwtService.GenerateTokens(user.GID, user.Name);
            var player = await _playerService.GetByIdAsync(user.GID);

            return new LoginResponseDto
            {
                AccessToken = tokenModel.AccessToken,
                RefreshToken = tokenModel.RefreshToken,
                TokenType = tokenModel.TokenType,
                ExpiresIn = tokenModel.ExpiresIn,
                Player = player
            };
        }

        /// <summary>
        /// 处理账号注册。
        /// </summary>
        /// <param name="request">注册请求。</param>
        /// <returns>注册完成后的登录响应。</returns>
        public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var normalizedAccount = NormalizeAccount(request.Account);
            if (string.IsNullOrEmpty(normalizedAccount))
            {
                throw new InvalidOperationException("账号不能为空");
            }

            ValidatePasswordStrength(request.Password);

            if (request.Password != request.ConfirmPassword)
            {
                throw new InvalidOperationException("两次输入的密码不一致");
            }

            if (await IsAccountExistsAsync(normalizedAccount))
            {
                throw new InvalidOperationException("该账号已被注册");
            }

            if (!PlayerProfessionCatalog.IsPlayable(request.Profession))
            {
                throw new InvalidOperationException("请选择有效职业。");
            }

            var playerId = Guid.NewGuid().ToString("N");
            var playerName = string.IsNullOrWhiteSpace(request.Name) ? request.Account : request.Name.Trim();

            // 中文注释：直接使用核心层的玩家初始化逻辑，保证新角色注册后就具备完整基础属性。
            var user = PlayerManager.CreateNewPlayer(playerId, playerName, request.Profession);
            user.Account = normalizedAccount;
            user.PasswordHash = HashPassword(request.Password);
            user.LastUpdateTime = DateTime.Now;

            await _userRepository.AddAsync(user);
            await _starterPackageService.ApplyOnRegisterAsync(user);

            await _gameSyncService.SyncPlayerAsync(user.GID);

            var tokenModel = _jwtService.GenerateTokens(user.GID, user.Name);
            var player = await _playerService.GetByIdAsync(user.GID);

            return new LoginResponseDto
            {
                AccessToken = tokenModel.AccessToken,
                RefreshToken = tokenModel.RefreshToken,
                TokenType = tokenModel.TokenType,
                ExpiresIn = tokenModel.ExpiresIn,
                Player = player
            };
        }

        /// <summary>
        /// 刷新登录令牌。
        /// </summary>
        /// <param name="request">刷新令牌请求。</param>
        /// <returns>新的登录响应。</returns>
        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var tokenModel = _jwtService.RefreshTokens(request.AccessToken, request.RefreshToken);
            if (tokenModel == null)
            {
                throw new InvalidOperationException("刷新令牌无效或已过期");
            }

            var userId = _jwtService.GetUserIdFromToken(tokenModel.AccessToken);
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("令牌无效");
            }

            var player = await _playerService.GetByIdAsync(userId);

            return new LoginResponseDto
            {
                AccessToken = tokenModel.AccessToken,
                RefreshToken = tokenModel.RefreshToken,
                TokenType = tokenModel.TokenType,
                ExpiresIn = tokenModel.ExpiresIn,
                Player = player
            };
        }

        /// <summary>
        /// 注销当前登录会话。
        /// </summary>
        /// <param name="userId">玩家编号。</param>
        /// <param name="accessToken">当前访问令牌。</param>
        /// <param name="refreshToken">当前刷新令牌。</param>
        /// <returns>登出执行结果。</returns>
        public async Task<bool> LogoutAsync(string userId, string? accessToken = null, string? refreshToken = null)
        {
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _jwtService.RevokeRefreshTokenAsync(refreshToken);
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                var expiresAt = _jwtService.GetExpirationDate(accessToken);
                if (expiresAt.HasValue && expiresAt.Value > DateTime.UtcNow)
                {
                    await _jwtService.AddToBlacklistAsync(accessToken, userId, expiresAt.Value, "登出");
                }
            }

            return true;
        }

        /// <summary>
        /// 检查账号是否已存在。
        /// </summary>
        /// <param name="account">待检查账号。</param>
        /// <returns>账号已存在返回真。</returns>
        public async Task<bool> IsAccountExistsAsync(string account)
        {
            var normalizedAccount = NormalizeAccount(account);
            if (string.IsNullOrEmpty(normalizedAccount))
            {
                return false;
            }

            return await _userRepository.ExistsAsync(u => u.Account == normalizedAccount && !u.IsDeleted);
        }

        /// <summary>
        /// 对明文密码进行 BCrypt 哈希。
        /// </summary>
        /// <param name="password">明文密码。</param>
        /// <returns>密码哈希结果。</returns>
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        /// <summary>
        /// 校验明文密码与哈希是否匹配。
        /// </summary>
        /// <param name="password">明文密码。</param>
        /// <param name="hash">数据库中的密码哈希。</param>
        /// <returns>匹配返回真。</returns>
        private static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        /// <summary>
        /// 校验注册密码强度。
        /// 当前要求为 8 到 32 位，且必须同时包含大写字母、小写字母和数字。
        /// </summary>
        /// <param name="password">待校验密码。</param>
        private static void ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("密码不能为空");
            }

            if (password.Length < 8)
            {
                throw new InvalidOperationException("密码长度至少 8 位");
            }

            if (password.Length > 32)
            {
                throw new InvalidOperationException("密码长度不能超过 32 位");
            }

            var hasUpper = password.Any(char.IsUpper);
            var hasLower = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);

            if (!hasUpper || !hasLower || !hasDigit)
            {
                throw new InvalidOperationException("密码必须包含大写字母、小写字母和数字");
            }
        }

        /// <summary>
        /// 统一标准化账号输入，避免前后空白导致“看起来同一个账号，实际查不到/重复注册”的问题。
        /// </summary>
        /// <param name="account">原始账号输入。</param>
        /// <returns>去除首尾空白后的账号文本。</returns>
        private static string NormalizeAccount(string? account) => account?.Trim() ?? string.Empty;
    }
}
