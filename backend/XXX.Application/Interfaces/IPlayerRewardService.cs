using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 中文注释：
    /// 玩家奖励发放服务接口。
    /// 这层只负责把奖励真正写到人物/背包数据里，不负责决定“什么时候发、因为什么发”，
    /// 这样签到、兑换码、后续活动奖励都可以复用同一套落库逻辑。
    /// </summary>
    public interface IPlayerRewardService
    {
        /// <summary>
        /// 发放一组奖励。
        /// </summary>
        /// <remarks>
        /// 调用方需要自行决定是否包事务。
        /// 这里故意不在服务内部自动开启事务，是为了让签到、兑换码这类“状态变更 + 发奖”流程可以共用同一笔事务。
        /// </remarks>
        Task<List<RewardItemDto>> GrantRewardsAsync(string playerId, IEnumerable<RewardGrantItemDto> rewards, string source);
    }
}
