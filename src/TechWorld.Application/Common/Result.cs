namespace TechWorld.Application.Common;

public class Result<T>
{
    public T? Value { get; private set; }
    public string? Error { get; private set; }
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;

    private Result() { }

    public static Result<T> Ok(T value) => new() { Value = value, IsSuccess = true };
    public static Result<T> Fail(string error) => new() { Error = error, IsSuccess = false };
}

public class Result
{
    public string? Error { get; private set; }
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;

    private Result() { }

    public static Result Ok() => new() { IsSuccess = true };
    public static Result Fail(string error) => new() { Error = error, IsSuccess = false };
}
