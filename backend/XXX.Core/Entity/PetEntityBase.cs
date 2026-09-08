namespace XXX.Entity
{
    /// <summary>
    /// 宠物的实体 从野怪模版中生成
    /// </summary>
    public class PetEntity : BaseAttributes
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string GID { get; set; } = string.Empty;

        /// <summary>
        /// 道号
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 等级
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// 携带的技能ID
        /// </summary>
        private List<string> skillIds = [];

        /// <summary>
        /// 获取技能ID列表
        /// </summary>
        public List<string> GetSkillIds() => skillIds;

        /// <summary>
        /// 设置技能ID列表
        /// </summary>
        public void SetSkillIds(List<string> ids) => skillIds = ids;
    }
}
