using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SqlSugar;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using XXX.Entity;
using XXX.Infrastructure.Data;
namespace XXX.Infrastructure.Authentication
{
    // 使用 JWT 进行玩家与管理员的身份认证。
    // 这个服务负责访问令牌生成、刷新令牌生成、令牌校验、黑名单管理与过期清理。
    public class JwtService : IJwtService

    {
        // 运行时读取的 JWT 配置选项。
        private readonly JwtOptions _options;

        // JWT 令牌解析与写入工具。
        private readonly JwtSecurityTokenHandler _tokenHandler;

        // HMAC 签名用的对称密钥。
        private readonly SymmetricSecurityKey _securityKey;

        // 访问令牌校验规则集合。
        private readonly TokenValidationParameters _validationParameters;

        // 用于持久化刷新令牌和黑名单的数据库客户端。
        private readonly ISqlSugarClient _db;

        // 用于记录令牌生成、刷新和清理的日志。
        private readonly ILogger<JwtService> _logger;

        // 构造 JWT 服务。
        // 启动时会立即校验配置是否完整，并预先构建好校验器与签名密钥。
        public JwtService(

            IOptions<JwtOptions> options,

            DbContext dbContext,

            ILogger<JwtService> logger)

        {
            _options = options.Value;

            _options.Validate();

            _db = dbContext.Db;

            _logger = logger;

            _tokenHandler = new JwtSecurityTokenHandler();

            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

            _validationParameters = new TokenValidationParameters

            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer = _options.Issuer,

                ValidAudience = _options.Audience,

                IssuerSigningKey = _securityKey,

                ClockSkew = TimeSpan.Zero

            };

        }
        // 统一生成访问令牌和刷新令牌。
        // 除了返回字符串，还会把刷新令牌落库，便于后续撤销与过期管理。
        public TokenModel GenerateTokens(string userId, string userName, string role = "user", IEnumerable<string>? permissions = null)

        {
            var accessToken = GenerateAccessToken(userId, userName, role, permissions);

            var refreshToken = GenerateRefreshToken();

            var expiresAt = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays);

            var tokenEntity = new RefreshTokenEntity

            {
                GID = Guid.NewGuid().ToString("N"),

                UserId = userId,

                Token = refreshToken,

                ExpiresAt = expiresAt,

                CreateTime = DateTime.UtcNow,

                IsRevoked = false

            };

            _db.Insertable(tokenEntity).ExecuteCommand();

            return new TokenModel

