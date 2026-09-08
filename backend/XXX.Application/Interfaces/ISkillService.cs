using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 技能展示服务接口。
    /// </summary>
    public interface ISkillService
    {
        /// <summary>
        /// 获取玩家技能总览。
        /// </summary>
        Task<SkillOverviewDto> GetSkillOverviewAsync(string playerId);

        /// <summary>
        /// 直接升级当前已掌握技能。
        /// </summary>
        Task<SkillUpgradeResultDto> UpgradeSkillAsync(string playerId, int skillId);

        /// <summary>
        /// 携带一个技能。
        /// </summary>
        Task<SkillOverviewDto> EquipSkillAsync(string playerId, int skillId);

        /// <summary>
        /// 卸下一个技能。
        /// </summary>
        Task<SkillOverviewDto> UnequipSkillAsync(string playerId, int skillId);
    }
}
