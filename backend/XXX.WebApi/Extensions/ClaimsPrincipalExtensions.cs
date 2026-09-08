using System.Security.Claims;
using XXX.Infrastructure.Authentication;

namespace XXX.WebApi.Extensions
{
    /// <summary>
    /// 当前登录用户声明扩展方法。
    /// 作用：统一读取玩家ID，避免各控制器重复实现并出现声明类型不一致问题。
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// 获取当前登录玩家ID。
        /// 关键逻辑：
        /// 1. 优先读取标准JWT声明 ClaimTypes.NameIdentifier（新令牌标准写法）。
        /// 2. 若不存在则回退到历史自定义声明 uid（旧令牌兼容）。
        /// 3. 最终返回 null 表示令牌中不存在可用玩家ID。
        /// </summary>
        /// <param name="user">当前请求用户上下文。</param>
        /// <returns>玩家ID，若未登录或声明缺失则返回 null。</returns>
        public static string? GetCurrentPlayerId(this ClaimsPrincipal? user)
        {
            if (user == null)
            {
                return null;
            }

            var playerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(playerId))
            {
                return playerId;
            }

            // 兼容历史令牌：旧版本仅写入 uid，自定义声明名沿用 JwtClaims.UserId。
            playerId = user.FindFirst(JwtClaims.UserId)?.Value;
            if (!string.IsNullOrWhiteSpace(playerId))
            {
                return playerId;
            }

            return null;
        }
    }
}
