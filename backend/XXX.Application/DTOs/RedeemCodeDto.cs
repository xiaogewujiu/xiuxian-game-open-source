namespace XXX.Application.DTOs
{
    /// <summary>
    /// 兑换码请求 DTO。
    /// </summary>
    public class RedeemCodeRequestDto
    {
        /// <summary>
        /// 要兑换的奖励码文本。
        /// </summary>
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// 中文注释：
    /// 兑换码兑换结果。
    /// 当前接口不只返回“成功/失败”，还返回实际奖励列表和兑换时间，
    /// 这样前端可以直接把结果弹窗展示完整，不需要再拼接额外说明。
    /// </summary>
    public class RedeemCodeResultDto
    {
        /// <summary>
        /// 本次兑换使用的奖励码。
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 兑换结果提示。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 兑换完成时间。
        /// </summary>
        public DateTime RedeemedAt { get; set; }

        /// <summary>
        /// 实际发放奖励列表。
        /// </summary>
        public List<RewardItemDto> Rewards { get; set; } = [];
    }
}
