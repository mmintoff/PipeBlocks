using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Extensions;

public sealed class MapPipeBlock<VRoot, VPrev, VOut> : IPipeBlock<VRoot, VOut>
{
    protected readonly BlockBuilder<VOut> Builder;
    public IBlockBuilder<VOut> BlockBuilder => Builder;

    private readonly string _vRootName = typeof(VRoot).Name,
                            _vPrevName = typeof(VPrev).Name,
                            _vOutName = typeof(VOut).Name;

    private readonly IPipeBlock<VRoot, VPrev> _previousPipeBlock;
    private readonly Func<Parameter<VPrev>, Parameter<VOut>> _map;

    private readonly List<IBlock<VOut>> _blocks = [];
    private readonly PipeBlockOptions _options;
    private readonly ILogger<MapPipeBlock<VRoot, VPrev, VOut>> _logger;
    private readonly bool _hasContextConstants;

    private static readonly Action<ILogger, string, string, string, string, Exception?> s_createdPipe = LoggerMessage.Define<string, string, string, string>(LogLevel.Information, default, "Created MapPipeBlock from {VRoot} to {VOut} via {VPrev} in pipe: '{PipeName}'");

    private static readonly Action<ILogger, string, string, Exception?> s_logAddBlock = LoggerMessage.Define<string, string>(LogLevel.Trace, default, "Added block: '{Type}' to pipe: '{name}'");
    private static readonly Action<ILogger, string, Exception?> s_logApplyingContextConfig = LoggerMessage.Define<string>(LogLevel.Trace, default, "Applying context configuration for pipe: '{name}'");
    private static readonly Action<ILogger, string, Exception?> s_logAppliedContextConfig = LoggerMessage.Define<string>(LogLevel.Trace, default, "Applied context configuration for pipe: '{name}'");

    private static readonly Action<ILogger, string, Guid, Exception?> s_sync_logExecutingPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Executing pipe: '{PipeName}' synchronously for context: {CorrelationId}");
    private static readonly Action<ILogger, string, int, Guid, Exception?> s_sync_logStoppingPipe = LoggerMessage.Define<string, int, Guid>(LogLevel.Trace, default, "Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}");
    private static readonly Action<ILogger, string, Guid, Exception?> s_sync_logCompletedPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Completed synchronous pipe: '{name}' execution for context: {CorrelationId}");

    private static readonly Action<ILogger, string, Guid, Exception?> s_async_logExecutingPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Executing pipe: '{PipeName}' asynchronously for context: {CorrelationId}");
    private static readonly Action<ILogger, string, int, Guid, Exception?> s_async_logStoppingPipe = LoggerMessage.Define<string, int, Guid>(LogLevel.Trace, default, "Stopping asynchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}");
    private static readonly Action<ILogger, string, Guid, Exception?> s_async_logCompletedPipe = LoggerMessage.Define<string, Guid>(LogLevel.Trace, default, "Completed asynchronous pipe: '{name}' execution for context: {CorrelationId}");

    public MapPipeBlock
        (
            IPipeBlock<VRoot, VPrev> previousPipeBlock,
            IOptions<PipeBlockOptions> options,
            Func<Parameter<VPrev>, Parameter<VOut>> map,
            BlockBuilder<VOut> blockBuilder
        )
    {
        _previousPipeBlock = previousPipeBlock;
        _options = options.Value;
        _map = map;
        Builder = blockBuilder;
        _logger = blockBuilder.CreateLogger<MapPipeBlock<VRoot, VPrev, VOut>>();
        s_createdPipe(_logger, _options.PipeName, _vRootName, _vOutName, _vPrevName, null);
    }

    public Parameter<VOut> Execute(Parameter<VRoot> value, Action<Context>? configureContext)
    {
        s_logApplyingContextConfig(_logger, _options.PipeName, null);
        configureContext?.Invoke(value.Context);
        s_logAppliedContextConfig(_logger, _options.PipeName, null);
        return Execute(value);
    }

    public Parameter<VOut> Execute(Parameter<VRoot> value)
    {
        var prevValue = _previousPipeBlock.Execute(value);
        s_sync_logExecutingPipe(_logger, _options.PipeName, value.CorrelationId, null);
        var newValue = _map(prevValue);

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

    public ValueTask<Parameter<VOut>> ExecuteAsync(Parameter<VRoot> value, Action<Context>? configureContext)
    {
        s_logApplyingContextConfig(_logger, _options.PipeName, null);
        configureContext?.Invoke(value.Context);
        s_logAppliedContextConfig(_logger, _options.PipeName, null);
        return ExecuteAsync(value);
    }

    public async ValueTask<Parameter<VOut>> ExecuteAsync(Parameter<VRoot> value)
    {
        var prevValue = await _previousPipeBlock.ExecuteAsync(value);
        s_async_logExecutingPipe(_logger, _options.PipeName, value.CorrelationId, null);
        var newValue = _map(prevValue);

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

    public MapPipeBlock<VRoot, VPrev, VOut> Then(IBlock<VOut> block)
        => AddBlock(block);

    public MapPipeBlock<VRoot, VPrev, VOut> Then<X>()
        where X : IBlock<VOut>
        => AddBlock(Builder.ResolveInstance<X>());

    public MapPipeBlock<VRoot, VPrev, VOut> Then(Func<BlockBuilder<VOut>, IBlock<VOut>> func)
        => AddBlock(func(Builder));

    private MapPipeBlock<VRoot, VPrev, VOut> AddBlock(IBlock<VOut> block)
    {
        _blocks.Add(block);
        s_logAddBlock(_logger, block.ToString() ?? "Unknown", _options.PipeName, null);
        return this;
    }

    private static bool IsFinished(Parameter<VOut> value) => value.Context.IsFlipped
        ? !(value.Context.IsFinished || value.IsFailure)
        : value.Context.IsFinished || value.IsFailure;
}

public static partial class PipeExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="VRoot"></typeparam>
    /// <typeparam name="VNext"></typeparam>
    /// <param name="pipe"></param>
    /// <param name="options"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public static MapPipeBlock<VRoot, VRoot, VNext> Map<VRoot, VNext>(this IPipeBlock<VRoot> pipe, IOptions<PipeBlockOptions> options, Func<Parameter<VRoot>, Parameter<VNext>> map)
        => new(pipe, options, map, (BlockBuilder<VNext>)pipe.BlockBuilder.CreateBlockBuilder<VNext>());

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="VRoot"></typeparam>
    /// <typeparam name="VOut"></typeparam>
    /// <typeparam name="VNext"></typeparam>
    /// <param name="pipe"></param>
    /// <param name="options"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    public static MapPipeBlock<VRoot, VOut, VNext> Map<VRoot, VOut, VNext>(this IPipeBlock<VRoot, VOut> pipe, IOptions<PipeBlockOptions> options, Func<Parameter<VOut>, Parameter<VNext>> map)
        => new(pipe, options, map, (BlockBuilder<VNext>)pipe.BlockBuilder.CreateBlockBuilder<VNext>());
}