using System.Net;
using System.Text.Json;
using XXX.Application.DTOs.Responses;

namespace XXX.WebApi.Middleware
{
    /// <summary>
    /// 全局异常处理中间件
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// 执行中间件
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (statusCode, clientMessage) = BuildErrorResponse(ex);
                if (statusCode >= (int)HttpStatusCode.InternalServerError)
                {
                    _logger.LogError(
                        ex,
                        "未处理的异常。Method={Method}, Path={Path}, Message={Message}",
                        context.Request.Method,
                        context.Request.Path,
                        ex.Message);
                }
                else
                {
                    _logger.LogWarning(
                        "业务请求被拒绝。Method={Method}, Path={Path}, StatusCode={StatusCode}, Message={Message}",
                        context.Request.Method,
                        context.Request.Path,
                        statusCode,
                        ex.Message);
                }

                await HandleExceptionAsync(context, statusCode, clientMessage);
            }
        }

        /// <summary>
        /// 处理异常
        /// </summary>
        private static Task HandleExceptionAsync(HttpContext context, int statusCode, string clientMessage)
        {
            context.Response.ContentType = "application/json";
            var response = ApiResponse.Error(clientMessage, statusCode);

            context.Response.StatusCode = response.Code;
            response.Path = context.Request.Path;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }

        private static (int StatusCode, string ClientMessage) BuildErrorResponse(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "未授权访问"),
                KeyNotFoundException => ((int)HttpStatusCode.NotFound, "资源不存在"),
                ArgumentException => ((int)HttpStatusCode.BadRequest, exception.Message),
                InvalidOperationException => ((int)HttpStatusCode.BadRequest, exception.Message),
                _ => ((int)HttpStatusCode.InternalServerError, "服务器内部错误")
            };
        }
    }
}
