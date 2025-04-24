using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Blocks;
/// <summary>
/// Represents a synchronous code block that processes a context, optionally using a value from the context.
/// </summary>
/// <typeparam name="C">The context type, implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public abstract class CodeBlock<C, V> : ISyncBlock<C, V>
    where C : IContext<V>
{
    /// <summary>
    /// Executes the block with the provided context.
    /// If the context is flipped and contains a failure, the context is returned unchanged.
    /// Otherwise, calls the derived class implementation with the value.
    /// </summary>
    /// <param name="context">The context for execution.</param>
    /// <returns>The updated context after execution.</returns>
    public virtual C Execute(C context) => context.Value.Match(
        x => context.IsFlipped ? Execute(context, x.Value) : context,
        x => Execute(context, x));

    /// <summary>
    /// Override this method to implement logic using the value within the context.
    /// </summary>
    /// <param name="context">The context for execution.</param>
    /// <param name="value">The value extracted from the context.</param>
    /// <returns>The updated context.</returns>
    protected abstract C Execute(C context, V value);
}

/// <summary>
/// Represents an asynchronous code block that processes a context, optionally using a value from the context.
/// </summary>
/// <typeparam name="C">The context type, implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public abstract class AsyncCodeBlock<C, V> : IAsyncBlock<C, V>
    where C : IContext<V>
{
    /// <summary>
    /// Executes the block asynchronously with the provided context.
    /// If the context is flipped and contains a failure, the context is returned unchanged.
    /// Otherwise, calls the derived class implementation with the value.
    /// </summary>
    /// <param name="context">The context for execution.</param>
    /// <returns>A <see cref="ValueTask{C}"/> representing the asynchronous operation.</returns>
    public virtual ValueTask<C> ExecuteAsync(C context) => context.Value.Match(
        x => context.IsFlipped ? ExecuteAsync(context, x.Value) : ValueTask.FromResult(context),
        x => ExecuteAsync(context, x));

    /// <summary>
    /// Override this method to implement asynchronous logic using the value within the context.
    /// </summary>
    /// <param name="context">The context for execution.</param>
    /// <param name="value">The value extracted from the context.</param>
    /// <returns>A <see cref="ValueTask{C}"/> representing the result of the asynchronous operation.</returns>
    protected abstract ValueTask<C> ExecuteAsync(C context, V value);
}
