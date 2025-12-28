using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that executes a series of blocks starting from a specific step, based on a function that determines the start step.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="StartFromPipeBlock{C, V}"/> class.
/// </remarks>
/// <param name="pipeName">The name of the pipe.</param>
/// <param name="startStepFunc">The function to determine the step to start from.</param>
/// <param name="blockBuilder">The block builder used to resolve blocks.</param>
public partial class StartFromPipeBlock<V>(
        string pipeName,
        Func<Parameter<V>, int> startStepFunc,
        BlockBuilder<V> blockBuilder
        ) : ISyncBlock<V>, IAsyncBlock<V>
{
    private readonly List<IBlock<V>> _blocks = [];
    private readonly ILogger<StartFromPipeBlock<V>> _logger = blockBuilder.CreateLogger<StartFromPipeBlock<V>>();

    /// <summary>
    /// Executes the blocks synchronously, starting from a specified step, and returns the modified context.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>The modified context after executing the blocks.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        int i = startStepFunc(value);
        _logger.LogTrace("Executing pipe: '{name}' synchronously for context: {CorrelationId}, starting from: {step}", pipeName, value.Context.CorrelationId, i);
        for (; i < _blocks.Count; i++)
        {
            if (IsFinished(value))
            {
                _logger.LogTrace("Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}", pipeName, i, value.Context.CorrelationId);
                break;
            }

            value = BlockExecutor.ExecuteSync(_blocks[i], value);
        }
        _logger.LogTrace("Completed synchronous pipe: '{name}' execution for context: {CorrelationId}", pipeName, value.Context.CorrelationId);
        return value;
    }

    /// <summary>
    /// Executes the blocks asynchronously, starting from a specified step, and returns the modified context.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the modified context after execution.</returns>
    public async ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        int i = startStepFunc(value);
        _logger.LogTrace("Executing pipe: '{name}' asynchronously for context: {CorrelationId}, starting from: {step}", pipeName, value.Context.CorrelationId, i);
        for (; i < _blocks.Count; i++)
        {
            if (IsFinished(value))
            {
                _logger.LogTrace("Stopping asynchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}", pipeName, i, value.Context.CorrelationId);
                break;
            }

            value = await BlockExecutor.ExecuteAsync(_blocks[i], value);
        }
        _logger.LogTrace("Completed asynchronous pipe: '{name}' execution for context: {CorrelationId}", pipeName, value.Context.CorrelationId);
        return value;
    }

    /// <summary>
    /// Converts the current <see cref="StartFromPipeBlock{C, V}"/> into a synchronous function.
    /// </summary>
    /// <returns>A function that executes the block synchronously.</returns>
    public PipeBlockDelegate<V> ToDelegate() => Execute;

    /// <summary>
    /// Converts the current <see cref="StartFromPipeBlock{C, V}"/> into an asynchronous function.
    /// </summary>
    /// <returns>A function that executes the block asynchronously.</returns>
    public PipeBlockAsyncDelegate<V> ToAsyncDelegate() => ExecuteAsync;

    /// <summary>
    /// Adds a new block to the pipe to be executed after the current blocks.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The updated <see cref="StartFromPipeBlock{C, V}"/> instance.</returns>
    public StartFromPipeBlock<V> Then(IBlock<V> block)
        => AddBlock(block);

    /// <summary>
    /// Adds a new block to the pipe to be executed after the current blocks, based on a resolved instance.
    /// </summary>
    /// <typeparam name="X">The type of block to resolve and add.</typeparam>
    /// <returns>The updated <see cref="StartFromPipeBlock{C, V}"/> instance.</returns>
    public StartFromPipeBlock<V> Then<X>()
        where X : IBlock<V>
        => AddBlock(blockBuilder.ResolveInstance<X>());

    /// <summary>
    /// Adds a new block to the pipe to be executed after the current blocks, based on a function that resolves the block.
    /// </summary>
    /// <param name="func">The function that resolves the block to add.</param>
    /// <returns>The updated <see cref="StartFromPipeBlock{C, V}"/> instance.</returns>
    public StartFromPipeBlock<V> Then(Func<BlockBuilder<V>, IBlock<V>> func)
        => AddBlock(func(blockBuilder));

    private StartFromPipeBlock<V> AddBlock(IBlock<V> block)
    {
        _blocks.Add(block);
        _logger.LogTrace("Added block: '{Type}' to pipe: '{name}'", block.ToString(), pipeName);
        return this;
    }

    private static bool IsFinished(Parameter<V> value) => value.Context.IsFlipped
        ? !(value.Context.IsFinished || IsFailure(value))
        : value.Context.IsFinished || IsFailure(value);

    private static bool IsFailure(Parameter<V> value) => value.Match(
        _ => true,
        _ => false);

    public override string ToString() => pipeName;
}

/// <summary>
/// Extension methods for creating a StartFromPipeBlock.
/// </summary>
public static partial class BuilderExtensions
{
    /// <summary>
    /// Creates a new <see cref="StartFromPipeBlock{C, V}"/> with a specified start step function.
    /// </summary>
    /// <typeparam name="C">The context type.</typeparam>
    /// <typeparam name="V">The value type associated with the context.</typeparam>
    /// <param name="blockBuilder">The block builder used to resolve the blocks.</param>
    /// <param name="pipeName">The name of the pipe.</param>
    /// <param name="startStepFunc">The function that determines the start step for execution.</param>
    /// <returns>A new <see cref="StartFromPipeBlock{C, V}"/> instance.</returns>
    public static StartFromPipeBlock<V> CreatePipe<V>(this BlockBuilder<V> blockBuilder,
        string pipeName,
        Func<Parameter<V>, int> startStepFunc)
        => new(pipeName, startStepFunc, blockBuilder);
}
