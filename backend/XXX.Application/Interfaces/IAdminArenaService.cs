using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台竞技场服务接口。
    /// </summary>
    public interface IAdminArenaService
    {
        /// <summary>
        /// 获取竞技场总览。
        /// </summary>
        Task<AdminArenaOverviewDto> GetOverviewAsync();

        /// <summary>
        /// 获取竞技场玩家列表。
        /// </summary>
        Task<List<AdminArenaPlayerDto>> GetPlayersAsync(string? keyword = null, int take = 200);

        /// <summary>
        /// 获取对战日志。
        /// </summary>
        Task<List<AdminArenaBattleLogDto>> GetBattleLogsAsync(string? playerId = null, int take = 100);

        /// <summary>
        /// 调整玩家积分。
        /// </summary>
        Task<bool> AdjustPointsAsync(AdminAdjustPointsDto dto);

        /// <summary>
        /// 禁赛玩家。
        /// </summary>
        Task<bool> BanPlayerAsync(AdminBanPlayerDto dto);

        /// <summary>
        /// 解禁玩家。
        /// </summary>
        Task<bool> UnbanPlayerAsync(AdminUnbanPlayerDto dto);

        /// <summary>
        /// 获取赛季信息。
        /// </summary>
        Task<AdminArenaSeasonDto> GetSeasonInfoAsync();

        /// <summary>
        /// 手动结算赛季（发放奖励并开始新赛季）。
        /// </summary>
        Task<AdminSettleSeasonResultDto> SettleSeasonAsync();

        /// <summary>
        /// 重置赛季（清空所有玩家数据，从赛季1开始）。
        /// </summary>
        Task<bool> ResetSeasonAsync();

        /// <summary>
        /// 调整赛季时长和开关。
        /// </summary>
        Task<bool> AdjustSeasonAsync(AdminAdjustSeasonDto dto);

        /// <summary>
        /// 获取赛季奖励列表。
        /// </summary>
        Task<List<AdminArenaSeasonRewardDto>> GetSeasonRewardsAsync();

        /// <summary>
        /// 保存赛季奖励（新增或更新）。
        /// </summary>
        Task<bool> SaveSeasonRewardAsync(AdminSaveSeasonRewardDto dto);

        /// <summary>
        /// 删除赛季奖励。
        /// </summary>
        Task<bool> DeleteSeasonRewardAsync(string gid);
    }
}
