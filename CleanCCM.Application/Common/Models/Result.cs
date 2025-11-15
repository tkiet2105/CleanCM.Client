namespace CleanCCM.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public Error Error { get; protected set; }

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }
}

public class Result<T> : Result
{
    public T Value { get; }

    private Result(bool isSuccess, T value, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, Error.None);
    }

    public static Result<T> Failure(Error error)
    {
        return new Result<T>(false, default, error);
    }
}
