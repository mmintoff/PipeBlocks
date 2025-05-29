using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that executes a loop based on a condition, either performing a "Do" or "While" loop style.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
public sealed class LoopBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    private readonly IBlock<C, V> _block;
    private readonly Func<C, bool> _evaluator;
    private readonly LoopStyle _loopStyle;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopBlock{C, V}"/> class.
    /// </summary>
    /// <param name="block">The block to execute in the loop.</param>
    /// <param name="evaluator">A function that evaluates whether to continue looping.</param>
    /// <param name="loopStyle">The style of the loop, either "Do" or "While".</param>
    public LoopBlock(IBlock<C, V> block, Func<C, bool> evaluator, LoopStyle loopStyle)
        => (_block, _evaluator, _loopStyle)
        = (block, evaluator, loopStyle);

    /// <summary>
    /// Executes the block synchronously, looping until the evaluation function returns false.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>The modified context after loop execution.</returns>
    public C Execute(C context)
    {
        if (_loopStyle == LoopStyle.Do)
            context = BlockExecutor.ExecuteSync(_block, context);
        while (_evaluator(context))
            context = BlockExecutor.ExecuteSync(_block, context);

        return context;
    }

    /// <summary>
    /// Executes the block asynchronously, looping until the evaluation function returns false.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>A task that represents the asynchronous operation, with the modified context after loop execution.</returns>
    public async ValueTask<C> ExecuteAsync(C context)
    {
        if (_loopStyle == LoopStyle.Do)
            context = await BlockExecutor.ExecuteAsync(_block, context);
        while (_evaluator(context))
            context = await BlockExecutor.ExecuteAsync(_block, context);

        return context;
    }
}

/// <summary>
/// Specifies the type of loop to execute: a "Do" or "While" loop.
/// </summary>
public enum LoopStyle
{
    /// <summary>
    /// Executes the block once before checking the condition, and repeats as long as the condition is true.
    /// </summary>
    Do,

    /// <summary>
    /// Checks the condition before executing the block, and repeats as long as the condition is true.
    /// </summary>
    While
}

/// <summary>
/// A builder class for constructing <see cref="LoopBlock{C, V}"/> instances.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
public class LoopBuilder<C, V>(BlockBuilder<C, V> blockBuilder)
    where C : IContext<V>
{
    /// <summary>
    /// Creates a "Do" loop block.
    /// </summary>
    /// <param name="block">The block to execute in the loop.</param>
    /// <param name="evaluator">A function that evaluates whether to continue looping.</param>
    /// <returns>A new <see cref="LoopBlock{C, V}"/> configured for a "Do" loop.</returns>
    public LoopBlock<C, V> Do(IBlock<C, V> block, Func<C, bool> evaluator)
        => new(block, evaluator, LoopStyle.Do);

    /// <summary>
    /// Creates a "Do" loop block using a block resolved by type.
    /// </summary>
    /// <typeparam name="X">The type of the block to resolve.</typeparam>
    /// <param name="evaluator">A function that evaluates whether to continue looping.</param>
    /// <returns>A new <see cref="LoopBlock{C, V}"/> configured for a "Do" loop.</returns>
    public LoopBlock<C, V> Do<X>(Func<C, bool> evaluator)
        where X : IBlock<C, V>
        => new(blockBuilder.ResolveInstance<X>(), evaluator, LoopStyle.Do);

    /// <summary>
    /// Creates a "While" loop block.
    /// </summary>
    /// <param name="block">The block to execute in the loop.</param>
    /// <param name="evaluator">A function that evaluates whether to continue looping.</param>
    /// <returns>A new <see cref="LoopBlock{C, V}"/> configured for a "While" loop.</returns>
    public LoopBlock<C, V> While(IBlock<C, V> block, Func<C, bool> evaluator)
        => new(block, evaluator, LoopStyle.While);

    /// <summary>
    /// Creates a "While" loop block using a block resolved by type.
    /// </summary>
    /// <typeparam name="X">The type of the block to resolve.</typeparam>
    /// <param name="evaluator">A function that evaluates whether to continue looping.</param>
    /// <returns>A new <see cref="LoopBlock{C, V}"/> configured for a "While" loop.</returns>
    public LoopBlock<C, V> While<X>(Func<C, bool> evaluator)
        where X : IBlock<C, V>
        => new(blockBuilder.ResolveInstance<X>(), evaluator, LoopStyle.While);
}

/// <summary>
/// Extension methods for the <see cref="BlockBuilder{C, V}"/> class to create loop blocks.
/// </summary>
public static partial class BuilderExtensions
{
    /// <summary>
    /// Creates a new <see cref="LoopBuilder{C, V}"/> instance for constructing loop blocks.
    /// </summary>
    /// <typeparam name="C">The context type.</typeparam>
    /// <typeparam name="V">The value type associated with the context.</typeparam>
    /// <param name="builder">The <see cref="BlockBuilder{C, V}"/> instance used to resolve blocks.</param>
    /// <returns>A new <see cref="LoopBuilder{C, V}"/> instance.</returns>
    public static LoopBuilder<C, V> Loop<C, V>(this BlockBuilder<C, V> builder)
        where C : IContext<V>
        => new(builder);
}