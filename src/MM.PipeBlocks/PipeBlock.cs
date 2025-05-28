using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks;
/// <summary>
/// Represents a pipeline of blocks that execute sequentially in either synchronous or asynchronous fashion.
/// </summary>
/// <typeparam name="C">The context type implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public partial class PipeBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    /// <summary>
    /// The <see cref="BlockBuilder{C, V}"/> used to resolve and create blocks.
    /// </summary>
    protected readonly BlockBuilder<C, V> Builder;

    /// <summary>
    /// The list of blocks that make up the pipeline.
    /// </summary>
    protected readonly List<IBlock<C, V>> _blocks = [];

    private readonly string _pipeName;
    private readonly ILogger<PipeBlock<C, V>> _logger;

    private static readonly Action<ILogger, string, string, Exception?> s_logAddBlock = LoggerMessage.Define<string, string>(LogLevel.Trace, default, "Added block: '{Type}' to pipe: '{name}'");

    private static readonly Action<ILogger, string, Guid, Exception?> s_sync_logExecutingPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Executing pipe: '{PipeName}' synchronously for context: {CorrelationId}");
    private static readonly Action<ILogger, string, int, Guid, Exception?> s_sync_logStoppingPipe = LoggerMessage.Define<string, int, Guid>(LogLevel.Trace, default, "Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}");
    private static readonly Action<ILogger, string, Guid, Exception?> s_sync_logCompletedPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Completed synchronous pipe: '{name}' execution for context: {CorrelationId}");

    private static readonly Action<ILogger, string, Guid, Exception?> s_async_logExecutingPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Executing pipe: '{PipeName}' asynchronously for context: {CorrelationId}");
    private static readonly Action<ILogger, string, int, Guid, Exception?> s_async_logStoppingPipe = LoggerMessage.Define<string, int, Guid>(LogLevel.Trace, default, "Stopping asynchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}");
    private static readonly Action<ILogger, string, Guid, Exception?> s_async_logCompletedPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Completed asynchronous pipe: '{name}' execution for context: {CorrelationId}");

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeBlock{C, V}"/> class with a given name and builder.
    /// </summary>
    /// <param name="pipeName">The name of the pipe, used for logging.</param>
    /// <param name="blockBuilder">The builder used to resolve additional blocks.</param>
    public PipeBlock(string pipeName, BlockBuilder<C, V> blockBuilder)
    {
        _pipeName = pipeName;
        Builder = blockBuilder;
        _logger = blockBuilder.CreateLogger<PipeBlock<C, V>>();
        _logger.LogInformation("Created pipe: '{name}'", _pipeName);
    }

    /// <summary>
    /// Executes the pipeline synchronously with the provided context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>The updated context after pipeline execution.</returns>
    public C Execute(C context)
    {
        s_sync_logExecutingPipe(_logger, _pipeName, context.CorrelationId, null);
        for (int i = 0; i < _blocks.Count; i++)
        {
            if (IsFinished(context))
            {
                _logger.LogTrace("Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}", _pipeName, i, context.CorrelationId);
                s_sync_logStoppingPipe(_logger, _pipeName, i, context.CorrelationId, null);
                break;
            }

            context = BlockExecutor.ExecuteSync(_blocks[i], context);
        }
        s_sync_logCompletedPipe(_logger, _pipeName, context.CorrelationId, null);
        return context;
    }

    /// <summary>
    /// Executes the pipeline asynchronously with the provided context.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <returns>A task representing the asynchronous operation, returning the updated context.</returns>
    public async ValueTask<C> ExecuteAsync(C context)
    {
        s_async_logExecutingPipe(_logger, _pipeName, context.CorrelationId, null);
        for (int i = 0; i < _blocks.Count; i++)
        {
            if (IsFinished(context))
            {
                s_async_logStoppingPipe(_logger, _pipeName, i, context.CorrelationId, null);
                break;
            }

            context = await BlockExecutor.ExecuteAsync(_blocks[i], context);
        }
        s_async_logCompletedPipe(_logger, _pipeName, context.CorrelationId, null);
        return context;
    }

    /// <summary>
    /// Converts the current <see cref="PipeBlock{C, V}"/> into a synchronous function.
    /// </summary>
    /// <returns>A function that executes the block synchronously.</returns>
    public PipeBlockDelegate<C, V> ToDelegate() => Execute;

    /// <summary>
    /// Converts the current <see cref="PipeBlock{C, V}"/> into an asynchronous function.
    /// </summary>
    /// <returns>A function that executes the block asynchronously.</returns>
    public PipeBlockAsyncDelegate<C, V> ToAsyncDelegate() => ExecuteAsync;

    /// <summary>
    /// Adds a block to the pipeline.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The current instance for chaining.</returns>
    public PipeBlock<C, V> Then(IBlock<C, V> block)
        => AddBlock(block);

    /// <summary>
    /// Resolves and adds a block of the specified type to the pipeline.
    /// </summary>
    /// <typeparam name="X">The block type to resolve and add.</typeparam>
    /// <returns>The current instance for chaining.</returns>
    public PipeBlock<C, V> Then<X>()
        where X : IBlock<C, V>
        => AddBlock(Builder.ResolveInstance<X>());

    /// <summary>
    /// Adds a block to the pipeline using a builder function.
    /// </summary>
    /// <param name="func">A function that takes a builder and returns a block.</param>
    /// <returns>The current instance for chaining.</returns>
    public PipeBlock<C, V> Then(Func<BlockBuilder<C, V>, IBlock<C, V>> func)
        => AddBlock(func(Builder));

    /// <summary>
    /// Returns the name of the pipe.
    /// </summary>
    public override string ToString() => _pipeName;

    /// <summary>
    /// Adds a block to the internal list and logs the operation.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The current instance.</returns>
    protected PipeBlock<C, V> AddBlock(IBlock<C, V> block)
    {
        _blocks.Add(block);
        s_logAddBlock(_logger, block.ToString() ?? "Unknown", _pipeName, null);
        return this;
    }

    private static bool IsFinished(C c) => c.IsFlipped
        ? !(c.IsFinished || IsFailure(c))
        : c.IsFinished || IsFailure(c);

    private static bool IsFailure(C c) => c.Value.Match(
        _ => true,
        _ => false);
}