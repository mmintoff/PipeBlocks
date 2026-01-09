namespace MM.PipeBlocks.Abstractions;

public interface IFailureState
{
    object? Value { get; set; }
    Guid CorrelationId { get; set; }
    string? FailureReason { get; set; }

    bool TryGetValue<T>(out T? result)
    {
        if (Value is T castValue)
        {
            result = castValue;
            return true;
        }
        result = default;
        return false;
    }

    T GetValue<T>()
    {
        if (Value is T castValue)
            return castValue;

        throw new InvalidCastException(
            $"Value is not of type {typeof(T).FullName}. Actual type: {Value?.GetType().FullName ?? "null"}");
    }
}

public interface IFailureState<VIn> : IFailureState
{
    new VIn Value { get; set; }
}