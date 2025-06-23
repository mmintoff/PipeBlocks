using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure
/// <summary>
/// A block that marks the context as finished and optionally transforms it before returning.
/// Supports both synchronous and asynchronous execution logic.
/// </summary>
/// <typeparam name="C">The type of context implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value held in the context.</typeparam>
public sealed class ReturnBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    private readonly Func<C, C>? _syncContextFunc;
    private readonly Func<C, V, C>? _syncContextValueFunc;
    private readonly Func<C, ValueTask<C>>? _asyncContextFunc;
    private readonly Func<C, V, ValueTask<C>>? _asyncContextValueFunc;

    private readonly ExecutionStrategy _executionStrategy;
    private readonly ILogger<ReturnBlock<C, V>> _logger;

    private static readonly Action<ILogger, Guid, Exception?> s_logContextTerminated =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            default,
            "Context {CorrelationId} terminated in Return Block");

    private enum ExecutionStrategy : byte
    {
        NoTransform,
        SyncContext,
        SyncContextValue,
        AsyncContext,
        AsyncContextValue
    }

    /// <summary>
    /// Initializes a return block that marks context as finished without transformation.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    public ReturnBlock(ILogger<ReturnBlock<C, V>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _executionStrategy = ExecutionStrategy.NoTransform;
    }

    /// <summary>
    /// Initializes a synchronous return block using a transformation function.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    /// <param name="func">A function that transforms the context.</param>
    public ReturnBlock(ILogger<ReturnBlock<C, V>> logger, Func<C, C> func)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _syncContextFunc = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.SyncContext;
    }

    /// <summary>
    /// Initializes a synchronous return block using a transformation function that also uses the context value.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    /// <param name="func">A function that transforms the context using its value.</param>
    public ReturnBlock(ILogger<ReturnBlock<C, V>> logger, Func<C, V, C> func)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _syncContextValueFunc = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.SyncContextValue;
    }

    /// <summary>
    /// Initializes an asynchronous return block using a transformation function.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    /// <param name="func">An asynchronous function that transforms the context.</param>
    public ReturnBlock(ILogger<ReturnBlock<C, V>> logger, Func<C, ValueTask<C>> func)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _asyncContextFunc = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.AsyncContext;
    }

    /// <summary>
    /// Initializes an asynchronous return block using a transformation function that also uses the context value.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    /// <param name="func">An asynchronous function that transforms the context using its value.</param>
    public ReturnBlock(ILogger<ReturnBlock<C, V>> logger, Func<C, V, ValueTask<C>> func)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _asyncContextValueFunc = func ?? throw new ArgumentNullException(nameof(func));
        _executionStrategy = ExecutionStrategy.AsyncContextValue;
    }

    /// <summary>
    /// Executes the block synchronously, transforming and finalizing the context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The transformed and finalized context.</returns>
    public C Execute(C context)
    {
        context.IsFinished = true;
        s_logContextTerminated(_logger, context.CorrelationId, null);

        return context.Value.Match(
            _ => context,
            x => ExecuteWithValue(context, x));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private C ExecuteWithValue(C context, V value)
    {
        return _executionStrategy switch
        {
            ExecutionStrategy.NoTransform => context,
            ExecutionStrategy.SyncContext => _syncContextFunc!(context),
            ExecutionStrategy.SyncContextValue => _syncContextValueFunc!(context, value),
            ExecutionStrategy.AsyncContext => GetResultFromAsync(_asyncContextFunc!(context)),
            ExecutionStrategy.AsyncContextValue => GetResultFromAsync(_asyncContextValueFunc!(context, value)),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };
    }

    /// <summary>
    /// Executes the block asynchronously, transforming and finalizing the context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>A task that represents the asynchronous operation, containing the updated context.</returns>
    public ValueTask<C> ExecuteAsync(C context)
    {
        context.IsFinished = true;
        s_logContextTerminated(_logger, context.CorrelationId, null);

        return context.Value.Match(
            _ => new ValueTask<C>(context), // If value is Some but we're not using it, just return context
            x => ExecuteWithValueAsync(context, x));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask<C> ExecuteWithValueAsync(C context, V value)
    {
        return _executionStrategy switch
        {
            ExecutionStrategy.NoTransform => new ValueTask<C>(context),
            ExecutionStrategy.SyncContext => new ValueTask<C>(_syncContextFunc!(context)),
            ExecutionStrategy.SyncContextValue => new ValueTask<C>(_syncContextValueFunc!(context, value)),
            ExecutionStrategy.AsyncContext => _asyncContextFunc!(context),
            ExecutionStrategy.AsyncContextValue => _asyncContextValueFunc!(context, value),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };
    }

    // Helper method to synchronously get result from async operation
    // This should only be used when we know the operation can be completed synchronously
    // or when we have no choice but to block (like in the sync Execute method)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static C GetResultFromAsync(ValueTask<C> task)
        => task.IsCompleted ? task.Result : AsyncContext.Run(task.AsTask);
}

/// <summary>
/// A no-op block that simply returns the provided context unchanged.
/// </summary>
/// <typeparam name="C">The type of context implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value held in the context.</typeparam>
public sealed class NoopBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    /// <summary>
    /// Synchronously returns the context as-is.
    /// </summary>
    /// <param name="context">The current context.</param>
    /// <returns>The unchanged context.</returns>
    public C Execute(C context) => context;

    /// <summary>
    /// Asynchronously returns the context as-is.
    /// </summary>
    /// <param name="context">The current context.</param>
    /// <returns>A task that represents the asynchronous operation, containing the unchanged context.</returns>
    public ValueTask<C> ExecuteAsync(C context) => ValueTask.FromResult(context);
}
