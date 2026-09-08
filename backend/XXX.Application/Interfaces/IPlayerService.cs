using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 玩家核心服务接口。
    /// 负责角色基础资料、经验成长、属性点、突破与基础货币变更等主链路。
    /// </summary>
    /// <remarks>
    /// 当前实现的几个关键点：
    /// 1. 读取玩家信息时直接查数据库，避免旧缓存让前端看到过期面板。
    /// 2. 等级、属性点、突破状态都会在这里统一组装成前端可直接消费的结构。
    /// 3. 加经验、加点、突破后会触发属性重算或同步，确保人物面板与真实战斗底板一致。
    /// </remarks>
    public interface IPlayerService
    {
        /// <summary>
        /// 根据玩家编号获取玩家基础信息。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <returns>玩家基础信息；玩家不存在或已删除时返回空。</returns>
        Task<PlayerDto?> GetByIdAsync(string playerId);

        /// <summary>
        /// 根据账号获取玩家基础信息。
        /// </summary>
        /// <param name="account">玩家登录账号。</param>
        /// <returns>玩家基础信息；账号不存在时返回空。</returns>
        Task<PlayerDto?> GetByAccountAsync(string account);

        /// <summary>
        /// 获取玩家详细信息。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <returns>玩家详细信息；不存在时返回空。</returns>
        Task<PlayerDetailDto?> GetDetailAsync(string playerId);

        /// <summary>
        /// 创建新玩家。
        /// </summary>
        /// <param name="request">创建玩家请求。</param>
        /// <returns>创建后的玩家基础信息。</returns>
        Task<PlayerDto> CreateAsync(CreatePlayerRequestDto request);

        /// <summary>
        /// 更新玩家可编辑信息。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <param name="request">更新请求。</param>
        /// <returns>更新后的玩家信息；不存在时返回空。</returns>
        Task<PlayerDto?> UpdateAsync(string playerId, UpdatePlayerRequestDto request);

        Task<EquipmentAutoSellSettingsDto?> GetEquipmentAutoSellSettingsAsync(string playerId);

        Task<EquipmentAutoSellSettingsDto?> UpdateEquipmentAutoSellSettingsAsync(string playerId, UpdateEquipmentAutoSellSettingsRequestDto request);


        /// <summary>
        /// 更新玩家头像图片路径。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <param name="avatarImagePath">头像相对路径。</param>
        /// <returns>更新后的玩家信息；不存在时返回空。</returns>
        Task<PlayerDto?> UpdateAvatarImageAsync(string playerId, string avatarImagePath);

        /// <summary>
        /// 为玩家增加经验值。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <param name="request">经验变更请求，兼容 <c>exp</c> 与 <c>amount</c> 两种字段。</param>
        /// <returns>变更后的玩家信息；不存在时返回空。</returns>
        Task<PlayerDto?> AddExpAsync(string playerId, AddExpRequestDto request);

        /// <summary>
        /// 扣除玩家金币。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <param name="amount">扣除数量。</param>
        /// <param name="reason">扣除原因。</param>
        /// <returns>扣除成功返回真。</returns>
        Task<bool> DeductGoldAsync(string playerId, long amount, string reason);

        /// <summary>
        /// 增加玩家金币。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <param name="amount">增加数量。</param>
        /// <param name="reason">增加原因。</param>
        /// <returns>增加成功返回真。</returns>
        Task<bool> AddGoldAsync(string playerId, long amount, string reason);

        /// <summary>
        /// 更新玩家最后登录时间。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <returns>更新成功返回真。</returns>
        Task<bool> UpdateLastLoginTimeAsync(string playerId);

        /// <summary>
        /// 检查玩家是否存在。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <returns>存在且未删除时返回真。</returns>
        Task<bool> ExistsAsync(string playerId);

        /// <summary>
        /// 获取玩家属性加点总览。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <returns>属性加点总览；不存在时返回空。</returns>
        Task<PlayerAttributePointOverviewDto?> GetAttributePointOverviewAsync(string playerId);

        /// <summary>
        /// 为指定属性投入 1 点属性点。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <param name="request">加点请求。</param>
        /// <returns>最新属性加点总览；不存在时返回空。</returns>
        Task<PlayerAttributePointOverviewDto?> AllocateAttributePointAsync(string playerId, AdjustPlayerAttributePointRequestDto request);

        /// <summary>
        /// 从指定属性返还 1 点属性点。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <param name="request">返还请求。</param>
        /// <returns>最新属性加点总览；不存在时返回空。</returns>
        Task<PlayerAttributePointOverviewDto?> RefundAttributePointAsync(string playerId, AdjustPlayerAttributePointRequestDto request);

        /// <summary>
        /// 执行一次突破。
        /// </summary>
        /// <param name="playerId">玩家唯一编号。</param>
        /// <returns>突破结果；不存在时返回空。</returns>
        Task<PlayerBreakthroughResultDto?> BreakthroughAsync(string playerId);
    }
}
