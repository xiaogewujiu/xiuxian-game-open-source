using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 宝石系统服务接口
    /// </summary>
    public interface IGemService
    {
        /// <summary>获取所有宝石模板</summary>
        Task<List<GemTemplateDto>> GetTemplatesAsync();

        /// <summary>获取玩家背包中的宝石</summary>
        Task<List<GemInventoryDto>> GetInventoryGemsAsync(string playerId);

        /// <summary>镶嵌宝石到装备</summary>
        Task SocketGemAsync(string playerId, SocketGemDto dto);

        /// <summary>从装备取下宝石</summary>
        Task UnsocketGemAsync(string playerId, UnsocketGemDto dto);

        /// <summary>宝石合成</summary>
        Task SynthesizeGemAsync(string playerId, SynthesizeGemDto dto);
    }

    /// <summary>
    /// 后台宝石管理服务接口
    /// </summary>
    public interface IAdminGemService
    {
        /// <summary>获取宝石模板列表</summary>
        Task<List<AdminGemListItemDto>> GetListAsync();

        /// <summary>获取宝石模板详情</summary>
        Task<AdminGemDetailDto?> GetDetailAsync(long id);

        /// <summary>创建宝石模板</summary>
        Task CreateAsync(AdminGemSaveDto dto);

        /// <summary>更新宝石模板</summary>
        Task UpdateAsync(long id, AdminGemSaveDto dto);

        /// <summary>删除宝石模板</summary>
        Task DeleteAsync(long id);

        /// <summary>批量生成宝石模板</summary>
        Task<int> BatchGenerateAsync(AdminGemBatchGenerateDto dto);
    }
}
