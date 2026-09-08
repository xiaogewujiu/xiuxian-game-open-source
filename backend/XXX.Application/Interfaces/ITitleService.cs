using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 玩家称号服务接口。
    /// </summary>
    public interface ITitleService
    {
        /// <summary>
        /// 获取玩家称号总览（当前佩戴 + 已拥有列表）。
        /// </summary>
        Task<PlayerTitleOverviewDto> GetOverviewAsync(string playerId);

        /// <summary>
        /// 佩戴称号。
        /// </summary>
        Task<bool> EquipTitleAsync(string playerId, string titleId);

        /// <summary>
        /// 取消佩戴称号。
        /// </summary>
        Task<bool> UnequipTitleAsync(string playerId);

        /// <summary>
        /// 获取指定玩家当前佩戴的称号（供其他模块调用）。
        /// </summary>
        Task<CurrentTitleDto?> GetCurrentTitleAsync(string playerId);

        /// <summary>
        /// 授予称号（系统内部调用）。
        /// </summary>
        Task<bool> GrantTitleAsync(string playerId, string titleId);

        /// <summary>
        /// 回收称号（系统内部调用）。
        /// </summary>
        Task<bool> RevokeTitleAsync(string playerId, string titleId);
    }
}
