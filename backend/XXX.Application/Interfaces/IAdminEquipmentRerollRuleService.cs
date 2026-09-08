using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 装备洗练规则管理服务。
    /// </summary>
    public interface IAdminEquipmentRerollRuleService
    {
        Task<List<AdminEquipmentEnhanceRuleDto>> GetEnhanceRulesAsync();
        Task SaveEnhanceRuleAsync(AdminEquipmentEnhanceRuleDto dto);
        Task DeleteEnhanceRuleAsync(long gid);
        Task<List<AdminEquipmentRerollCostRuleDto>> GetRerollCostRulesAsync();
        Task SaveRerollCostRuleAsync(AdminEquipmentRerollCostRuleDto dto);
        Task DeleteRerollCostRuleAsync(long gid);
        Task<AdminEquipmentRerollSystemConfigDto?> GetSystemConfigAsync();
        Task SaveSystemConfigAsync(AdminEquipmentRerollSystemConfigDto dto);
        Task<List<AdminEquipmentRerollSlotPoolConfigDto>> GetSlotPoolConfigsAsync();
        Task SaveSlotPoolConfigAsync(AdminEquipmentRerollSlotPoolConfigDto dto);
        Task DeleteSlotPoolConfigAsync(long gid);
        Task<List<AdminEquipmentRerollTierConfigDto>> GetTierConfigsAsync();
        Task SaveTierConfigAsync(AdminEquipmentRerollTierConfigDto dto);
        Task DeleteTierConfigAsync(long gid);
        Task<List<AdminEquipmentRerollAttributeValueConfigDto>> GetAttributeValueConfigsAsync();
        Task SaveAttributeValueConfigAsync(AdminEquipmentRerollAttributeValueConfigDto dto);
        Task DeleteAttributeValueConfigAsync(long gid);
    }
}
