using XXX.Application.DTOs;

namespace XXX.Application.Interfaces
{
    /// <summary>
    /// 中文注释：
    /// 锻造服务接口。
    /// 当前阶段只负责一个“最小但真实”的开发闭环：
    /// 1. 返回后端真实可锻造图纸；
    /// 2. 校验金币与材料；
    /// 3. 真实生成装备实例并写入数据库。
    /// </summary>
    public interface IForgeService
    {
        /// <summary>
        /// 获取锻造总览。
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <returns>锻造总览</returns>
        Task<ForgeOverviewDto> GetForgeOverviewAsync(string playerId);

        /// <summary>
        /// 执行锻造。
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <param name="request">锻造请求</param>
        /// <returns>锻造结果</returns>
        Task<ForgeEquipmentResultDto> ForgeAsync(string playerId, ForgeEquipmentRequestDto request);

        /// <summary>
        /// 领取已完成的锻造结果。
        /// </summary>
        /// <param name="playerId">玩家唯一标识符</param>
        /// <returns>锻造结果</returns>
        Task<ForgeEquipmentResultDto?> CollectForgeResultAsync(string playerId);
    }
}
