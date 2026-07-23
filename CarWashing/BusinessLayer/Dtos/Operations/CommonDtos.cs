namespace BusinessLayer.Dtos.Operations
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

    public class OperationResult<T>
    {
        public bool Succeeded { get; set; }
        public int StatusCode { get; set; }
        public string? Error { get; set; }
        public T? Data { get; set; }

        public static OperationResult<T> Success(T data, int statusCode = 200)
        {
            return new OperationResult<T>
            {
                Succeeded = true,
                StatusCode = statusCode,
                Data = data
            };
        }

        public static OperationResult<T> Failure(string error, int statusCode)
        {
            return new OperationResult<T>
            {
                Succeeded = false,
                StatusCode = statusCode,
                Error = error
            };
        }
    }
}
