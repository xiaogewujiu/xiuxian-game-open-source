using System.Text.Json;
using XXX.Application.DTOs.Responses;

namespace XXX.WebApi.Middleware
{
    /// <summary>
    /// API统一响应中间件
    /// 将控制器返回的数据包装为统一格式
    /// </summary>
    public class ApiResponseMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ApiResponseMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 执行中间件
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                // 只处理成功的API响应
                if (context.Response.StatusCode == 200 &&
                    context.Response.ContentType?.Contains("application/json") == true)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var body = await new StreamReader(responseBody).ReadToEndAsync();

                    // 如果已经是ApiResponse格式，则不再包装
                    if (!string.IsNullOrEmpty(body) && !body.TrimStart().StartsWith("{"))
                    {
                        var wrappedResponse = ApiResponse.Ok(body);
                        await WriteResponseAsync(context, wrappedResponse, originalBodyStream);
                        return;
                    }

                    // 尝试解析为对象并包装
                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        var data = doc.RootElement;
                        var wrappedResponse = ApiResponse<object>.Ok(data, "操作成功");
                        await WriteResponseAsync(context, wrappedResponse, originalBodyStream);
                        return;
                    }
                    catch
                    {
                        // 解析失败，返回原始内容
                    }
                }

                // 复制原始响应
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        /// <summary>
        /// 写入包装后的响应
        /// </summary>
        private static async Task WriteResponseAsync(HttpContext context, ApiResponse response, Stream originalBody)
        {
            context.Response.Body = originalBody;
            context.Response.ContentType = "application/json";

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await JsonSerializer.SerializeAsync(originalBody, response, options);
        }
    }
}
