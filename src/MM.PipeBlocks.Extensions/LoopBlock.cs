using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that executes a loop based on a condition, either performing a "Do" or "While" loop style.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
public sealed class LoopBlock<V> : ISyncBlock<V>, IAsyncBlock<V>
{
    private readonly IBlock<V> _block;
    private readonly Func<Parameter<V>, bool> _evaluator;
    private readonly LoopStyle _loopStyle;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopBlock{C, V}"/> class.
    /// </summary>
    /// <param name="block">The block to execute in the loop.</param>
    /// <param name="evaluator">A function that evaluates whether to continue looping.</param>
    /// <param name="loopStyle">The style of the loop, either "Do" or "While".</param>
    public LoopBlock(IBlock<V> block, Func<Parameter<V>, bool> evaluator, LoopStyle loopStyle)
        => (_block, _evaluator, _loopStyle)
        = (block, evaluator, loopStyle);

    /// <summary>
    /// Executes the block synchronously, looping until the evaluation function returns false.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>The modified context after loop execution.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        if (_loopStyle == LoopStyle.Do)
            value = BlockExecutor.ExecuteSync(_block, value);
        while (_evaluator(value))
            value = BlockExecutor.ExecuteSync(_block, value);

        return value;
    }

    /// <summary>
    /// Executes the block asynchronously, looping until the evaluation function returns false.
    /// </summary>
    /// <param name="context">The context to execute.</param>
    /// <returns>A task that represents the asynchronous operation, with the modified context after loop execution.</returns>
    public async ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        if (_loopStyle == LoopStyle.Do)
            value = await BlockExecutor.ExecuteAsync(_block, value);
        while (_evaluator(value))
            value = await BlockExecutor.ExecuteAsync(_block, value);

        return value;
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
public class LoopBuilder<V>(BlockBuilder<V> blockBuilder)
{
    /// <summary>
    /// Creates a "Do" loop block.
    /// </summary>
    /// <param name="block">The block to execute in the loop.</param>
    /// <param name="evaluator">A function that evaluates whether to continue looping.</param>
    /// <returns>A new <see cref="LoopBlock{C, V}"/> configured for a "Do" loop.</returns>
    public LoopBlock<V> Do(IBlock<V> block, Func<Parameter<V>, bool> evaluator)
        => new(block, evaluator, LoopStyle.Do);

    /// <summary>
    /// Creates a "Do" loop block using a block resolved by type.
    /// </summary>
    /// <typeparam name="X">The type of the block to resolve.</typeparam>
    /// <param name="evaluator">A function that evaluates whether to continue looping.</param>
    /// <returns>A new <see cref="LoopBlock{C, V}"/> configured for a "Do" loop.</returns>
    public LoopBlock<V> Do<X>(Func<Parameter<V>, bool> evaluator)
        where X : IBlock<V>
        => new(blockBuilder.ResolveInstance<X>(), evaluator, LoopStyle.Do);

    /// <summary>
    /// Creates a "While" loop block.
    /// </summary>
    /// <param name="block">The block to execute in the loop.</param>
    /// <param name="evaluator">A function that evaluates whether to continue looping.</param>
    /// <returns>A new <see cref="LoopBlock{C, V}"/> configured for a "While" loop.</returns>
    public LoopBlock<V> While(IBlock<V> block, Func<Parameter<V>, bool> evaluator)
        => new(block, evaluator, LoopStyle.While);

    /// <summary>
    /// Creates a "While" loop block using a block resolved by type.
    /// </summary>
    /// <typeparam name="X">The type of the block to resolve.</typeparam>
    /// <param name="evaluator">A function that evaluates whether to continue looping.</param>
    /// <returns>A new <see cref="LoopBlock{C, V}"/> configured for a "While" loop.</returns>
    public LoopBlock<V> While<X>(Func<Parameter<V>, bool> evaluator)
        where X : IBlock<V>
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
    public static LoopBuilder<V> Loop<V>(this BlockBuilder<V> builder)
        => new(builder);
}