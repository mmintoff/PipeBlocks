using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks;

/// <summary>
/// Represents a mapped pipeline that transforms data from <typeparamref name="VRoot"/> through <typeparamref name="VPrev"/> to <typeparamref name="VOut"/>.
/// </summary>
/// <typeparam name="VRoot">The original input type of the entire pipeline.</typeparam>
/// <typeparam name="VPrev">The output type of the previous pipeline stage.</typeparam>
/// <typeparam name="VOut">The output type of this pipeline stage.</typeparam>
public sealed class MapPipeBlock<VRoot, VPrev, VOut> : IPipeBlock<VRoot, VOut>
{
    /// <summary>
    /// The <see cref="BlockBuilder{VOut}"/> used to resolve and create blocks.
    /// </summary>
    private readonly BlockBuilder<VOut> Builder;

    /// <summary>
    /// Gets the configuration options for this pipeline.
    /// </summary>
    public PipeBlockOptions Options => _options;

    private readonly string _vRootName = typeof(VRoot).Name,
                            _vPrevName = typeof(VPrev).Name,
                            _vOutName = typeof(VOut).Name;

    private readonly IPipeBlock<VRoot, VPrev> _previousPipeBlock;
    private readonly IBlock<VPrev, VOut> _mapBlock;

    /// <summary>
    /// The list of blocks that execute after the mapping.
    /// </summary>
    private readonly List<IBlock<VOut>> _blocks = [];
    private readonly PipeBlockOptions _options;
    private readonly ILogger<MapPipeBlock<VRoot, VPrev, VOut>> _logger;
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
    /// Initializes a new instance of the <see cref="MapPipeBlock{VRoot, VPrev, VOut}"/> class.
    /// </summary>
    /// <param name="previousPipeBlock">The pipeline that produces <typeparamref name="VPrev"/>.</param>
    /// <param name="mapBlock">The block that transforms <typeparamref name="VPrev"/> to <typeparamref name="VOut"/>.</param>
    /// <param name="blockBuilder">The builder used to resolve additional blocks.</param>
    internal MapPipeBlock
        (
            IPipeBlock<VRoot, VPrev> previousPipeBlock,
            IBlock<VPrev, VOut> mapBlock,
            BlockBuilder<VOut> blockBuilder
        )
    {
        _previousPipeBlock = previousPipeBlock;
        _mapBlock = mapBlock;

        _options = new PipeBlockOptions
        {
            HandleExceptions = _previousPipeBlock.Options.HandleExceptions,
            PipeName = $"[{_vRootName}]->[{_vOutName}] via [{_vPrevName}]"
        };

        Builder = blockBuilder;
        _logger = blockBuilder.CreateLogger<MapPipeBlock<VRoot, VPrev, VOut>>();
        s_createdPipe(_logger, _options.PipeName, null);
    }

    /// <summary>
    /// Executes the pipeline synchronously with the provided parameter and an optional context configuration.
    /// </summary>
    /// <param name="value">The parameter to execute the pipeline with.</param>
    /// <param name="configureContext">An optional action to configure the execution context before execution begins.</param>
    /// <returns>The updated parameter after pipeline execution.</returns>
    public Parameter<VOut> Execute(Parameter<VRoot> value, Action<Context>? configureContext)
    {
        s_logApplyingContextConfig(_logger, _options.PipeName, null);
        configureContext?.Invoke(value.Context);
        s_logAppliedContextConfig(_logger, _options.PipeName, null);
        return Execute(value);
    }

    /// <summary>
    /// Executes the pipeline synchronously with the provided parameter.
    /// </summary>
    /// <param name="value">The parameter to execute the pipeline with.</param>
    /// <returns>The updated parameter after pipeline execution.</returns>
    public Parameter<VOut> Execute(Parameter<VRoot> value)
    {
        var prevValue = _previousPipeBlock.Execute(value);
        s_sync_logExecutingPipe(_logger, _options.PipeName, value.CorrelationId, null);
        var newValue = BlockExecutor.ExecuteSync(_mapBlock, prevValue);

        if (_hasContextConstants)
        {
            _options.ConfigureContextConstants!(value.Context);
        }

        for (int i = 0; i < _blocks.Count; i++)
        {
            if (IsFinished(newValue))
            {
                s_sync_logStoppingPipe(_logger, _options.PipeName, i, newValue.CorrelationId, null);
                break;
            }
            newValue = BlockExecutor.ExecuteSync(_blocks[i], newValue, _options.HandleExceptions);
        }
        s_sync_logCompletedPipe(_logger, _options.PipeName, newValue.CorrelationId, null);
        return newValue;
    }

