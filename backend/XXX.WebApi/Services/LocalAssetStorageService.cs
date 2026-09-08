using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using XXX.Application.DTOs;

namespace XXX.WebApi.Services
{
    /// <summary>
    /// 本地图片资源存储服务。
    /// </summary>
    public sealed class LocalAssetStorageService
    {
        private const int ThumbMaxWidth = 120;
        private const int ThumbMaxHeight = 120;
        /// <summary>
        /// 允许上传的图片扩展名集合。
        /// </summary>
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".png",
            ".jpg",
            ".jpeg",
            ".webp",
            ".gif"
        };

        /// <summary>
        /// 单张图片允许的最大大小，单位为字节。
        /// 当前限制为 5MB。
        /// </summary>
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        /// <summary>
        /// Web 宿主环境。
        /// 用于定位 <c>wwwroot/uploads</c> 存储目录。
        /// </summary>
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// 初始化本地图片资源存储服务。
        /// </summary>
        /// <param name="environment">Web 宿主环境。</param>
        public LocalAssetStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// 保存一张上传图片到本地目录。
        /// 会先校验文件大小和扩展名，再返回前端可直接访问的相对路径。
        /// </summary>
        /// <param name="file">上传文件。</param>
        /// <param name="category">图片分类。</param>
        /// <param name="cancellationToken">异步取消令牌。</param>
        /// <returns>图片保存结果。</returns>
        public async Task<AssetUploadResultDto> SaveImageAsync(IFormFile file, string category, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length <= 0)
            {
                throw new InvalidOperationException("未选择要上传的图片。");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException("图片不能超过 5MB。");
            }

            var extension = Path.GetExtension(file.FileName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("仅支持 png、jpg、jpeg、webp、gif 图片。");
            }

            var normalizedCategory = NormalizeCategory(category);
            var uploadsRoot = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", normalizedCategory);
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var relativePath = $"/uploads/{normalizedCategory}/{fileName}";
            return new AssetUploadResultDto
            {
                RelativePath = relativePath,
                Url = relativePath,
                FileName = string.IsNullOrWhiteSpace(file.FileName) ? fileName : file.FileName
            };
        }

        /// <summary>
        /// 保存上传图片并自动生成缩略图。
        /// </summary>
        public async Task<CollectionImageUploadResultDto> SaveImageWithThumbnailAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length <= 0)
                throw new InvalidOperationException("未选择要上传的图片。");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("图片不能超过 5MB。");

            var extension = Path.GetExtension(file.FileName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("仅支持 png、jpg、jpeg、webp、gif 图片。");

            var uploadsRoot = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "collections");
            var thumbRoot = Path.Combine(uploadsRoot, "thumb");
            Directory.CreateDirectory(uploadsRoot);
            Directory.CreateDirectory(thumbRoot);

            var baseName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
            var fileName = $"{baseName}{extension.ToLowerInvariant()}";
            var filePath = Path.Combine(uploadsRoot, fileName);
            var thumbPath = Path.Combine(thumbRoot, fileName);

            // 保存原图
            await using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // 生成缩略图
            using (var image = await Image.LoadAsync(filePath, cancellationToken))
            {
                var ratioX = (double)ThumbMaxWidth / image.Width;
                var ratioY = (double)ThumbMaxHeight / image.Height;
                var ratio = Math.Min(ratioX, ratioY);
                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                image.Mutate(x => x.Resize(newWidth, newHeight));
                await image.SaveAsync(thumbPath, cancellationToken);
            }

            return new CollectionImageUploadResultDto
            {
                OriginalPath = $"/uploads/collections/{fileName}",
                ThumbPath = $"/uploads/collections/thumb/{fileName}",
                FileName = string.IsNullOrWhiteSpace(file.FileName) ? fileName : file.FileName
            };
        }

        /// <summary>
        /// 删除反馈上传失败后遗留的临时图片。
        /// 仅允许处理 feedback 目录内的路径，避免路径越权。
        /// </summary>
        public bool DeleteFeedbackImage(string relativePath)
        {
            const string prefix = "/uploads/feedback/";
            if (string.IsNullOrWhiteSpace(relativePath) || !relativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
            var relativeFile = relativePath[prefix.Length..].Replace('/', Path.DirectorySeparatorChar);
            if (relativeFile.Contains("..", StringComparison.Ordinal)) return false;
            var root = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "feedback"));
            var fullPath = Path.GetFullPath(Path.Combine(root, relativeFile));
            if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath)) return false;
            File.Delete(fullPath);
            return true;
        }

        /// <summary>
        /// 归一化上传分类名称。
        /// 只允许映射到系统内置目录，避免任意字符串直接拼进磁盘路径。
        /// </summary>
        /// <param name="category">前端传入的图片分类。</param>
        /// <returns>归一化后的安全目录名。</returns>
        private static string NormalizeCategory(string category)
        {
            var normalized = string.IsNullOrWhiteSpace(category) ? "misc" : category.Trim().ToLowerInvariant();
            return normalized switch
            {
                "avatar" or "avatars" => "avatars",
                "item" or "items" => "items",
                "equipment" or "equipments" => "equipments",
                "pill" or "pills" => "pills",
                "forge" or "forge-recipes" => "forge-recipes",
                "world-boss" or "worldboss" => "world-boss",
                "collection" or "collections" => "collections",
                "feedback" or "feedbacks" => "feedback",
                _ => "misc"
            };
        }
    }
}
