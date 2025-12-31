using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MM.PipeBlocks.Abstractions;
using System.Runtime.CompilerServices;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that executes a series of blocks starting from a specific step, based on a function that determines the start step.
/// </summary>
/// <typeparam name="V">The value type associated with the parameter.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="StartFromPipeBlock{V}"/> class.
/// </remarks>
public partial class StartFromPipeBlock<V> : IPipeBlock<V>
{
    /// <summary>
    /// The <see cref="BlockBuilder{V}"/> used to resolve and create blocks.
    /// </summary>
    protected readonly BlockBuilder<V> Builder;

    private readonly List<IBlock<V>> _blocks = [];
    private readonly PipeBlockOptions _options;
    private readonly Func<Parameter<V>, int> _startStepFunc;
    private readonly ILogger<StartFromPipeBlock<V>> _logger;
    private readonly bool _hasContextConstants;

    private static readonly Action<ILogger, string, string, Exception?> s_logAddBlock = LoggerMessage.Define<string, string>(LogLevel.Trace, default, "Added block: '{Type}' to pipe: '{name}'");
    private static readonly Action<ILogger, string, Exception?> s_logApplyingContextConfig = LoggerMessage.Define<string>(LogLevel.Trace, default, "Applying context configuration for pipe: '{name}'");
    private static readonly Action<ILogger, string, Exception?> s_logAppliedContextConfig = LoggerMessage.Define<string>(LogLevel.Trace, default, "Applied context configuration for pipe: '{name}'");

    private static readonly Action<ILogger, string, Guid, int, Exception?> s_sync_logExecutingPipe = LoggerMessage.Define<string, Guid, int>(LogLevel.Trace, default, "Executing pipe: '{PipeName}' synchronously for context: {CorrelationId}, starting from: {step}");
    private static readonly Action<ILogger, string, int, Guid, Exception?> s_sync_logStoppingPipe = LoggerMessage.Define<string, int, Guid>(LogLevel.Trace, default, "Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}");
    private static readonly Action<ILogger, string, Guid, Exception?> s_sync_logCompletedPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Completed synchronous pipe: '{name}' execution for context: {CorrelationId}");

    private static readonly Action<ILogger, string, Guid, int, Exception?> s_async_logExecutingPipe = LoggerMessage.Define<string, Guid, int>(LogLevel.Trace, default, "Executing pipe: '{PipeName}' asynchronously for context: {CorrelationId}, starting from: {step}");
    private static readonly Action<ILogger, string, int, Guid, Exception?> s_async_logStoppingPipe = LoggerMessage.Define<string, int, Guid>(LogLevel.Trace, default, "Stopping asynchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}");
    private static readonly Action<ILogger, string, Guid, Exception?> s_async_logCompletedPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Completed asynchronous pipe: '{name}' execution for context: {CorrelationId}");

    /// <summary>
    /// Initializes a new instance of the StartFromPipeBlock class with the specified options, start step function, and block builder.
    /// </summary>
    /// <param name="options">The options used to configure the pipe block. Cannot be null.</param>
    /// <param name="startStepFunc">A function that determines the starting step index based on the provided parameter.</param>
    /// <param name="blockBuilder">The block builder used to construct and configure the pipe block. Cannot be null.</param>
    public StartFromPipeBlock
        (
            IOptions<PipeBlockOptions> options,
            Func<Parameter<V>, int> startStepFunc,
            BlockBuilder<V> blockBuilder
        )
    {
        _options = options.Value;
        _hasContextConstants = _options.ConfigureContextConstants != null;
        _startStepFunc = startStepFunc;
        Builder = blockBuilder;
        _logger = blockBuilder.CreateLogger<StartFromPipeBlock<V>>();
        _logger.LogInformation("Created pipe: '{name}'", _options.PipeName);
    }

    /// <summary>
    /// Executes the blocks synchronously, starting from a specified step, with an optional context configuration.
    /// </summary>
    /// <param name="value">The parameter to execute.</param>
    /// <param name="configureContext">An optional action to configure the execution context before execution begins.</param>
    /// <returns>The modified parameter after executing the blocks.</returns>
    public Parameter<V> Execute(Parameter<V> value, Action<Context>? configureContext)
    {
        s_logApplyingContextConfig(_logger, _options.PipeName, null);
        configureContext?.Invoke(value.Context);
        s_logAppliedContextConfig(_logger, _options.PipeName, null);
        return Execute(value);
    }

