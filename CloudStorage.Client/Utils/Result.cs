namespace CloudStorage.Client.Utils;

public class Result<T>
{
    public T Value { get; set; }
    public string Error { get; set; }

    public bool IsError { get; set; } = false;

    public Result(T value)
    {
        Value = value;
        IsError = false;
    }

    public Result(string error)
    {
        Error = error;
        IsError = true;
    }
}
