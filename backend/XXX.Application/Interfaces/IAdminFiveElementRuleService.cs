using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 后台聚灵阵规则配置服务。
    /// </summary>
    public interface IAdminFiveElementRuleService
    {
        /// <summary>
        /// 获取聚灵阵主等级规则列表。
        /// </summary>
        /// <returns>主等级规则列表。</returns>
        Task<List<AdminFiveElementLevelRuleListItemDto>> GetLevelRulesAsync();

        /// <summary>
        /// 获取指定主等级的聚灵阵规则详情。
        /// </summary>
        /// <param name="arrayLevel">聚灵阵主等级。</param>
        /// <returns>主等级规则详情；不存在时返回空。</returns>
        Task<AdminFiveElementLevelRuleDetailDto?> GetLevelRuleDetailAsync(int arrayLevel);

        /// <summary>
        /// 保存聚灵阵主等级规则。
        /// </summary>
        /// <param name="request">主等级规则请求。</param>
        /// <returns>保存后的主等级规则详情。</returns>
        Task<AdminFiveElementLevelRuleDetailDto> SaveLevelRuleAsync(AdminFiveElementLevelRuleDetailDto request);

        /// <summary>
        /// 删除指定主等级的聚灵阵规则。
        /// </summary>
        /// <param name="arrayLevel">聚灵阵主等级。</param>
        /// <returns>删除成功返回真。</returns>
        Task<bool> DeleteLevelRuleAsync(int arrayLevel);

        /// <summary>
        /// 获取启用的五行等级区间规则列表。
        /// </summary>
        Task<List<AdminFiveElementBranchRuleRangeListItemDto>> GetBranchRuleRangesAsync(string? elementType = null);

        /// <summary>
        /// 获取指定区间规则详情。
        /// </summary>
        Task<AdminFiveElementBranchRuleRangeDetailDto?> GetBranchRuleRangeDetailAsync(string gid);

        /// <summary>
        /// 保存五行等级区间规则。
        /// </summary>
        Task<AdminFiveElementBranchRuleRangeDetailDto> SaveBranchRuleRangeAsync(AdminFiveElementBranchRuleRangeDetailDto request);

        /// <summary>
        /// 删除指定五行等级区间规则。
        /// </summary>
        Task<bool> DeleteBranchRuleRangeAsync(string gid);


        /// <summary>
        /// 获取五行分支升级规则列表（旧逐级规则兼容接口）。
        /// 新运行时不使用该接口，保留它是为了兼容现有管理端调用。
        /// </summary>
        /// <param name="elementType">可选的元素类型筛选。</param>
        /// <returns>旧逐级规则列表。</returns>
        Task<List<AdminFiveElementBranchRuleListItemDto>> GetBranchRulesAsync(string? elementType = null);


        /// <param name="gid">分支规则主键。</param>
        /// <returns>分支规则详情；不存在时返回空。</returns>
        Task<AdminFiveElementBranchRuleDetailDto?> GetBranchRuleDetailAsync(string gid);

        /// <summary>
        /// 保存五行分支升级规则。
        /// </summary>
        /// <param name="request">分支规则请求。</param>
        /// <returns>保存后的分支规则详情。</returns>
        Task<AdminFiveElementBranchRuleDetailDto> SaveBranchRuleAsync(AdminFiveElementBranchRuleDetailDto request);

        /// <summary>
        /// 删除指定五行分支升级规则。
        /// </summary>
        /// <param name="gid">分支规则主键。</param>
        /// <returns>删除成功返回真。</returns>
        Task<bool> DeleteBranchRuleAsync(string gid);
    }
}
