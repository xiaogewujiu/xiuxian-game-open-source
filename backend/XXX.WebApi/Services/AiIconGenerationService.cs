using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace XXX.WebApi.Services
{
    /// <summary>
    /// AI 图标生成服务 —— 调用 Agnes API 生成图标，处理透明背景并裁剪。
    /// </summary>
    public sealed class AiIconGenerationService
    {
        private const string ApiEndpoint = "https://apihub.agnes-ai.com/v1/images/generations";
        private const string Model = "agnes-image-2.0-flash";
        private const int IconSize = 128;
        private const int WhiteThreshold = 220;

        private static readonly string StylePrefix =
            "minimalist flat icon, simple clean design, pure white background, no shadow, no gradients, " +
            "perfectly centered composition, centered on canvas with equal padding on all sides, " +
            "Chinese fantasy xianxia theme, ";

        private static readonly string TitleStylePrefix =
            "rectangular banner badge design, elegant ornate border, pure white background, no shadow, " +
            "centered composition, Chinese fantasy xianxia theme, ";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LocalAssetStorageService _assetStorage;
        private readonly ILogger<AiIconGenerationService> _logger;
        private readonly string _apiKey;

        public AiIconGenerationService(
            IHttpClientFactory httpClientFactory,
            LocalAssetStorageService assetStorage,
            ILogger<AiIconGenerationService> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _assetStorage = assetStorage;
            _logger = logger;
            _apiKey = configuration["AiIcon:ApiKey"]
                ?? "sk-HFVBMYa1wTKdMaCjH5HVDllJHRBJ845YaqHfcSOlpZcDYHJO";
        }

        /// <summary>
        /// 生成图标并保存到指定分类目录。
        /// </summary>
        /// <param name="prompt">用户输入的提示词。</param>
        /// <param name="category">存储分类（items / equipments 等）。</param>
        /// <param name="oldRelativePath">上一次生成的图标路径（用于删除旧文件）。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>生成结果，包含相对路径和预览 URL。</returns>
        public async Task<IconGenerationResult> GenerateAsync(
            string prompt,
            string category,
            string? oldRelativePath,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new InvalidOperationException("提示词不能为空。");

            // 删除旧文件
            if (!string.IsNullOrWhiteSpace(oldRelativePath))
            {
                DeleteFileIfExists(oldRelativePath);
            }

            var normalizedCategory = NormalizeCategory(category);
            var isTitle = normalizedCategory == "title-image";

            // 1. 调用 Agnes API 生成图片
            var imageBytes = await CallAgnesApiAsync(prompt, isTitle, cancellationToken);

            // 2. 处理图片：去白底、裁剪、缩放（称号用长方形，其他用正方形）
            var processedBytes = isTitle ? ProcessTitleImage(imageBytes) : ProcessImage(imageBytes);

            // 3. 保存到分类目录
            var uploadsRoot = Path.Combine("wwwroot", "uploads", normalizedCategory);
            var fullDir = Path.Combine(Directory.GetCurrentDirectory(), uploadsRoot);
            Directory.CreateDirectory(fullDir);

            var fileName = $"ai_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}.png";
            var fullPath = Path.Combine(fullDir, fileName);
            await File.WriteAllBytesAsync(fullPath, processedBytes, cancellationToken);

            var relativePath = $"/uploads/{normalizedCategory}/{fileName}";

            _logger.LogInformation("AI icon generated: {Path}", relativePath);

            return new IconGenerationResult
            {
                RelativePath = relativePath,
                Url = relativePath,
                FileName = fileName
            };
        }

        /// <summary>
        /// 删除指定相对路径对应的本地文件。
        /// </summary>
        public void DeleteFileIfExists(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var cleanPath = relativePath.TrimStart('/');
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cleanPath);

            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Deleted old AI icon: {Path}", relativePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old icon: {Path}", relativePath);
            }
        }

        private async Task<byte[]> CallAgnesApiAsync(string prompt, bool isTitle, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(180);

            var stylePrefix = isTitle ? TitleStylePrefix : StylePrefix;
            var imageSize = isTitle ? "1024x512" : "1024x1024";

            var request = new
            {
                model = Model,
                prompt = stylePrefix + prompt,
                size = imageSize,
                extra_body = new { response_format = "url" }
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint);
            httpRequest.Content = JsonContent.Create(request);
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            using var response = await client.SendAsync(httpRequest, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Agnes API error: {Status} {Body}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"AI 图片生成失败（{(int)response.StatusCode}）。");
            }

            var result = await response.Content.ReadFromJsonAsync<AgnesResponse>(cancellationToken: ct);
            var imageUrl = result?.Data?.FirstOrDefault()?.Url;
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new InvalidOperationException("AI 未返回有效图片。");

            // 下载图片
            var imageBytes = await client.GetByteArrayAsync(imageUrl, ct);
            return imageBytes;
        }

        private static byte[] ProcessImage(byte[] imageBytes)
        {
            using var image = Image.Load<Rgba32>(imageBytes);

            // 去除近白色背景（设为透明）
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        ref var pixel = ref row[x];
                        if (pixel.R > WhiteThreshold && pixel.G > WhiteThreshold && pixel.B > WhiteThreshold)
                        {
                            pixel.A = 0;
                        }
                    }
                }
            });

            // 裁剪到内容区域
            var bounds = GetContentBounds(image);
            if (bounds.IsEmpty)
            {
                // 没有内容，返回空白
                using var empty = new Image<Rgba32>(IconSize, IconSize, new Rgba32(0, 0, 0, 0));
                using var ms = new MemoryStream();
                empty.SaveAsPng(ms);
                return ms.ToArray();
            }

            // 裁剪
            var cropped = image.Clone(ctx => ctx.Crop(bounds));

            // 创建正方形画布，15% 边距
            var contentW = bounds.Width;
            var contentH = bounds.Height;
            var maxDim = Math.Max(contentW, contentH);
            var padding = (int)(maxDim * 0.15);
            var canvasSize = maxDim + padding * 2;

            using var canvas = new Image<Rgba32>(canvasSize, canvasSize, new Rgba32(0, 0, 0, 0));
            var pasteX = (canvasSize - contentW) / 2;
            var pasteY = (canvasSize - contentH) / 2;
            canvas.Mutate(ctx => ctx.DrawImage(cropped, new Point(pasteX, pasteY), 1f));

            // 缩放到 128x128
            canvas.Mutate(ctx => ctx.Resize(IconSize, IconSize, KnownResamplers.Lanczos3));

            using var outputMs = new MemoryStream();
            canvas.SaveAsPng(outputMs);
            return outputMs.ToArray();
        }

        private static byte[] ProcessTitleImage(byte[] imageBytes)
        {
            const int titleWidth = 80;
            const int titleHeight = 30;

            using var image = Image.Load<Rgba32>(imageBytes);

            // 去除近白色背景（设为透明）
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        ref var pixel = ref row[x];
                        if (pixel.R > WhiteThreshold && pixel.G > WhiteThreshold && pixel.B > WhiteThreshold)
                        {
                            pixel.A = 0;
                        }
                    }
                }
            });

            // 裁剪到内容区域
            var bounds = GetContentBounds(image);
            if (bounds.IsEmpty)
            {
                using var empty = new Image<Rgba32>(titleWidth, titleHeight, new Rgba32(0, 0, 0, 0));
                using var ms = new MemoryStream();
                empty.SaveAsPng(ms);
                return ms.ToArray();
            }

            var cropped = image.Clone(ctx => ctx.Crop(bounds));

            // 保持宽高比缩放到目标尺寸内
            var ratio = Math.Min((double)titleWidth / bounds.Width, (double)titleHeight / bounds.Height);
            var scaledW = (int)(bounds.Width * ratio);
            var scaledH = (int)(bounds.Height * ratio);

            cropped.Mutate(ctx => ctx.Resize(scaledW, scaledH, KnownResamplers.Lanczos3));

            // 居中放置到画布
            using var canvas = new Image<Rgba32>(titleWidth, titleHeight, new Rgba32(0, 0, 0, 0));
            var pasteX = (titleWidth - scaledW) / 2;
            var pasteY = (titleHeight - scaledH) / 2;
            canvas.Mutate(ctx => ctx.DrawImage(cropped, new Point(pasteX, pasteY), 1f));

            using var outputMs = new MemoryStream();
            canvas.SaveAsPng(outputMs);
            return outputMs.ToArray();
        }

        private static Rectangle GetContentBounds(Image<Rgba32> image)
        {
            int minX = image.Width, minY = image.Height, maxX = 0, maxY = 0;
            bool found = false;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < row.Length; x++)
                    {
                        if (row[x].A > 0)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                            found = true;
                        }
                    }
                }
            });

            if (!found) return Rectangle.Empty;
            return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }

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
                "title-image" or "title" => "title-image",
                _ => "misc"
            };
        }
    }

    public class IconGenerationResult
    {
        public string RelativePath { get; set; } = "";
        public string Url { get; set; } = "";
        public string FileName { get; set; } = "";
    }

    // Agnes API 响应模型
    file class AgnesResponse
    {
        [JsonPropertyName("data")]
        public List<AgnesImageData>? Data { get; set; }
    }

    file class AgnesImageData
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
