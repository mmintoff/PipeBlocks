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
public partial class StartFromPipeBlock<C, V>(
        string pipeName,
        Func<C, int> startStepFunc,
        BlockBuilder<C, V> blockBuilder
        ) : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    private readonly List<IBlock<C, V>> _blocks = [];
    private readonly ILogger<StartFromPipeBlock<C, V>> _logger = blockBuilder.CreateLogger<StartFromPipeBlock<C, V>>();

    /// <summary>
    /// Executes the blocks synchronously, starting from a specified step, and returns the modified context.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>The modified context after executing the blocks.</returns>
    public C Execute(C context)
    {
        int i = startStepFunc(context);
        _logger.LogTrace("Executing pipe: '{name}' synchronously for context: {CorrelationId}, starting from: {step}", pipeName, context.CorrelationId, i);
        for (; i < _blocks.Count; i++)
        {
            if (IsFinished(context))
            {
                _logger.LogTrace("Stopping synchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}", pipeName, i, context.CorrelationId);
                break;
            }

            context = BlockExecutor.ExecuteSync(_blocks[i], context);
        }
        _logger.LogTrace("Completed synchronous pipe: '{name}' execution for context: {CorrelationId}", pipeName, context.CorrelationId);
        return context;
    }

    /// <summary>
    /// Executes the blocks asynchronously, starting from a specified step, and returns the modified context.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the modified context after execution.</returns>
    public async ValueTask<C> ExecuteAsync(C context)
    {
        int i = startStepFunc(context);
        _logger.LogTrace("Executing pipe: '{name}' asynchronously for context: {CorrelationId}, starting from: {step}", pipeName, context.CorrelationId, i);
        for (; i < _blocks.Count; i++)
        {
            if (IsFinished(context))
            {
                _logger.LogTrace("Stopping asynchronous pipe: '{name}' execution at step: {Step} for context: {CorrelationId}", pipeName, i, context.CorrelationId);
                break;
            }

            context = await BlockExecutor.ExecuteAsync(_blocks[i], context);
        }
        _logger.LogTrace("Completed asynchronous pipe: '{name}' execution for context: {CorrelationId}", pipeName, context.CorrelationId);
        return context;
    }

    /// <summary>
    /// Converts the current <see cref="StartFromPipeBlock{C, V}"/> into a synchronous function.
    /// </summary>
    /// <returns>A function that executes the block synchronously.</returns>
    public PipeBlockDelegate<C, V> ToDelegate() => Execute;

    /// <summary>
    /// Converts the current <see cref="StartFromPipeBlock{C, V}"/> into an asynchronous function.
    /// </summary>
    /// <returns>A function that executes the block asynchronously.</returns>
    public PipeBlockAsyncDelegate<C, V> ToAsyncDelegate() => ExecuteAsync;

    /// <summary>
    /// Adds a new block to the pipe to be executed after the current blocks.
    /// </summary>
    /// <param name="block">The block to add.</param>
    /// <returns>The updated <see cref="StartFromPipeBlock{C, V}"/> instance.</returns>
    public StartFromPipeBlock<C, V> Then(IBlock<C, V> block)
        => AddBlock(block);

    /// <summary>
    /// Adds a new block to the pipe to be executed after the current blocks, based on a resolved instance.
    /// </summary>
    /// <typeparam name="X">The type of block to resolve and add.</typeparam>
    /// <returns>The updated <see cref="StartFromPipeBlock{C, V}"/> instance.</returns>
    public StartFromPipeBlock<C, V> Then<X>()
        where X : IBlock<C, V>
        => AddBlock(blockBuilder.ResolveInstance<X>());

    /// <summary>
    /// Adds a new block to the pipe to be executed after the current blocks, based on a function that resolves the block.
    /// </summary>
    /// <param name="func">The function that resolves the block to add.</param>
    /// <returns>The updated <see cref="StartFromPipeBlock{C, V}"/> instance.</returns>
    public StartFromPipeBlock<C, V> Then(Func<BlockBuilder<C, V>, IBlock<C, V>> func)
        => AddBlock(func(blockBuilder));

    private StartFromPipeBlock<C, V> AddBlock(IBlock<C, V> block)
    {
        _blocks.Add(block);
        _logger.LogTrace("Added block: '{Type}' to pipe: '{name}'", block.ToString(), pipeName);
        return this;
    }

    private static bool IsFinished(C c) => c.IsFlipped
        ? !(c.IsFinished || IsFailure(c))
        : c.IsFinished || IsFailure(c);

    private static bool IsFailure(C c) => c.Value.Match(
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
    public static StartFromPipeBlock<C, V> CreatePipe<C, V>(this BlockBuilder<C, V> blockBuilder,
        string pipeName,
        Func<C, int> startStepFunc)
        where C : IContext<V>
        => new(pipeName, startStepFunc, blockBuilder);
}
