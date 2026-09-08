using XXX.Application.DTOs;
using XXX.Entity;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 管理后台技能模板服务接口。
    /// </summary>
    public interface IAdminSkillService
    {
        /// <summary>
        /// 获取技能模板列表。
        /// </summary>
        Task<List<AdminSkillListItemDto>> GetListAsync(string? keyword = null, string? catalog = null);

        /// <summary>
        /// 获取技能模板详情。
        /// </summary>
        Task<AdminSkillDetailDto?> GetDetailAsync(int skillId);

        /// <summary>
        /// 获取指定技能的全部后续等级。
        /// </summary>
        Task<List<AdminSkillNextLevelDto>> GetNextLevelsAsync(int skillId);

        /// <summary>
        /// 保存技能模板。
        /// </summary>
        Task<AdminSkillDetailDto> SaveAsync(AdminSkillDetailDto request);

        /// <summary>
        /// 复制当前技能创建下一级技能。
        /// </summary>
        Task<AdminSkillDetailDto> CopyNextAsync(int skillId);

        /// <summary>
        /// 将已有独立技能关联为下一级。
        /// </summary>
        Task<AdminSkillDetailDto> LinkNextAsync(int skillId, int nextSkillId, List<SkillUpgradeCondition> conditions);

        /// <summary>
        /// 更新当前技能升级到下一级的条件。
        /// </summary>
        Task<AdminSkillDetailDto> UpdateUpgradeConditionsAsync(int skillId, List<SkillUpgradeCondition> conditions);

        /// <summary>
        /// 删除当前技能及其后续技能。
        /// </summary>
        Task DeleteNextChainAsync(int skillId);

        /// <summary>
        /// 删除技能模板。
        /// </summary>
        Task<bool> DeleteAsync(int skillId);
    }
}
