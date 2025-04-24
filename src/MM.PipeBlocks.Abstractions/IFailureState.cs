namespace MM.PipeBlocks.Abstractions;
/// <summary>
/// Represents the failure state of a context, including the failed value, the correlation ID, and the reason for failure.
/// </summary>
/// <typeparam name="V">The type of the value associated with the failure state.</typeparam>
public interface IFailureState<V>
{
    /// <summary>
    /// Gets or sets the value of type <typeparamref name="V"/> associated with the failure state.
    /// </summary>
    V Value { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID associated with the failure state, used for tracking the flow of operations.
    /// </summary>
    Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the failure reason as a string, providing more information about why the operation failed.
    /// </summary>
    string? FailureReason { get; set; }
}