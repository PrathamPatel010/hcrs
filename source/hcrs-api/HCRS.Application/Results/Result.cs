namespace HCRS.Application.Results;

public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    public Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new Result(true, null);
    public static Result Failure(Error error) => new Result(false, error);
}

public class Result<T> : Result
{
    public T? Data { get; }
    public Result(bool isSuccess, T? data, Error? error) : base(isSuccess, error)
    {
        Data = data;
    }
    public static Result<T> Success(T value) => new Result<T>(true, value, null);
    public new static Result<T> Failure(Error error) => new Result<T>(false, default, error);
}