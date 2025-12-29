using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure
/// <summary>
/// Represents a branching block that determines the next block to execute based on the current parameter and value.
/// </summary>
/// <typeparam name="V">The type of the value associated with the parameter.</typeparam>
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
    /// Initializes a new instance using a synchronous function that takes the parameter.
    /// </summary>
    public BranchBlock(Func<Parameter<V>, IBlock<V>> nextBlockFunc)
    {
        _syncContextFunc = nextBlockFunc ?? throw new ArgumentNullException(nameof(nextBlockFunc));
        _executionStrategy = ExecutionStrategy.Sync;
    }

    /// <summary>
    /// Initializes a new instance using an asynchronous function that takes the parameter.
    /// </summary>
    public BranchBlock(Func<Parameter<V>, ValueTask<IBlock<V>>> nextBlockFunc)
    {
        _asyncContextFunc = nextBlockFunc ?? throw new ArgumentNullException(nameof(nextBlockFunc));
        _executionStrategy = ExecutionStrategy.Async;
    }

    /// <summary>
    /// Executes the block synchronously by determining the next block based on the parameter and invoking it.
    /// </summary>
    /// <param name="value">The parameter used to determine and invoke the next block.</param>
    /// <returns>The updated parameter after executing the determined block.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        return value.Match(
            x => value.Context.IsFlipped ? ExecuteWithStrategy(value) : value,
            x => ExecuteWithStrategy(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Parameter<V> ExecuteWithStrategy(Parameter<V> value)
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
    /// Executes the block asynchronously by determining the next block based on the parameter and invoking it.
    /// </summary>
    /// <param name="value">The parameter used to determine and invoke the next block.</param>
    /// <returns>A <see cref="ValueTask{T}"/> representing the updated parameter.</returns>
    public ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        return value.Match(
            x => value.Context.IsFlipped ? ExecuteWithStrategyAsync(value) : new ValueTask<Parameter<V>>(value),
            x => ExecuteWithStrategyAsync(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<Parameter<V>> ExecuteWithStrategyAsync(Parameter<V> value)
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