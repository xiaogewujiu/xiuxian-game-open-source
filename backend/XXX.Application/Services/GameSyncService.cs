using Microsoft.Extensions.Logging;
using XXX.Application.Interfaces;

namespace XXX.Application.Services
{
    /// <summary>
    /// 游戏数据同步服务。
    /// 这里保留旧同步接口以兼容非排行榜调用方；排行榜当前由查询服务从真实业务表构建。
    /// </summary>
    public class GameSyncService : IGameSyncService
    {
        private readonly ILogger<GameSyncService> _logger;

        /// <summary>
        /// 初始化游戏数据同步服务。
        /// </summary>
        /// <param name="logger">日志记录器。</param>
        public GameSyncService(ILogger<GameSyncService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 兼容旧调用：当前不执行排行榜副作用。
        /// </summary>
        public Task SyncPlayerAsync(string playerId)
        {
            return Task.CompletedTask;
        }


    }
}
