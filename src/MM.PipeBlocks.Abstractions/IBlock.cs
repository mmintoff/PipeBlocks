namespace MM.PipeBlocks.Abstractions;

/// <summary>
/// Represents the bare minimum interface for a block. Useful for type restrictions
/// </summary>
public interface IBlock { }
/// <summary>
/// Represents a block of code that operates on a value of type <typeparamref name="V"/>.
/// </summary>
/// <typeparam name="VIn">The type of the value in the parameter.</typeparam>
/// <typeparam name="VOut">The type of the value in the parameter.</typeparam>
public interface IBlock<VIn, VOut> : IBlock { }

public interface IBlock<V> : IBlock<V, V> { }

/// <summary>
/// Represents a synchronous block that operates on a value of type <typeparamref name="V"/>.
/// </summary>
/// <typeparam name="VIn">The type of the value in the parameter.</typeparam>
/// <typeparam name="VOut">The type of the value in the parameter.</typeparam>
public interface ISyncBlock<VIn, VOut> : IBlock<VIn, VOut>
{
    /// <summary>
    /// Executes the block synchronously with the given parameter.
    /// </summary>
    /// <param name="value">The parameter on which the block operates.</param>
    /// <returns>The updated parameter after execution of the block.</returns>
    Parameter<VOut> Execute(Parameter<VIn> value);
}

public interface ISyncBlock<V> : IBlock<V>, ISyncBlock<V, V> { }

/// <summary>
/// Represents an asynchronous block that operates on a value of type <typeparamref name="V"/>.
/// </summary>
/// <typeparam name="VIn">The type of the value in the parameter.</typeparam>
/// <typeparam name="VOut">The type of the value in the parameter.</typeparam>
public interface IAsyncBlock<VIn, VOut> : IBlock<VIn, VOut>
{
    /// <summary>
    /// Executes the block asynchronously with the given parameter.
    /// </summary>
    /// <param name="value">The parameter on which the block operates.</param>
    /// <returns>A <see cref="ValueTask{T}"/> representing the asynchronous operation, containing the updated parameter after execution.</returns>
    ValueTask<Parameter<VOut>> ExecuteAsync(Parameter<VIn> value);
}

public interface IAsyncBlock<V> : IBlock<V>, IAsyncBlock<V, V> { }