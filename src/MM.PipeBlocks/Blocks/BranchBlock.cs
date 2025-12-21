using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Internal;
using Nito.AsyncEx;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure
/// <summary>
/// Represents a branching block that determines the next block to execute based on the current context and value.
/// </summary>
/// <typeparam name="C">The context type, implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public sealed class BranchBlock<V> : ISyncBlock<V>, IAsyncBlock<V>
{
    private readonly Func<Parameter<V>, IBlock<V>>? _syncContextFunc;
    private readonly Func<Parameter<V>, ValueTask<IBlock<V>>>? _asyncContextFunc;

    private readonly ExecutionStrategy _executionStrategy;

    private enum ExecutionStrategy : byte
    {
        Sync,
        Async,
    }

    /// <summary>
    /// Initializes a new instance using a synchronous function that takes the context.
    /// </summary>
    public BranchBlock(Func<Parameter<V>, IBlock<V>> nextBlockFunc)
    {
        _syncContextFunc = nextBlockFunc ?? throw new ArgumentNullException(nameof(nextBlockFunc));
        _executionStrategy = ExecutionStrategy.Sync;
    }

    /// <summary>
    /// Initializes a new instance using an asynchronous function that takes the context.
    /// </summary>
    public BranchBlock(Func<Parameter<V>, ValueTask<IBlock<V>>> nextBlockFunc)
    {
        _asyncContextFunc = nextBlockFunc ?? throw new ArgumentNullException(nameof(nextBlockFunc));
        _executionStrategy = ExecutionStrategy.Async;
    }

    /// <summary>
    /// Executes the block synchronously by determining the next block based on the context and invoking it.
    /// </summary>
    /// <param name="context">The context used to determine and invoke the next block.</param>
    /// <returns>The updated context after executing the determined block.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        return value.Match(
            x => Context.IsFlipped ? ExecuteWithValue(x.Value) : value,
            x => ExecuteWithValue(x));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Parameter<V> ExecuteWithValue(Parameter<V> value)
    {
        var nextBlock = _executionStrategy switch
        {
            ExecutionStrategy.Sync => _syncContextFunc!(value),
            ExecutionStrategy.Async => GetBlockFromAsync(_asyncContextFunc!(value)),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };

        return BlockExecutor.ExecuteSync(nextBlock, value);
    }

    /// <summary>
    /// Executes the block asynchronously by determining the next block based on the context and invoking it.
    /// </summary>
    /// <param name="context">The context used to determine and invoke the next block.</param>
    /// <returns>A <see cref="ValueTask{C}"/> representing the updated context.</returns>
    public ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        return value.Match(
            x => Context.IsFlipped ? ExecuteWithValueAsync(x.Value) : new ValueTask<Parameter<V>>(value),
            x => ExecuteWithValueAsync(x));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<Parameter<V>> ExecuteWithValueAsync(Parameter<V> value)
    {
        var nextBlock = _executionStrategy switch
        {
            ExecutionStrategy.Sync => _syncContextFunc!(value),
            ExecutionStrategy.Async => await _asyncContextFunc!(value).ConfigureAwait(false),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };

        return await BlockExecutor.ExecuteAsync(nextBlock, value).ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IBlock<V> GetBlockFromAsync(ValueTask<IBlock<V>> task)
        => task.IsCompleted ? task.Result : AsyncContext.Run(task.AsTask);
}