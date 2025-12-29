using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure
/// <summary>
/// A block that marks the parameter as finished and optionally transforms it before returning.
/// Supports both synchronous and asynchronous execution logic.
/// </summary>
/// <typeparam name="V">The type of the value held in the parameter.</typeparam>
public sealed class ReturnBlock<V> : ISyncBlock<V>, IAsyncBlock<V>
{
    private readonly Func<Parameter<V>, Parameter<V>>? _syncFunc;
    private readonly Func<Parameter<V>, ValueTask<Parameter<V>>>? _asyncFunc;

    private readonly ExecutionStrategy _executionStrategy;
    private readonly ILogger<ReturnBlock<V>> _logger;

    private static readonly Action<ILogger, Guid, Exception?> s_logContextTerminated =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            default,
            "Context {CorrelationId} terminated in Return Block");

    private enum ExecutionStrategy : byte
    {
        NoTransform,
        SyncFunc,
        AsyncFunc
    }

    /// <summary>
    /// Initializes a return block that marks parameter as finished without transformation.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    public ReturnBlock(ILogger<ReturnBlock<V>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _executionStrategy = ExecutionStrategy.NoTransform;
    }

    /// <summary>
    /// Initializes a synchronous return block using a transformation function.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    /// <param name="func">A function that transforms the parameter.</param>
    public ReturnBlock(ILogger<ReturnBlock<V>> logger, Func<Parameter<V>, Parameter<V>> func)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _syncFunc = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.SyncFunc;
    }

    /// <summary>
    /// Initializes an asynchronous return block using a transformation function.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    /// <param name="func">An asynchronous function that transforms the parameter.</param>
    public ReturnBlock(ILogger<ReturnBlock<V>> logger, Func<Parameter<V>, ValueTask<Parameter<V>>> func)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _asyncFunc = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.AsyncFunc;
    }

    /// <summary>
    /// Executes the block synchronously, transforming and finalizing the parameter.
    /// </summary>
    /// <param name="value">The execution parameter.</param>
    /// <returns>The transformed and finalized parameter.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        value.Context.IsFinished = true;
        s_logContextTerminated(_logger, value.Context.CorrelationId, null);

        return value.Match(
            _ => value,
            x => ExecuteWithStrategy(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Parameter<V> ExecuteWithStrategy(Parameter<V> value)
    {
        return _executionStrategy switch
        {
            ExecutionStrategy.NoTransform => value,
            ExecutionStrategy.SyncFunc => _syncFunc!(value),
            ExecutionStrategy.AsyncFunc => GetResultFromAsync(_asyncFunc!(value)),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };
    }

    /// <summary>
    /// Executes the block asynchronously, transforming and finalizing the parameter.
    /// </summary>
    /// <param name="value">The execution parameter.</param>
    /// <returns>A task that represents the asynchronous operation, containing the updated parameter.</returns>
    public ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        value.Context.IsFinished = true;
        s_logContextTerminated(_logger, value.Context.CorrelationId, null);

        return value.Match(
            _ => new ValueTask<Parameter<V>>(value), // If value is Some but we're not using it, just return context
            x => ExecuteWithStrategyAsync(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask<Parameter<V>> ExecuteWithStrategyAsync(Parameter<V> value)
    {
        return _executionStrategy switch
        {
            ExecutionStrategy.NoTransform => new ValueTask<Parameter<V>>(value),
            ExecutionStrategy.SyncFunc => new ValueTask<Parameter<V>>(_syncFunc!(value)),
            ExecutionStrategy.AsyncFunc => _asyncFunc!(value),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };
    }

    // Helper method to synchronously get result from async operation
    // This should only be used when we know the operation can be completed synchronously
    // or when we have no choice but to block (like in the sync Execute method)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Parameter<V> GetResultFromAsync(ValueTask<Parameter<V>> task)
        => task.IsCompleted ? task.Result : AsyncContext.Run(task.AsTask);
}

/// <summary>
/// A no-op block that simply returns the provided parameter unchanged.
/// </summary>
/// <typeparam name="V">The type of the value held in the parameter.</typeparam>
public sealed class NoopBlock<V> : ISyncBlock<V>, IAsyncBlock<V>
{
    /// <summary>
    /// Synchronously returns the parameter as-is.
    /// </summary>
    /// <param name="value">The current parameter.</param>
    /// <returns>The unchanged parameter.</returns>
    public Parameter<V> Execute(Parameter<V> value) => value;

    /// <summary>
    /// Asynchronously returns the parameter as-is.
    /// </summary>
    /// <param name="value">The current parameter.</param>
    /// <returns>A task that represents the asynchronous operation, containing the unchanged parameter.</returns>
    public ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value) => ValueTask.FromResult(value);
}
