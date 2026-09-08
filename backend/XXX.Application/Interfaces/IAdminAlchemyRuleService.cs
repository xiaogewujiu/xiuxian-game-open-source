using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 炼丹职业规则后台服务。
    /// </summary>
    public interface IAdminAlchemyRuleService
    {
        /// <summary>
        /// 获取炼丹职业等级规则列表。
        /// </summary>
        /// <returns>等级规则列表。</returns>
        Task<List<AdminAlchemyProfessionLevelRuleDto>> GetLevelRulesAsync();

        /// <summary>
        /// 获取指定等级的炼丹职业规则。
        /// </summary>
        /// <param name="level">职业等级。</param>
        /// <returns>等级规则详情；不存在时返回空。</returns>
        Task<AdminAlchemyProfessionLevelRuleDto?> GetLevelRuleAsync(int level);

        /// <summary>
        /// 保存指定等级的炼丹职业规则。
        /// </summary>
        /// <param name="request">等级规则请求。</param>
        /// <returns>保存后的等级规则。</returns>
        Task<AdminAlchemyProfessionLevelRuleDto> SaveLevelRuleAsync(AdminAlchemyProfessionLevelRuleDto request);

        /// <summary>
        /// 删除指定等级的炼丹职业规则。
        /// </summary>
        /// <param name="level">职业等级。</param>
        /// <returns>删除成功返回真。</returns>
        Task<bool> DeleteLevelRuleAsync(int level);

        /// <summary>
        /// 获取炼丹职业通用规则。
        /// </summary>
        /// <returns>通用规则详情；不存在时返回空。</returns>
        Task<AdminAlchemyProfessionRuleDto?> GetRuleAsync();

        /// <summary>
        /// 保存炼丹职业通用规则。
        /// </summary>
        /// <param name="request">通用规则请求。</param>
        /// <returns>保存后的通用规则。</returns>
        Task<AdminAlchemyProfessionRuleDto> SaveRuleAsync(AdminAlchemyProfessionRuleDto request);
    }
}
