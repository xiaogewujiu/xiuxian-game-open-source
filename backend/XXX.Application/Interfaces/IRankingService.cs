using XXX.Ranking;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 排行榜服务接口
    /// 提供排行榜配置管理、排名查询、分数更新、赛季管理等排行榜系统功能
    /// </summary>
    /// <remarks>
    /// 排行榜系统是游戏的竞争核心，提供：
    /// - 多类型排行榜（等级、战力、竞技场、成就、财富）
    /// - 实时排名更新
    /// - 赛季机制
    /// - 排名奖励
    /// </remarks>
    public interface IRankingService
    {
        /// <summary>
        /// 初始化排行榜系统
        /// </summary>
        /// <remarks>
        /// 初始化流程：
        /// 1. 从数据库加载排行榜配置
        /// 2. 如果没有配置，加载默认排行榜
        /// 3. 初始化刷新时间记录
        /// </remarks>
        Task InitializeAsync();

        /// <summary>
        /// 重新从数据库装载排行榜配置缓存。
        /// </summary>
        Task ReloadCacheAsync();

        /// <summary>
        /// 获取所有排行榜配置
        /// </summary>
        /// <returns>排行榜配置列表，按排序顺序排列</returns>
        Task<List<RankingConfig>> GetAllRankingsAsync();

        /// <summary>
        /// 获取指定排行榜配置
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>排行榜配置，如果不存在则返回null</returns>
        Task<RankingConfig?> GetRankingConfigAsync(string rankingId);

        /// <summary>
        /// 获取排行榜完整排名列表
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>排名条目列表，按分数降序排列</returns>
        Task<List<RankingEntry>> GetRankingAsync(string rankingId);

        /// <summary>
        /// 获取排行榜前N名
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="count">获取数量</param>
        /// <returns>排名条目列表</returns>
        Task<List<RankingEntry>> GetTopNAsync(string rankingId, int count);

        /// <summary>
        /// 获取玩家在指定排行榜的排名
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>排名，如果未上榜则返回null</returns>
        Task<int?> GetPlayerRankAsync(string playerId, string rankingId);



        /// <summary>
        /// 检查并处理赛季状态
        /// </summary>
        /// <remarks>
        /// 对启用赛季的排行榜：
        /// - 检查赛季是否结束
        /// - 创建赛季结束快照
        /// - 开始新赛季
        /// </remarks>
        Task CheckSeasonsAsync();

        /// <summary>
        /// 获取排行榜统计信息
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <returns>统计信息</returns>
        /// <remarks>
        /// 统计信息包含：
        /// - 当前条目数
        /// - 最大容量
        /// - 上次刷新时间
        /// - 赛季信息
        /// </remarks>
        Task<RankingStats> GetRankingStatsAsync(string rankingId);

        /// <summary>
        /// 创建排行榜快照
        /// </summary>
        /// <param name="rankingId">排行榜ID</param>
        /// <param name="type">快照类型</param>
        /// <param name="description">快照描述</param>
        /// <returns>快照记录ID，如果创建失败则返回null</returns>
        /// <remarks>
        /// 快照用于保存历史排名数据
        /// 常用于赛季结束、活动结束等场景
        /// </remarks>
        Task<string?> CreateSnapshotAsync(string rankingId, SnapshotType type, string description);
    }
}
