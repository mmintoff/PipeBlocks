namespace MM.PipeBlocks.Abstractions;
/// <summary>
/// Represents a block of code that operates on a context of type <typeparamref name="C"/> and a value of type <typeparamref name="V"/>.
/// </summary>
/// <typeparam name="C">The type of the context that the block operates on.</typeparam>
/// <typeparam name="V">The type of the value in the context.</typeparam>
public interface IBlock<V>
{ }

/// <summary>
/// Represents a synchronous block that operates on a context of type <typeparamref name="C"/> and a value of type <typeparamref name="V"/>.
/// </summary>
/// <typeparam name="C">The type of the context that the block operates on.</typeparam>
/// <typeparam name="V">The type of the value in the context.</typeparam>
public interface ISyncBlock<V> : IBlock<V>
{
    /// <summary>
    /// Executes the block synchronously with the given context.
    /// </summary>
    /// <param name="context">The context on which the block operates.</param>
    /// <returns>The updated context after execution of the block.</returns>
    Parameter<V> Execute(Parameter<V> value);
}

/// <summary>
/// Represents an asynchronous block that operates on a context of type <typeparamref name="C"/> and a value of type <typeparamref name="V"/>.
/// </summary>
/// <typeparam name="C">The type of the context that the block operates on.</typeparam>
/// <typeparam name="V">The type of the value in the context.</typeparam>
public interface IAsyncBlock<V> : IBlock<V>
{
    /// <summary>
    /// Executes the block asynchronously with the given context.
    /// </summary>
    /// <param name="context">The context on which the block operates.</param>
    /// <returns>A <see cref="ValueTask{C}"/> representing the asynchronous operation, containing the updated context after execution.</returns>
    ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value);
}