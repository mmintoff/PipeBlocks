using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MM.PipeBlocks.Abstractions;
using System.Runtime.CompilerServices;

namespace MM.PipeBlocks;
/// <summary>
/// Represents a pipeline of blocks that execute sequentially in either synchronous or asynchronous fashion.
/// </summary>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public partial class PipeBlock<V> : IPipeBlock<V>
{
    /// <summary>
    /// The <see cref="BlockBuilder{V}"/> used to resolve and create blocks.
    /// </summary>
    protected readonly BlockBuilder<V> Builder;

    public PipeBlockOptions Options => _options;

    /// <summary>
    /// The list of blocks that make up the pipeline.
    /// </summary>
    protected readonly List<IBlock<V>> _blocks = [];

    private readonly PipeBlockOptions _options;
    private readonly ILogger<PipeBlock<V>> _logger;
    private readonly bool _hasContextConstants;

    private static readonly Action<ILogger, string, Exception?> s_createdPipe = LoggerMessage.Define<string>(LogLevel.Information, default, "Created pipe: '{PipeName}'");

    private static readonly Action<ILogger, string, string, Exception?> s_logAddBlock = LoggerMessage.Define<string, string>(LogLevel.Trace, default, "Added block: '{Type}' to pipe: '{name}'");
    private static readonly Action<ILogger, string, Exception?> s_logApplyingContextConfig = LoggerMessage.Define<string>(LogLevel.Trace, default, "Applying context configuration for pipe: '{name}'");
    private static readonly Action<ILogger, string, Exception?> s_logAppliedContextConfig = LoggerMessage.Define<string>(LogLevel.Trace, default, "Applied context configuration for pipe: '{name}'");

    private static readonly Action<ILogger, string, Guid, Exception?> s_sync_logExecutingPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Executing pipe: '{PipeName}' synchronously for context: {CorrelationId}");
    private static readonly Action<ILogger, string, int, Guid, Exception?> s_sync_logStoppingPipe = LoggerMessage.Define<string, int, Guid>(LogLevel.Trace, default, "Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}");
    private static readonly Action<ILogger, string, Guid, Exception?> s_sync_logCompletedPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Completed synchronous pipe: '{name}' execution for context: {CorrelationId}");

    private static readonly Action<ILogger, string, Guid, Exception?> s_async_logExecutingPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Executing pipe: '{PipeName}' asynchronously for context: {CorrelationId}");
    private static readonly Action<ILogger, string, int, Guid, Exception?> s_async_logStoppingPipe = LoggerMessage.Define<string, int, Guid>(LogLevel.Trace, default, "Stopping asynchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}");
    private static readonly Action<ILogger, string, Guid, Exception?> s_async_logCompletedPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Completed asynchronous pipe: '{name}' execution for context: {CorrelationId}");

    /// <summary>
    /// Initializes a new instance of the <see cref="PipeBlock{V}"/> class with a given name and builder.
    /// </summary>
    /// <param name="options">The options containing the pipe configuration.</param>
    /// <param name="blockBuilder">The builder used to resolve additional blocks.</param>
    public PipeBlock(IOptions<PipeBlockOptions> options, BlockBuilder<V> blockBuilder)
    {
        _options = options.Value;
        _hasContextConstants = _options.ConfigureContextConstants != null;
        Builder = blockBuilder;
        _logger = blockBuilder.CreateLogger<PipeBlock<V>>();
        s_createdPipe(_logger, _options.PipeName, null);
    }

    /// <summary>
    /// Executes the pipeline synchronously with the provided parameter and an optional context configuration.
    /// </summary>
    /// <param name="value">The parameter to execute the pipeline with.</param>
    /// <param name="configureContext">An optional action to configure the execution context before execution begins.</param>
    /// <returns>The updated parameter after pipeline execution.</returns>
    public Parameter<V> Execute(Parameter<V> value, Action<Context>? configureContext)
    {
        s_logApplyingContextConfig(_logger, _options.PipeName, null);
        configureContext?.Invoke(value.Context);
        s_logAppliedContextConfig(_logger, _options.PipeName, null);
        return Execute(value);
    }

    /// <summary>
    /// Executes the pipeline synchronously with the provided context.
    /// </summary>
    /// <param name="value">The execution context.</param>
    /// <returns>The updated context after pipeline execution.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        s_sync_logExecutingPipe(_logger, _options.PipeName, value.Context.CorrelationId, null);
        if(_hasContextConstants && !IsFinished(value))
        {
            _options.ConfigureContextConstants!(value.Context);
        }

        for (int i = 0; i < _blocks.Count; i++)
        {
            if (IsFinished(value))
            {
                s_sync_logStoppingPipe(_logger, _options.PipeName, i, value.Context.CorrelationId, null);
                break;
            }

            value = BlockExecutor.ExecuteSync(_blocks[i], value, _options.HandleExceptions);
        }
        s_sync_logCompletedPipe(_logger, _options.PipeName, value.Context.CorrelationId, null);
        return value;
    }

    /// <summary>
    /// Executes the pipeline asynchronously with the provided parameter and an optional context configuration.
    /// </summary>
    /// <param name="value">The parameter to execute the pipeline with.</param>
    /// <param name="configureContext">An optional action to configure the execution context before execution begins.</param>
    /// <returns>A task representing the asynchronous operation, returning the updated parameter.</returns>
    public ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value, Action<Context>? configureContext)
    {
        s_logApplyingContextConfig(_logger, _options.PipeName, null);
        configureContext?.Invoke(value.Context);
        s_logAppliedContextConfig(_logger, _options.PipeName, null);
        return ExecuteAsync(value);  // Return the task directly, no await
    }

    /// <summary>
    /// Executes the pipeline asynchronously with the provided context.
    /// </summary>
    /// <param name="value">The execution context.</param>
    /// <returns>A task representing the asynchronous operation, returning the updated context.</returns>
    public async ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        s_async_logExecutingPipe(_logger, _options.PipeName, value.Context.CorrelationId, null);
        if (_hasContextConstants && !IsFinished(value))
        {
            _options.ConfigureContextConstants!(value.Context);
        }

        for (int i = 0; i < _blocks.Count; i++)
        {
            if (IsFinished(value))
            {
                s_async_logStoppingPipe(_logger, _options.PipeName, i, value.Context.CorrelationId, null);
                break;
            }

            var task = BlockExecutor.ExecuteAsync(_blocks[i], value, _options.HandleExceptions);
            value = task.IsCompletedSuccessfully ? task.Result : await task;
        }
        s_async_logCompletedPipe(_logger, _options.PipeName, value.Context.CorrelationId, null);
        return value;
    }

    /// <summary>
    /// Adds a block to the pipeline.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The current instance for chaining.</returns>
    public PipeBlock<V> Then(IBlock<V> block)
        => AddBlock(block);

    /// <summary>
    /// Resolves and adds a block of the specified type to the pipeline.
    /// </summary>
    /// <typeparam name="X">The block type to resolve and add.</typeparam>
    /// <returns>The current instance for chaining.</returns>
    public PipeBlock<V> Then<X>()
        where X : IBlock<V>
        => AddBlock(Builder.ResolveInstance<X>());

    /// <summary>
    /// Adds a block to the pipeline using a builder function.
    /// </summary>
    /// <param name="func">A function that takes a builder and returns a block.</param>
    /// <returns>The current instance for chaining.</returns>
    public PipeBlock<V> Then(Func<BlockBuilder<V>, IBlock<V>> func)
        => AddBlock(func(Builder));

    /// <summary>
    /// Returns the name of the pipe.
    /// </summary>
    public override string ToString() => _options.PipeName;

    /// <summary>
    /// Adds a block to the internal list and logs the operation.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The current instance.</returns>
    protected PipeBlock<V> AddBlock(IBlock<V> block)
    {
        _blocks.Add(block);
        s_logAddBlock(_logger, block.ToString() ?? "Unknown", _options.PipeName, null);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFinished(Parameter<V> value) => value.Context.IsFlipped
        ? !(value.Context.IsFinished || value.IsFailure)
        : value.Context.IsFinished || value.IsFailure;

    /// <summary>
    /// Begins a mapping operation to transform the pipeline output to type <typeparamref name="VNext"/>.
    /// Returns a <see cref="Mapper{V, VNext}"/> that requires a block to be specified via <see cref="Mapper{V, VNext}.Via{X}"/> to complete the mapping.
    /// </summary>
    /// <typeparam name="VNext">The target type to map to.</typeparam>
    /// <returns>A mapper configuration that must be completed by specifying a transformation block.</returns>
    public Mapper<V, VNext> Map<VNext>() => new(this, Builder);
}