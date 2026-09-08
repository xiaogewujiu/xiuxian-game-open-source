using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 公会服务接口
    /// 提供公会创建、加入、退出、查询等公会管理功能
    /// </summary>
    /// <remarks>
    /// 公会系统是游戏的社交核心，提供：
    /// - 公会创建和管理
    /// - 成员管理
    /// - 公会贡献系统
    /// - 公会等级和权限
    /// </remarks>
    public interface IGuildService
    {
        /// <summary>
        /// 获取所有公会列表
        /// </summary>
        /// <returns>公会列表，只返回未删除的公会</returns>
        /// <remarks>
        /// 返回信息包含：
        /// - 公会ID和名称
        /// - 公会等级
        /// - 成员数量和上限
        /// - 会长名称
        /// </remarks>
        Task<List<GuildDto>> GetGuildsAsync();

        /// <summary>
        /// 创建公会
        /// </summary>
        /// <param name="playerId">创建者玩家ID</param>
        /// <param name="request">创建公会请求，包含公会名称</param>
        /// <returns>创建成功的公会信息</returns>
        /// <exception cref="Exception">当公会名称已存在或玩家已加入其他公会时抛出异常</exception>
        /// <remarks>
        /// 创建流程：
        /// 1. 验证公会名称唯一性
        /// 2. 验证玩家未加入其他公会
        /// 3. 创建公会实体
        /// 4. 创建会长成员记录
        /// 5. 返回公会信息
        /// </remarks>
        Task<GuildDto> CreateGuildAsync(string playerId, CreateGuildRequestDto request);

        /// <summary>
        /// 加入公会
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <param name="guildId">目标公会ID</param>
        /// <returns>是否加入成功</returns>
        /// <exception cref="Exception">当公会不存在、玩家已加入其他公会或公会已满时抛出异常</exception>
        /// <remarks>
        /// 加入流程：
        /// 1. 验证公会存在且未删除
        /// 2. 验证玩家未加入其他公会
        /// 3. 验证公会未满员
        /// 4. 创建成员记录
        /// 5. 更新公会成员数量
        /// </remarks>
        Task<bool> JoinGuildAsync(string playerId, string guildId);

        /// <summary>
        /// 退出公会
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>是否退出成功</returns>
        /// <exception cref="Exception">当玩家未加入公会或玩家是会长时抛出异常</exception>
        /// <remarks>
        /// 退出流程：
        /// 1. 验证玩家已加入公会
        /// 2. 验证玩家不是会长（会长需要先转让或解散公会）
        /// 3. 删除成员记录
        /// 4. 更新公会成员数量
        /// </remarks>
        Task<bool> LeaveGuildAsync(string playerId);

        /// <summary>
        /// 获取玩家所在公会
        /// </summary>
        /// <param name="playerId">玩家ID</param>
        /// <returns>公会信息，如果玩家未加入公会则返回null</returns>
        Task<GuildDto?> GetPlayerGuildAsync(string playerId);

        /// <summary>
        /// 获取公会成员列表
        /// </summary>
        /// <param name="guildId">公会ID</param>
        /// <returns>成员列表，按职位和贡献排序</returns>
        /// <remarks>
        /// 返回信息包含：
        /// - 玩家ID和名称
        /// - 玩家等级
        /// - 职位（会长、副会长、长老、成员）
        /// - 贡献值
        /// - 加入时间
        /// </remarks>
        Task<List<GuildMemberDto>> GetGuildMembersAsync(string guildId);
    }
}
