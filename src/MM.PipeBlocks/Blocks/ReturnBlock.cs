using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;

namespace MM.PipeBlocks.Blocks;
/// <summary>
/// A block that marks the context as finished and optionally transforms it before returning.
/// Supports both synchronous and asynchronous execution logic.
/// </summary>
/// <typeparam name="C">The type of context implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value held in the context.</typeparam>
public sealed class ReturnBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    private readonly bool _isSyncFunc;
    private readonly Either<Func<C, C>, Func<C, V, C>>? _func;
    private readonly Either<Func<C, ValueTask<C>>, Func<C, V, ValueTask<C>>>? _asyncFunc;
    private readonly ILogger<ReturnBlock<C, V>> _logger;

    /// <summary>
    /// Initializes a synchronous return block using a transformation function.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    /// <param name="func">A function that transforms the context.</param>
    public ReturnBlock(ILogger<ReturnBlock<C, V>> logger, Func<C, C> func)
        => (_logger, _func, _isSyncFunc) = (logger, func, true);

    /// <summary>
    /// Initializes a synchronous return block using a transformation function that also uses the context value.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    /// <param name="func">A function that transforms the context using its value.</param>
    public ReturnBlock(ILogger<ReturnBlock<C, V>> logger, Func<C, V, C> func)
        => (_logger, _func, _isSyncFunc) = (logger, func, true);

    /// <summary>
    /// Initializes an asynchronous return block using a transformation function.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    /// <param name="func">An asynchronous function that transforms the context.</param>
    public ReturnBlock(ILogger<ReturnBlock<C, V>> logger, Func<C, ValueTask<C>> func)
        => (_logger, _asyncFunc, _isSyncFunc) = (logger, func, false);

    /// <summary>
    /// Initializes an asynchronous return block using a transformation function that also uses the context value.
    /// </summary>
    /// <param name="logger">The logger for diagnostics and tracing.</param>
    /// <param name="func">An asynchronous function that transforms the context using its value.</param>
    public ReturnBlock(ILogger<ReturnBlock<C, V>> logger, Func<C, V, ValueTask<C>> func)
        => (_logger, _asyncFunc, _isSyncFunc) = (logger, func, false);

    /// <summary>
    /// Executes the block synchronously, transforming and finalizing the context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The transformed and finalized context.</returns>
    public C Execute(C context)
    {
        context.IsFinished = true;
        _logger.LogInformation("Context {CorrelationId} terminated in Return Block", context.CorrelationId);
        return context.Value.Match(
            _ => context,
            value => _isSyncFunc
                    ? _func!.Match(
                        f => f(context),
                        f => f(context, value))
                    : AsyncContext.Run(async () => await _asyncFunc!.MatchAsync(
                        f => f(context),
                        f => f(context, value)))
        );
    }

    /// <summary>
    /// Executes the block asynchronously, transforming and finalizing the context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>A task that represents the asynchronous operation, containing the updated context.</returns>
    public async ValueTask<C> ExecuteAsync(C context)
    {
        context.IsFinished = true;
        _logger.LogInformation("Context {CorrelationId} terminated in Return Block", context.CorrelationId);
        return await context.Value.MatchAsync(
            _ => ValueTask.FromResult(context),
            value => _isSyncFunc
                        ? ValueTask.FromResult(_func!.Match(
                            f => f(context),
                            f => f(context, value)))
                        : _asyncFunc!.MatchAsync(
                            f => f(context),
                            f => f(context, value))
        );
    }
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
