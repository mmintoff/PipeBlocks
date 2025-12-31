using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that adapts parameters between two different value types, executes a series of blocks in the adapted parameter,
/// and then adapts the result back to the original value type.
/// </summary>
/// <typeparam name="V1">The original value type associated with the parameter.</typeparam>
/// <typeparam name="V2">The adapted value type associated with the parameter.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="AdapterPipeBlock{V1, V2}"/> class.
/// </remarks>
public sealed class AdapterPipeBlock<V1, V2> : IPipeBlock<V1>
{
    protected readonly BlockBuilder<V2> Builder;

    private readonly string _v1Name = typeof(V1).Name,
                            _v2Name = typeof(V2).Name;

    private readonly List<IBlock<V2>> _blocks = [];
    private readonly PipeBlockOptions _options;
    private readonly IAdapter<V1, V2> _adapter;
    private readonly ILogger<AdapterPipeBlock<V1, V2>> _logger;
    private readonly bool _hasContextConstants;

    private static readonly Action<ILogger, string, string, Exception?> s_switching_types = LoggerMessage.Define<string, string>(LogLevel.Trace, default, "Switching value from: {From} to: {To}");
    private static readonly Action<ILogger, string, string, Exception?> s_switched_types = LoggerMessage.Define<string, string>(LogLevel.Trace, default, "Switched value from: {From} to: {To}");

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
    /// Initializes a new instance of the AdapterPipeBlock class, configuring the pipe block to adapt data from type V1
    /// to type V2 using the specified adapter and block builder.
    /// </summary>
    /// <remarks>This constructor sets up logging for the adapter pipe block and applies any context constants
    /// configuration specified in the options. The adapter and block builder are required for data transformation and
    /// block construction, respectively.</remarks>
    /// <param name="options">The options used to configure the pipe block, including context constants and pipe name settings. Cannot be
    /// null.</param>
    /// <param name="adapter">The adapter responsible for transforming data from type V1 to type V2. Cannot be null.</param>
    /// <param name="blockBuilder">The block builder used to construct and configure the underlying pipe block for type V2. Cannot be null.</param>
    public AdapterPipeBlock
        (
            IOptions<PipeBlockOptions> options,
            IAdapter<V1, V2> adapter,
            BlockBuilder<V2> blockBuilder
        )
    {
        _options = options.Value;
        _hasContextConstants = _options.ConfigureContextConstants != null;
        _adapter = adapter;
        Builder = blockBuilder;
        _logger = blockBuilder.CreateLogger<AdapterPipeBlock<V1, V2>>();
        _logger.LogInformation("Created adapter pipe: '{name}' adapting from: {V1} to: {V2}", _options.PipeName, _v1Name, _v2Name);
    }

    /// <summary>
    /// Executes the blocks synchronously, starting from a specified step, with an optional context configuration.
    /// </summary>
    /// <param name="value">The parameter to execute.</param>
    /// <param name="configureContext">An optional action to configure the execution context before execution begins.</param>
    /// <returns>The modified parameter after executing the blocks.</returns>
    public Parameter<V1> Execute(Parameter<V1> value, Action<Context>? configureContext)
    {
        s_logApplyingContextConfig(_logger, _options.PipeName, null);
        configureContext?.Invoke(value.Context);
        s_logAppliedContextConfig(_logger, _options.PipeName, null);
        return Execute(value);
    }

    /// <summary>
    /// Executes the block synchronously, switching between parameters as necessary.
    /// </summary>
    /// <param name="value">The original parameter to execute.</param>
    /// <returns>The original parameter after execution, potentially modified.</returns>
    public Parameter<V1> Execute(Parameter<V1> value)
    {
        s_switching_types(_logger, _v1Name, _v2Name, null);
        var nValue = _adapter.Adapt(value);
        s_switched_types(_logger, _v1Name, _v2Name, null);
        s_sync_logExecutingPipe(_logger, _options.PipeName, nValue.Context.CorrelationId, null);
        if (_hasContextConstants)
        {
            _options.ConfigureContextConstants!(value.Context);
        }

        for (int i = 0; i < _blocks.Count; i++)
        {
            if (IsFinished(nValue))
            {
                s_sync_logStoppingPipe(_logger, _options.PipeName, i, nValue.Context.CorrelationId, null);
                break;
            }
            nValue = BlockExecutor.ExecuteSync(_blocks[i], nValue, _options.HandleExceptions);
        }
        s_switching_types(_logger, _v2Name, _v1Name, null);
        var result = _adapter.Adapt(nValue, value);
        s_switched_types(_logger, _v2Name, _v1Name, null);
        s_sync_logCompletedPipe(_logger, _options.PipeName, nValue.Context.CorrelationId, null);
        return result;
    }

    /// <summary>
    /// Executes the blocks asynchronously, starting from a specified step, with an optional context configuration.
    /// </summary>
    /// <param name="value">The parameter to execute.</param>
    /// <param name="configureContext">An optional action to configure the execution context before execution begins.</param>
    /// <returns>A task representing the asynchronous operation, with the modified parameter after execution.</returns>
    public ValueTask<Parameter<V1>> ExecuteAsync(Parameter<V1> value, Action<Context>? configureContext)
    {
        s_logApplyingContextConfig(_logger, _options.PipeName, null);
        configureContext?.Invoke(value.Context);
        s_logAppliedContextConfig(_logger, _options.PipeName, null);
        return ExecuteAsync(value);  // Return the task directly, no await
    }

