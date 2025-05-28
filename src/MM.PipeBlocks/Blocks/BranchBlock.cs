using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;
using System.Runtime.CompilerServices;

namespace MM.PipeBlocks.Blocks;
/// <summary>
/// Represents a branching block that determines the next block to execute based on the current context and value.
/// </summary>
/// <typeparam name="C">The context type, implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public sealed class BranchBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    private readonly Func<C, IBlock<C, V>>? _syncContextFunc;
    private readonly Func<C, V, IBlock<C, V>>? _syncContextValueFunc;
    private readonly Func<C, ValueTask<IBlock<C, V>>>? _asyncContextFunc;
    private readonly Func<C, V, ValueTask<IBlock<C, V>>>? _asyncContextValueFunc;

    private readonly ExecutionStrategy _executionStrategy;

    private enum ExecutionStrategy : byte
    {
        SyncContext,
        SyncContextValue,
        AsyncContext,
        AsyncContextValue
    }

    /// <summary>
    /// Initializes a new instance using a synchronous function that takes the context.
    /// </summary>
    public BranchBlock(Func<C, IBlock<C, V>> nextBlockFunc)
    {
        _syncContextFunc = nextBlockFunc ?? throw new ArgumentNullException(nameof(nextBlockFunc));
        _executionStrategy = ExecutionStrategy.SyncContext;
    }

    /// <summary>
    /// Initializes a new instance using a synchronous function that takes the context and a value.
    /// </summary>
    public BranchBlock(Func<C, V, IBlock<C, V>> nextBlockFunc)
    {
        _syncContextValueFunc = nextBlockFunc ?? throw new ArgumentNullException(nameof(nextBlockFunc));
        _executionStrategy = ExecutionStrategy.SyncContextValue;
    }

    /// <summary>
    /// Initializes a new instance using an asynchronous function that takes the context.
    /// </summary>
    public BranchBlock(Func<C, ValueTask<IBlock<C, V>>> nextBlockFunc)
    {
        _asyncContextFunc = nextBlockFunc ?? throw new ArgumentNullException(nameof(nextBlockFunc));
        _executionStrategy = ExecutionStrategy.AsyncContext;
    }

    /// <summary>
    /// Initializes a new instance using an asynchronous function that takes the context and a value.
    /// </summary>
    public BranchBlock(Func<C, V, ValueTask<IBlock<C, V>>> nextBlockFunc)
    {
        _asyncContextValueFunc = nextBlockFunc ?? throw new ArgumentNullException(nameof(nextBlockFunc));
        _executionStrategy = ExecutionStrategy.AsyncContextValue;
    }

    /// <summary>
    /// Executes the block synchronously by determining the next block based on the context and invoking it.
    /// </summary>
    /// <param name="context">The context used to determine and invoke the next block.</param>
    /// <returns>The updated context after executing the determined block.</returns>
    public C Execute(C context)
    {
        return context.Value.Match(
            x => context.IsFlipped ? ExecuteWithValue(context, x.Value) : context,
            x => ExecuteWithValue(context, x));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private C ExecuteWithValue(C context, V value)
    {
        var nextBlock = _executionStrategy switch
        {
            ExecutionStrategy.SyncContext => _syncContextFunc!(context),
            ExecutionStrategy.SyncContextValue => _syncContextValueFunc!(context, value),
            ExecutionStrategy.AsyncContext => GetBlockFromAsync(_asyncContextFunc!(context)),
            ExecutionStrategy.AsyncContextValue => GetBlockFromAsync(_asyncContextValueFunc!(context, value)),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };

        return BlockExecutor.ExecuteSync(nextBlock, context);
    }

    /// <summary>
    /// Executes the block asynchronously by determining the next block based on the context and invoking it.
    /// </summary>
    /// <param name="context">The context used to determine and invoke the next block.</param>
    /// <returns>A <see cref="ValueTask{C}"/> representing the updated context.</returns>
    public ValueTask<C> ExecuteAsync(C context)
    {
        return context.Value.Match(
            x => context.IsFlipped ? ExecuteWithValueAsync(context, x.Value) : new ValueTask<C>(context),
            x => ExecuteWithValueAsync(context, x));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<C> ExecuteWithValueAsync(C context, V value)
    {
        var nextBlock = _executionStrategy switch
        {
            ExecutionStrategy.SyncContext => _syncContextFunc!(context),
            ExecutionStrategy.SyncContextValue => _syncContextValueFunc!(context, value),
            ExecutionStrategy.AsyncContext => await _asyncContextFunc!(context).ConfigureAwait(false),
            ExecutionStrategy.AsyncContextValue => await _asyncContextValueFunc!(context, value).ConfigureAwait(false),
            _ => throw new InvalidOperationException("Invalid execution strategy")
        };

        return await BlockExecutor.ExecuteAsync(nextBlock, context).ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IBlock<C, V> GetBlockFromAsync(ValueTask<IBlock<C, V>> task)
        => task.IsCompleted ? task.Result : AsyncContext.Run(task.AsTask);
}