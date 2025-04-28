namespace MM.PipeBlocks.Abstractions;
/// <summary>
/// Represents the context that holds a value of type <typeparamref name="V"/> and its associated state, including a correlation ID, failure state, and execution flags.
/// </summary>
/// <typeparam name="V">The type of the value that the context holds.</typeparam>
public interface IContext<V>
{
    /// <summary>
    /// Gets or sets the correlation ID for the context, used for tracking the flow of operations.
    /// </summary>
    Guid CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the value of the context, which can either be a successful value of type <typeparamref name="V"/> or a failure state.
    /// </summary>
    Either<IFailureState<V>, V> Value { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the context is finished (i.e., the processing of the context is completed).
    /// </summary>
    bool IsFinished { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the context has been flipped, used to track state transitions.
    /// </summary>
    bool IsFlipped { get; set; }

    /// <summary>
    /// Signal to the pipe that the next block should not execute, with a failure
    /// </summary>
    /// <param name="failureState">Object that captures information about the failure</param>
    void SignalBreak (IFailureState<V> failureState)
    {
        Value = new(failureState);
        SignalBreak();
    }

    /// <summary>
    /// Signal to the pipe that the next block should not execute
    /// </summary>
    void SignalBreak()
    {
        IsFinished = true;
    }
}

public static class IContextExtensions
{
    public static void SignalBreak<V>(this IContext<V> context, IFailureState<V> failureState)
    {
        context.Value = new(failureState);
        SignalBreak(context);
    }

    public static void SignalBreak<V>(this IContext<V> context)
    {
        context.IsFinished = true;
    }
}