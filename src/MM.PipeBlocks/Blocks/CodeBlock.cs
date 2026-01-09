using MM.PipeBlocks.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure

/// <summary>
/// Represents a synchronous code block that processes a parameter, optionally using a value from the parameter.
/// </summary>
/// <typeparam name="V">The type of the value associated with the parameter.</typeparam>
public abstract class CodeBlock<V> : ISyncBlock<V>
{
    /// <summary>
    /// Executes the block with the provided parameter.
    /// If the parameter context contains a failure and is not flipped, the parameter is returned unchanged.
    /// Otherwise, calls the derived class implementation with the value.
    /// </summary>
    /// <param name="value">The parameter for execution.</param>
    /// <returns>The updated parameter after execution.</returns>
    public virtual Parameter<V> Execute(Parameter<V> value)
    {
        if (value.IsFailure && !value.Context.IsFlipped)
            return value;

        return Execute(value, value.Value);
    }

    /// <summary>
    /// Override this method to implement logic using the value within the parameter.
    /// </summary>
    /// <param name="parameter">The parameter for execution.</param>
    /// <param name="extractedValue">The value extracted from the parameter.</param>
    /// <returns>The updated parameter.</returns>
    protected abstract Parameter<V> Execute(Parameter<V> parameter, V extractedValue);
}

public abstract class CodeBlock<VIn, VOut> : ISyncBlock<VIn, VOut>
{
    public Parameter<VOut> Execute(Parameter<VIn> value)
    {
        if (value.IsFailure && !value.Context.IsFlipped)
            return new Parameter<VOut>(value.Failure)
            {
                Context = value.Context
            };

        return Execute(value, value.Value);
    }

    protected abstract Parameter<VOut> Execute(Parameter<VIn> parameter, VIn extractedValue);
}

/// <summary>
/// Represents an asynchronous code block that processes a parameter, optionally using a value from the parameter.
/// </summary>
/// <typeparam name="V">The type of the value associated with the parameter.</typeparam>
public abstract class AsyncCodeBlock<V> : IAsyncBlock<V>
{
    /// <summary>
    /// Executes the block asynchronously with the provided parameter.
    /// If the parameter context contains a failure and is not flipped, the parameter is returned unchanged.
    /// Otherwise, calls the derived class implementation with the value.
    /// </summary>
    /// <param name="value">The parameter for execution.</param>
    /// <returns>A <see cref="ValueTask{T}"/> representing the asynchronous operation.</returns>
    public virtual ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        if (value.IsFailure && !value.Context.IsFlipped)
            return ValueTask.FromResult(value);

        return ExecuteAsync(value, value.Value);
    }

    /// <summary>
    /// Override this method to implement asynchronous logic using the value within the parameter.
    /// </summary>
    /// <param name="parameter">The parameter for execution.</param>
    /// <param name="extractedValue">The value extracted from the parameter.</param>
    /// <returns>A <see cref="ValueTask{T}"/> representing the result of the asynchronous operation.</returns>
    protected abstract ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> parameter, V extractedValue);
}

public abstract class AsyncCodeBlock<VIn, VOut> : IAsyncBlock<VIn, VOut>
{
    public ValueTask<Parameter<VOut>> ExecuteAsync(Parameter<VIn> value)
    {
        if (value.IsFailure && !value.Context.IsFlipped)
            return ValueTask.FromResult(new Parameter<VOut>(value.Failure)
            {
                Context = value.Context
            });

        return ExecuteAsync(value, value.Value);
    }

    protected abstract ValueTask<Parameter<VOut>> ExecuteAsync(Parameter<VIn> parameter, VIn extractedValue);
}