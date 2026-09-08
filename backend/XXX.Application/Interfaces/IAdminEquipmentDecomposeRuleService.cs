using XXX.Application.DTOs;

namespace XXX.Application.Interfaces;

public interface IAdminEquipmentDecomposeRuleService
{
    Task<List<AdminEquipmentDecomposeRuleDto>> GetRulesAsync();
    Task SaveRuleAsync(AdminEquipmentDecomposeRuleDto dto);
    Task DeleteRuleAsync(long gid);
}
