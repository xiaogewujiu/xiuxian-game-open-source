using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台奖励配置服务接口。
    /// </summary>
    public interface IAdminRewardConfigService
    {
        /// <summary>
        /// 获取签到奖励配置列表。
        /// </summary>
        Task<List<AdminCheckInRewardConfigDto>> GetCheckInConfigsAsync(bool? isMilestone = null);

        /// <summary>
        /// 保存签到奖励配置。
        /// </summary>
        Task<AdminCheckInRewardConfigDto> SaveCheckInConfigAsync(AdminCheckInRewardConfigDto request);

        /// <summary>
        /// 删除签到奖励配置。
        /// </summary>
        Task<bool> DeleteCheckInConfigAsync(int continuousDay);

        /// <summary>
        /// 获取兑换码配置列表。
        /// </summary>
        Task<List<AdminRedeemCodeConfigDto>> GetRedeemCodeConfigsAsync(string? keyword = null, bool? isEnabled = null);

        /// <summary>
        /// 保存兑换码配置。
        /// </summary>
        Task<AdminRedeemCodeConfigDto> SaveRedeemCodeConfigAsync(AdminRedeemCodeConfigDto request);

        /// <summary>
        /// 删除兑换码配置。
        /// </summary>
        Task<bool> DeleteRedeemCodeConfigAsync(string code);

        /// <summary>
        /// 重新加载当前内置活动奖励配置。
        /// </summary>
        Task ReloadBuiltInActivityConfigsAsync(string? operatorName = null);
    }
}
