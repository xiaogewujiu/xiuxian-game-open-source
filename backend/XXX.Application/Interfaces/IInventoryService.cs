using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 背包服务接口
    /// 负责管理玩家背包中的物品，包括查询、添加、使用、丢弃等操作
    /// </summary>
    /// <remarks>
    /// 背包系统是游戏的核心系统之一，与其他系统有以下关联：
    /// 1. 与玩家系统关联：所有物品都属于特定玩家
    /// 2. 与装备系统关联：装备类物品可以穿戴
    /// 3. 与商店系统关联：物品可以出售获得金币
    /// 4. 与任务系统关联：任务可能需要提交特定物品
    /// 5. 与炼丹系统关联：炼丹需要消耗材料物品
    /// </remarks>
    public interface IInventoryService
    {
        /// <summary>
        /// 获取玩家背包中的所有物品列表
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <returns>物品列表，包含物品ID、名称、数量、品质等信息</returns>
        /// <exception cref="ArgumentException">当playerId为空或无效时抛出</exception>
        Task<List<InventoryItemDto>> GetInventoryItemsAsync(string playerId);

        /// <summary>
        /// 获取指定物品的详细信息
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="itemInstanceId">物品实例ID</param>
        /// <returns>物品详细信息，如果物品不存在则返回null</returns>
        Task<InventoryItemDto?> GetItemDetailAsync(string playerId, long itemInstanceId);

        /// <summary>
        /// 添加物品到玩家背包
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="request">添加物品请求，包含物品ID、数量、是否绑定等信息</param>
        /// <returns>添加后的物品信息</returns>
        /// <remarks>
        /// 如果背包中已存在相同物品（且可叠加），则增加数量
        /// 如果不存在或不可叠加，则创建新的物品实例
        /// </remarks>
        Task<InventoryItemDto> AddItemAsync(string playerId, AddItemRequestDto request);

        /// <summary>
        /// 使用物品
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="request">使用物品请求，包含物品实例ID、使用数量、使用目标等</param>
        /// <returns>使用结果，包含是否成功、效果描述、剩余数量等</returns>
        /// <remarks>
        /// 使用物品可能触发以下效果：
        /// 1. 恢复HP/MP
        /// 2. 获得经验值
        /// 3. 获得buff效果
        /// 4. 触发特殊事件
        /// </remarks>
        Task<ItemUseResultDto> UseItemAsync(string playerId, UseItemRequestDto request);

        Task<SkillBookDecomposeResultDto> DecomposeSkillBookAsync(string playerId, SkillBookDecomposeRequestDto request);

        /// <summary>
        /// 丢弃物品
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="request">丢弃请求，包含物品实例ID和丢弃数量</param>
        /// <returns>是否成功丢弃</returns>
        /// <remarks>
        /// 注意：绑定的物品通常不能丢弃
        /// 丢弃的物品将从背包中永久删除，不会进入回收站
        /// </remarks>
        Task<bool> DiscardItemAsync(string playerId, DiscardItemRequestDto request);

        Task<BatchDiscardItemResultDto> DiscardItemsAsync(string playerId, BatchDiscardItemRequestDto request);

        /// <summary>
        /// 锁定道具
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="request">锁定请求</param>
        /// <returns>锁定结果</returns>
        Task<LockItemResultDto> LockItemAsync(string playerId, LockItemRequestDto request);

        /// <summary>
        /// 解锁道具
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="request">解锁请求</param>
        /// <returns>解锁结果</returns>
        Task<LockItemResultDto> UnlockItemAsync(string playerId, LockItemRequestDto request);

        /// <summary>
        /// 整理背包
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <returns>整理结果，包含整理前后的物品数量变化</returns>
        /// <remarks>
        /// 整理操作会：
        /// 1. 合并可叠加的相同物品
        /// 2. 按物品类型重新排序
        /// 3. 清理过期物品（如果有）
        /// </remarks>
        Task<InventoryOrganizeResultDto> OrganizeInventoryAsync(string playerId);

        /// <summary>
        /// 检查玩家是否拥有指定数量的某物品
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="itemId">物品模板ID</param>
        /// <returns>物品总数量（包含所有可叠加的该物品）</returns>
        Task<int> GetItemCountAsync(string playerId, string itemId);

        /// <summary>
        /// 从背包中扣除指定数量的物品
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="itemId">物品模板ID</param>
        /// <param name="quantity">扣除数量</param>
        /// <param name="reason">扣除原因，用于日志记录</param>
        /// <returns>是否成功扣除</returns>
        /// <remarks>
        /// 扣除顺序：优先扣除非绑定物品，然后扣除绑定物品
        /// 如果数量不足，则扣除失败，不会部分扣除
        /// </remarks>
        Task<bool> DeductItemAsync(string playerId, string itemId, int quantity, string reason);
    }
}
