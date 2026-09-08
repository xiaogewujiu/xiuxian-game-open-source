namespace XXX.Quest
{
    /// <summary>
    /// 任务操作结果。
    /// 供应用层任务服务和 WebApi 返回统一结果模型。
    /// </summary>
    public class QuestOperationResult
    {
        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 任务 ID。
        /// </summary>
        public string QuestId { get; set; } = string.Empty;
    }

    /// <summary>
    /// 任务提交结果。
    /// 供应用层任务服务和 WebApi 返回统一结果模型。
    /// </summary>
    public class QuestSubmitResult
    {
        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 任务 ID。
        /// </summary>
        public string QuestId { get; set; } = string.Empty;

        /// <summary>
        /// 奖励结果。
        /// </summary>
        public QuestRewardResult RewardResult { get; set; } = new();
    }
}