    /// <summary>
    /// Executes the pipeline asynchronously with the provided parameter and an optional context configuration.
    /// </summary>
    /// <param name="value">The parameter to execute the pipeline with.</param>
    /// <param name="configureContext">An optional action to configure the execution context before execution begins.</param>
    /// <returns>A task representing the asynchronous operation, returning the updated parameter.</returns>
    public ValueTask<Parameter<VOut>> ExecuteAsync(Parameter<VRoot> value, Action<Context>? configureContext)
    {
        s_logApplyingContextConfig(_logger, _options.PipeName, null);
        configureContext?.Invoke(value.Context);
        s_logAppliedContextConfig(_logger, _options.PipeName, null);
        return ExecuteAsync(value);
    }

    /// <summary>
    /// Executes the pipeline asynchronously with the provided parameter.
    /// </summary>
    /// <param name="value">The parameter to execute the pipeline with.</param>
    /// <returns>A task representing the asynchronous operation, returning the updated parameter.</returns>
    public async ValueTask<Parameter<VOut>> ExecuteAsync(Parameter<VRoot> value)
    {
        var prevValue = await _previousPipeBlock.ExecuteAsync(value);
        s_async_logExecutingPipe(_logger, _options.PipeName, value.CorrelationId, null);
        var newValue = await BlockExecutor.ExecuteAsync(_mapBlock, prevValue);

        if (_hasContextConstants)
        {
            _options.ConfigureContextConstants!(value.Context);
        }

        for (int i = 0; i < _blocks.Count; i++)
        {
            if (IsFinished(newValue))
            {
                s_async_logStoppingPipe(_logger, _options.PipeName, i, newValue.CorrelationId, null);
                break;
            }
            newValue = await BlockExecutor.ExecuteAsync(_blocks[i], newValue);
        }
        s_async_logCompletedPipe(_logger, _options.PipeName, newValue.CorrelationId, null);
        return newValue;
    }

    /// <summary>
    /// Adds a block to the internal list and logs the operation.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The current instance.</returns>
    private MapPipeBlock<VRoot, VPrev, VOut> AddBlock(IBlock<VOut> block)
    {
        _blocks.Add(block);
        s_logAddBlock(_logger, block.ToString() ?? "Unknown", _options.PipeName, null);
        return this;
    }

    private static bool IsFinished(Parameter<VOut> value) => value.Context.IsFlipped
        ? !(value.Context.IsFinished || value.IsFailure)
        : value.Context.IsFinished || value.IsFailure;

    /// <summary>
    /// Adds a block to the pipeline.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The current instance for chaining.</returns>
    public MapPipeBlock<VRoot, VPrev, VOut> Then(IBlock<VOut> block)
        => AddBlock(block);

    /// <summary>
    /// Resolves and adds a block of the specified type to the pipeline.
    /// </summary>
    /// <typeparam name="X">The block type to resolve and add.</typeparam>
    /// <returns>The current instance for chaining.</returns>
    public MapPipeBlock<VRoot, VPrev, VOut> Then<X>()
        where X : IBlock<VOut>
        => AddBlock(Builder.ResolveInstance<X>());

    /// <summary>
    /// Adds a block to the pipeline using a builder function.
    /// </summary>
    /// <param name="func">A function that takes a builder and returns a block.</param>
    /// <returns>The current instance for chaining.</returns>
    public MapPipeBlock<VRoot, VPrev, VOut> Then(Func<BlockBuilder<VOut>, IBlock<VOut>> func)
        => AddBlock(func(Builder));

    /// <summary>
    /// Begins a mapping operation to type <typeparamref name="VNext"/>.
    /// </summary>
    /// <typeparam name="VNext">The target type to map to.</typeparam>
    public Mapper<VRoot, VOut, VNext> Map<VNext>() => new(this, Builder);
}