using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 灵田规则后台服务。
    /// </summary>
    public interface IAdminSpiritFieldRuleService
    {
        /// <summary>
        /// 获取灵田系统总规则。
        /// </summary>
        /// <returns>系统总规则；不存在时返回空。</returns>
        Task<AdminSpiritFieldSystemRuleDto?> GetSystemRuleAsync();

        /// <summary>
        /// 保存灵田系统总规则。
        /// </summary>
        /// <param name="request">系统总规则请求。</param>
        /// <returns>保存后的系统总规则。</returns>
        Task<AdminSpiritFieldSystemRuleDto> SaveSystemRuleAsync(AdminSpiritFieldSystemRuleDto request);

        /// <summary>
        /// 获取全部加速道具规则。
        /// </summary>
        /// <returns>加速道具规则列表。</returns>
        Task<List<AdminSpiritFieldSpeedUpItemRuleDto>> GetSpeedUpRulesAsync();

        /// <summary>
        /// 获取指定加速道具规则详情。
        /// </summary>
        /// <param name="itemId">道具编号。</param>
        /// <returns>加速道具规则详情；不存在时返回空。</returns>
        Task<AdminSpiritFieldSpeedUpItemRuleDto?> GetSpeedUpRuleAsync(string itemId);

        /// <summary>
        /// 保存加速道具规则。
        /// </summary>
        /// <param name="request">加速道具规则请求。</param>
        /// <returns>保存后的加速道具规则。</returns>
        Task<AdminSpiritFieldSpeedUpItemRuleDto> SaveSpeedUpRuleAsync(AdminSpiritFieldSpeedUpItemRuleDto request);

        /// <summary>
        /// 删除指定加速道具规则。
        /// </summary>
        /// <param name="itemId">道具编号。</param>
        /// <returns>删除成功返回真。</returns>
        Task<bool> DeleteSpeedUpRuleAsync(string itemId);
    }
}
