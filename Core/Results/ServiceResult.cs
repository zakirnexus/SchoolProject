namespace SchoolProject.Core.Results
{
    public class ServiceResult<T>
    {
        public bool Success { get; init; }

        public string? ErrorMessage { get; init; }

        public T? Data { get; init; }

        public static ServiceResult<T> Ok(T data)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ServiceResult<T> Fail(string message)
        {
            return new ServiceResult<T>
            {
                Success = false,
                ErrorMessage = message
            };
        }
    }
}