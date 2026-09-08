namespace XXX.Application.DTOs.Responses
{
    /// <summary>
    /// 统一API响应基类
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 状态码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 请求路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ApiResponse()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 创建成功响应
        /// </summary>
        public static ApiResponse Ok(string message = "操作成功")
        {
            return new ApiResponse
            {
                Success = true,
                Code = 200,
                Message = message
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        public static ApiResponse Fail(string message, int code = 400)
        {
            return new ApiResponse
            {
                Success = false,
                Code = code,
                Message = message
            };
        }

        /// <summary>
        /// 创建错误响应
        /// </summary>
        public static ApiResponse Error(string message, int code = 500)
        {
            return new ApiResponse
            {
                Success = false,
                Code = code,
                Message = message
            };
        }
    }

    /// <summary>
    /// 统一API响应泛型类
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ApiResponse<T> : ApiResponse
    {
        /// <summary>
        /// 响应数据
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 创建成功响应
        /// </summary>
        public static ApiResponse<T> Ok(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Code = 200,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 创建失败响应
        /// </summary>
        public static new ApiResponse<T> Fail(string message, int code = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Code = code,
                Message = message,
                Data = default
            };
        }

        /// <summary>
        /// 创建带返回数据的失败响应
        /// </summary>
        public static ApiResponse<T> Fail(string message, T? data, int code = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Code = code,
                Message = message,
                Data = data
            };
        }
    }

    /// <summary>
    /// 通用分页结果（与前端 AdminPagedResult 对应）
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 分页响应数据
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage => PageIndex > 1;

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> Items { get; set; } = [];

        /// <summary>
        /// 创建分页响应
        /// </summary>
        public static PagedResponse<T> Create(List<T> items, int totalCount, int pageIndex, int pageSize)
        {
            return new PagedResponse<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
    }
}