    /// <summary>
    /// Executes the block asynchronously, switching between parameters as necessary.
    /// </summary>
    /// <param name="value">The original parameter to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the original parameter potentially modified.</returns>
    public async ValueTask<Parameter<V1>> ExecuteAsync(Parameter<V1> value)
    {
        s_switching_types(_logger, _v1Name, _v2Name, null);
        var nValue = _adapter.Adapt(value);
        s_switched_types(_logger, _v1Name, _v2Name, null);
        s_async_logExecutingPipe(_logger, _options.PipeName, nValue.CorrelationId, null);
        if (_hasContextConstants)
        {
            _options.ConfigureContextConstants!(value.Context);
        }

        for (int i = 0; i < _blocks.Count; i++)
        {
            if (IsFinished(nValue))
            {
                s_async_logStoppingPipe(_logger, _options.PipeName, i, nValue.CorrelationId, null);
                break;
            }
            nValue = await BlockExecutor.ExecuteAsync(_blocks[i], nValue, _options.HandleExceptions);
        }
        s_switching_types(_logger, _v2Name, _v1Name, null);
        var result = _adapter.Adapt(nValue, value);
        s_switched_types(_logger, _v2Name, _v1Name, null);
        s_async_logCompletedPipe(_logger, _options.PipeName, nValue.CorrelationId, null);
        return _adapter.Adapt(nValue);
    }

    /// <summary>
    /// Adds a block to the pipe to be executed after the current one.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The current <see cref="AdapterPipeBlock{V1, V2}"/> instance.</returns>
    public AdapterPipeBlock<V1, V2> Then(IBlock<V2> block)
        => AddBlock(block);

    /// <summary>
    /// Adds a block to the pipe to be executed after the current one.
    /// </summary>
    /// <typeparam name="X">The type of the block to add.</typeparam>
    /// <returns>The current <see cref="AdapterPipeBlock{V1, V2}"/> instance.</returns>
    public AdapterPipeBlock<V1, V2> Then<X>()
        where X : IBlock<V2>
        => AddBlock(Builder.ResolveInstance<X>());

    /// <summary>
    /// Adds a block to the pipe to be executed after the current one.
    /// </summary>
    /// <param name="func">A function that resolves the next block.</param>
    /// <returns>The current <see cref="AdapterPipeBlock{V1, V2}"/> instance.</returns>
    public AdapterPipeBlock<V1, V2> Then(Func<BlockBuilder<V2>, IBlock<V2>> func)
        => AddBlock(func(Builder));

    private AdapterPipeBlock<V1, V2> AddBlock(IBlock<V2> block)
    {
        _blocks.Add(block);
        s_logAddBlock(_logger, block.ToString() ?? "Unknown", _options.PipeName, null);
        return this;
    }

    private static bool IsFinished(Parameter<V2> value) => value.Context.IsFlipped
        ? !(value.Context.IsFinished || IsFailure(value))
        : value.Context.IsFinished || IsFailure(value);

    private static bool IsFailure(Parameter<V2> value) => value.Match(
        _ => true,
        _ => false);

    public override string ToString() => _options.PipeName;
}

/// <summary>
/// Defines an adapter that can convert between two different parameter value types.
/// </summary>
/// <typeparam name="V1">The original parameter value type.</typeparam>
/// <typeparam name="V2">The adapted parameter value type.</typeparam>
public interface IAdapter<V1, V2>
{
    /// <summary>
    /// Adapts a parameter with value type <typeparamref name="V1"/> to value type <typeparamref name="V2"/>.
    /// </summary>
    /// <param name="from">The parameter with value type <typeparamref name="V1"/> to adapt.</param>
    /// <param name="original">An optional original parameter of type <typeparamref name="V2"/> to use as a base for adaptation.</param>
    /// <returns>The adapted parameter with value type <typeparamref name="V2"/>.</returns>
    Parameter<V2> Adapt(Parameter<V1> from, Parameter<V2>? original = null);

    /// <summary>
    /// Adapts a parameter with value type <typeparamref name="V2"/> to value type <typeparamref name="V1"/>.
    /// </summary>
    /// <param name="from">The parameter with value type <typeparamref name="V2"/> to adapt.</param>
    /// <param name="original">An optional original parameter of type <typeparamref name="V1"/> to use as a base for adaptation.</param>
    /// <returns>The adapted parameter with value type <typeparamref name="V1"/>.</returns>
    Parameter<V1> Adapt(Parameter<V2> from, Parameter<V1>? original = null);
}

/// <summary>
/// Extension method to create a new <see cref="AdapterPipeBlock{V1, V2}"/> with the specified parameters.
/// </summary>
/// <returns>A new instance of <see cref="AdapterPipeBlock{V1, V2}"/>, which represents a pipeline that adapts between the two value types and can execute a series of blocks.</returns>
public static partial class BuilderExtensions
{
    /// <summary>
    /// Creates an <see cref="AdapterPipeBlock{V1, V2}"/> pipeline, starting with the provided <paramref name="pipeName"/>, 
    /// and setting up the adapter and block builder for transforming and processing the parameters.
    /// </summary>
    /// <param name="_">The block builder for resolving the blocks.</param>
    /// <param name="builder">The block builder for resolving the blocks.</param>
    /// <param name="pipeName">The name of the pipe.</param>
    /// <param name="adapter">The adapter responsible for transforming between source and target parameters.</param>
    /// <typeparam name="V1">The value type associated with the source parameter.</typeparam>
    /// <typeparam name="V2">The value type associated with the target parameter.</typeparam>
    /// <returns>A new instance of <see cref="AdapterPipeBlock{V1, V2}"/> configured with the provided parameters.</returns>
    public static AdapterPipeBlock<V1, V2> CreatePipe<V1, V2>(this IBlockBuilder<V1> builder,
        IOptions<PipeBlockOptions> options,
        IAdapter<V1, V2> adapter)
    => new(options, adapter, (BlockBuilder<V2>)builder.CreateBlockBuilder<V2>());
}