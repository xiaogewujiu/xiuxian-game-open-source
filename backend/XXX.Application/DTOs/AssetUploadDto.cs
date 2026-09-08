namespace XXX.Application.DTOs
{
    /// <summary>
    /// 图片上传结果。
    /// </summary>
    public class AssetUploadResultDto
    {
        /// <summary>
        /// 相对资源路径。
        /// </summary>
        public string RelativePath { get; set; } = string.Empty;

        /// <summary>
        /// 可直接访问的地址。
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 原始文件名。
        /// </summary>
        public string FileName { get; set; } = string.Empty;
    }

    /// <summary>
    /// 图鉴图片上传结果（含缩略图）。
    /// </summary>
    public class CollectionImageUploadResultDto
    {
        /// <summary>
        /// 原图相对路径。
        /// </summary>
        public string OriginalPath { get; set; } = string.Empty;

        /// <summary>
        /// 缩略图相对路径。
        /// </summary>
        public string ThumbPath { get; set; } = string.Empty;

        /// <summary>
        /// 原始文件名。
        /// </summary>
        public string FileName { get; set; } = string.Empty;
    }
}