            {
                AccessToken = accessToken,

                RefreshToken = refreshToken,

                TokenType = "Bearer",

                ExpiresIn = _options.AccessTokenExpirationMinutes * 60,

                IssuedAt = DateTime.UtcNow,

                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes)

            };

        }
        // 生成签名后的访问 JWT 令牌。
        // 这里同时写入标准 claim 和历史自定义 claim，保证新旧接口都能读到同一套身份信息。
        public string GenerateAccessToken(string userId, string userName, string role = "user", IEnumerable<string>? permissions = null)

        {
            // 中文注释：
            // 同时写入“标准声明 + 历史自定义声明”，避免新旧接口读取 claim 的口径不一致。
            var baseClaims = new List<Claim>

            {
                new Claim(ClaimTypes.NameIdentifier, userId),

                new Claim(ClaimTypes.Name, userName),

                new Claim(ClaimTypes.Role, role),

                new Claim(JwtClaims.UserId, userId),

                new Claim(JwtClaims.UserName, userName),

                new Claim(JwtClaims.Role, role),

                new Claim(JwtClaims.Jti, Guid.NewGuid().ToString()),

                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)

            };

            if (permissions != null)

            {
                foreach (var permission in permissions

                    .Where(permission => !string.IsNullOrWhiteSpace(permission))

                    .Distinct(StringComparer.OrdinalIgnoreCase))

                {
                    baseClaims.Add(new Claim(JwtClaims.Permission, permission.Trim()));

                }
            }
            var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(

                issuer: _options.Issuer,

                audience: _options.Audience,

                claims: baseClaims,

                notBefore: DateTime.UtcNow,

                expires: DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes),

                signingCredentials: credentials

            );

            return _tokenHandler.WriteToken(token);

        }
        // 生成高强度随机刷新令牌。
        public string GenerateRefreshToken()

        {
            var randomBytes = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);

            }
            return Convert.ToBase64String(randomBytes);

        }
        // 校验访问令牌并解析 ClaimsPrincipal。
        public ClaimsPrincipal? ValidateAccessToken(string token)

        {
            try

            {
                var principal = _tokenHandler.ValidateToken(token, _validationParameters, out _);

                return principal;

            }
            catch

            {
                return null;

            }
        }
        // 校验刷新令牌是否仍然可用。
        public bool ValidateRefreshToken(string refreshToken)

        {
            var tokenEntity = _db.Queryable<RefreshTokenEntity>()

                .Where(t => t.Token == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)

                .First();

            return tokenEntity != null;

        }
        // 使用旧访问令牌和旧刷新令牌换发新的令牌对。
        // 换发成功后会先撤销旧刷新令牌，避免被重复使用。
        public TokenModel? RefreshTokens(string accessToken, string refreshToken)

        {
            var tokenEntity = _db.Queryable<RefreshTokenEntity>()

                .Where(t => t.Token == refreshToken)

                .First();

            if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAt <= DateTime.UtcNow)

            {
                return null;

            }
            string userName = "user";

            string role = "user";

            var permissions = new List<string>();

            try

            {
                // 中文注释：
                // 刷新链路这里只需要继承旧访问令牌里的用户标识、角色和权限，
                // 不依赖 Claim 映射后的名称，直接读 JWT 原始 claim 更稳定。
                var jwtToken = _tokenHandler.ReadJwtToken(accessToken);

                userName = jwtToken.Claims.FirstOrDefault(claim =>

                    string.Equals(claim.Type, JwtClaims.UserName, StringComparison.OrdinalIgnoreCase) ||

                    string.Equals(claim.Type, ClaimTypes.Name, StringComparison.OrdinalIgnoreCase))?.Value ?? userName;

                role = jwtToken.Claims.FirstOrDefault(claim =>

                    string.Equals(claim.Type, JwtClaims.Role, StringComparison.OrdinalIgnoreCase) ||

                    string.Equals(claim.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))?.Value ?? role;

                permissions = jwtToken.Claims

                    .Where(claim => string.Equals(claim.Type, JwtClaims.Permission, StringComparison.OrdinalIgnoreCase))

                    .Select(claim => claim.Value)

                    .Where(value => !string.IsNullOrWhiteSpace(value))

                    .Distinct(StringComparer.OrdinalIgnoreCase)

                    .ToList();

            }
            catch

            {
            }
            tokenEntity.IsRevoked = true;

            tokenEntity.RevokedAt = DateTime.UtcNow;

            _db.Updateable(tokenEntity).ExecuteCommand();

            return GenerateTokens(tokenEntity.UserId, userName, role, permissions);

        }
        // 从令牌中提取用户编号。
        public string? GetUserIdFromToken(string token)

        {
            var principal = ValidateAccessToken(token);

            return principal?.FindFirst(JwtClaims.UserId)?.Value;

        }
        // 从令牌中提取用户名称。
        public string? GetUserNameFromToken(string token)

        {
            var principal = ValidateAccessToken(token);

            return principal?.FindFirst(JwtClaims.UserName)?.Value;

        }
        // 读取令牌的过期时间。
        public DateTime? GetExpirationDate(string token)

        {
            try

            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);

                return jwtToken.ValidTo;

            }
            catch

            {
                return null;

            }
        }
        // 撤销单个刷新令牌。
        public async Task RevokeRefreshTokenAsync(string refreshToken)

        {
            var tokenEntity = await _db.Queryable<RefreshTokenEntity>()

                .Where(t => t.Token == refreshToken)

                .FirstAsync();

            if (tokenEntity != null)

            {
                tokenEntity.IsRevoked = true;

                tokenEntity.RevokedAt = DateTime.UtcNow;

                await _db.Updateable(tokenEntity).ExecuteCommandAsync();

            }
        }
        // 撤销指定用户的全部刷新令牌。
        public async Task RevokeAllUserTokensAsync(string userId)

        {
            await _db.Updateable<RefreshTokenEntity>()

                .SetColumns(t => t.IsRevoked == true)

                .SetColumns(t => t.RevokedAt == DateTime.UtcNow)

                .Where(t => t.UserId == userId && !t.IsRevoked)

                .ExecuteCommandAsync();

        }
        // 把访问令牌加入黑名单。
        public async Task AddToBlacklistAsync(string accessToken, string userId, DateTime expiresAt, string? reason = null)

        {
            var blacklistEntity = new TokenBlacklistEntity

            {
                GID = Guid.NewGuid().ToString("N"),

                AccessToken = accessToken,

                UserId = userId,

                ExpiresAt = expiresAt,

                CreateTime = DateTime.UtcNow,

                Reason = reason ?? "Logout"

            };

            await _db.Insertable(blacklistEntity).ExecuteCommandAsync();

        }
        // 检查访问令牌是否已被加入黑名单。
        public async Task<bool> IsTokenBlacklistedAsync(string accessToken)

        {
            return await _db.Queryable<TokenBlacklistEntity>()

                .Where(t => t.AccessToken == accessToken && t.ExpiresAt > DateTime.UtcNow)

                .AnyAsync();

        }
        // 清理过期的刷新令牌和黑名单记录。
        public async Task CleanupExpiredTokensAsync()

        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7);

            await _db.Deleteable<RefreshTokenEntity>()

                .Where(t => t.ExpiresAt < cutoffDate || (t.IsRevoked && t.RevokedAt < cutoffDate))

                .ExecuteCommandAsync();

            await _db.Deleteable<TokenBlacklistEntity>()

                .Where(t => t.ExpiresAt < cutoffDate)

                .ExecuteCommandAsync();

            _logger.LogInformation("Cleaned up expired tokens older than {CutoffDate}", cutoffDate);

        }
    }
}