    /// <summary>
    /// Executes the blocks synchronously, starting from a specified step, and returns the modified parameter.
    /// </summary>
    /// <param name="value">The parameter to execute.</param>
    /// <returns>The modified parameter after executing the blocks.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        int i = _startStepFunc(value);
        s_sync_logExecutingPipe(_logger, _options.PipeName, value.Context.CorrelationId, i, null);
        if (_hasContextConstants)
        {
            _options.ConfigureContextConstants!(value.Context);
        }

        for (; i < _blocks.Count; i++)
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
    /// Executes the blocks asynchronously, starting from a specified step, with an optional context configuration.
    /// </summary>
    /// <param name="value">The parameter to execute.</param>
    /// <param name="configureContext">An optional action to configure the execution context before execution begins.</param>
    /// <returns>A task representing the asynchronous operation, with the modified parameter after execution.</returns>
    public ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value, Action<Context>? configureContext)
    {
        s_logApplyingContextConfig(_logger, _options.PipeName, null);
        configureContext?.Invoke(value.Context);
        s_logAppliedContextConfig(_logger, _options.PipeName, null);
        return ExecuteAsync(value);  // Return the task directly, no await
    }

    /// <summary>
    /// Executes the blocks asynchronously, starting from a specified step, and returns the modified parameter.
    /// </summary>
    /// <param name="value">The parameter to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the modified parameter after execution.</returns>
    public async ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        int i = _startStepFunc(value);
        s_async_logExecutingPipe(_logger, _options.PipeName, value.Context.CorrelationId, i, null);
        if (_hasContextConstants)
        {
            _options.ConfigureContextConstants!(value.Context);
        }

        for (; i < _blocks.Count; i++)
        {
            if (IsFinished(value))
            {
                s_async_logStoppingPipe(_logger, _options.PipeName, i, value.Context.CorrelationId, null);
                break;
            }

            value = await BlockExecutor.ExecuteAsync(_blocks[i], value, _options.HandleExceptions);
        }
        s_async_logCompletedPipe(_logger, _options.PipeName, value.Context.CorrelationId, null);
        return value;
    }

    /// <summary>
    /// Adds a new block to the pipe to be executed after the current blocks.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The updated <see cref="StartFromPipeBlock{V}"/> instance.</returns>
    public StartFromPipeBlock<V> Then(IBlock<V> block)
        => AddBlock(block);

    /// <summary>
    /// Adds a new block to the pipe to be executed after the current blocks, based on a resolved instance.
    /// </summary>
    /// <typeparam name="X">The type of block to resolve and add.</typeparam>
    /// <returns>The updated <see cref="StartFromPipeBlock{V}"/> instance.</returns>
    public StartFromPipeBlock<V> Then<X>()
        where X : IBlock<V>
        => AddBlock(Builder.ResolveInstance<X>());

    /// <summary>
    /// Adds a new block to the pipe to be executed after the current blocks, based on a function that resolves the block.
    /// </summary>
    /// <param name="func">The function that resolves the block to add.</param>
    /// <returns>The updated <see cref="StartFromPipeBlock{V}"/> instance.</returns>
    public StartFromPipeBlock<V> Then(Func<BlockBuilder<V>, IBlock<V>> func)
        => AddBlock(func(Builder));

    private StartFromPipeBlock<V> AddBlock(IBlock<V> block)
    {
        _blocks.Add(block);
        s_logAddBlock(_logger, block.ToString() ?? "Unknown", _options.PipeName, null);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFinished(Parameter<V> value) => value.Context.IsFlipped
        ? !(value.Context.IsFinished || IsFailure(value))
        : value.Context.IsFinished || IsFailure(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFailure(Parameter<V> value) => value.Match(
        _ => true,
        _ => false);

    public override string ToString() => _options.PipeName;
}

/// <summary>
/// Extension methods for creating a StartFromPipeBlock.
/// </summary>
public static partial class BuilderExtensions
{
    /// <summary>
    /// Creates a new <see cref="StartFromPipeBlock{V}"/> with a specified start step function.
    /// </summary>
    /// <typeparam name="V">The value type associated with the parameter.</typeparam>
    /// <param name="blockBuilder">The block builder used to resolve the blocks.</param>
    /// <param name="options">The options used to configure the pipe block.</param>
    /// <param name="startStepFunc">The function that determines the start step for execution.</param>
    /// <returns>A new <see cref="StartFromPipeBlock{V}"/> instance.</returns>
    public static StartFromPipeBlock<V> CreatePipe<V>(this BlockBuilder<V> blockBuilder,
        IOptions<PipeBlockOptions> options,
        Func<Parameter<V>, int> startStepFunc)
        => new(options, startStepFunc, blockBuilder);
}
