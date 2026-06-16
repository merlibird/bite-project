namespace Bite.Services.Common;

public class ServiceResult<T>
{
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsSuccess => ErrorMessage == null;
    public ServiceResultType ResultType { get; private set; }

    public static ServiceResult<T> Success(T data) => new() { Data = data, ResultType = ServiceResultType.Success };
    public static ServiceResult<T> Failure(string message, ServiceResultType type = ServiceResultType.Error) 
        => new() { ErrorMessage = message, ResultType = type };
}

public enum ServiceResultType
{
    Success,
    Error,
    Conflict,
    NotFound,
    Forbidden,
    Unauthorized,
    ValidationError
}
