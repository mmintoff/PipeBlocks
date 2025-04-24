using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;

namespace MM.PipeBlocks.Blocks;
/// <summary>
/// Represents a branching block that determines the next block to execute based on the current context and value.
/// </summary>
/// <typeparam name="C">The context type, implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of the value associated with the context.</typeparam>
public sealed class BranchBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    private readonly bool _isSyncFunc;
    private readonly Either<Func<C, IBlock<C, V>>, Func<C, V, IBlock<C, V>>>? _syncNextBlockFunc;
    private readonly Either<Func<C, ValueTask<IBlock<C, V>>>, Func<C, V, ValueTask<IBlock<C, V>>>>? _asyncNextBlockFunc;

    /// <summary>
    /// Initializes a new instance using a synchronous function that takes the context.
    /// </summary>
    public BranchBlock(Func<C, IBlock<C, V>> nextBlockFunc) => (_syncNextBlockFunc, _isSyncFunc) = (nextBlockFunc, true);

    /// <summary>
    /// Initializes a new instance using a synchronous function that takes the context and a value.
    /// </summary>
    public BranchBlock(Func<C, V, IBlock<C, V>> nextBlockFunc) => (_syncNextBlockFunc, _isSyncFunc) = (nextBlockFunc, true);

    /// <summary>
    /// Initializes a new instance using an asynchronous function that takes the context.
    /// </summary>
    public BranchBlock(Func<C, ValueTask<IBlock<C, V>>> nextBlockFunc) => (_asyncNextBlockFunc, _isSyncFunc) = (nextBlockFunc, false);

    /// <summary>
    /// Initializes a new instance using an asynchronous function that takes the context and a value.
    /// </summary>
    public BranchBlock(Func<C, V, ValueTask<IBlock<C, V>>> nextBlockFunc) => (_asyncNextBlockFunc, _isSyncFunc) = (nextBlockFunc, false);

    /// <summary>
    /// Executes the block synchronously by determining the next block based on the context and invoking it.
    /// </summary>
    /// <param name="context">The context used to determine and invoke the next block.</param>
    /// <returns>The updated context after executing the determined block.</returns>
    public C Execute(C context)
    {
        return context.Value.Match(
            x => context.IsFlipped ? Execute(context, x.Value) : context,
            x => Execute(context, x));

        C Execute(C context, V value)
            => BlockExecutor.ExecuteSync
            (
                _isSyncFunc
                    ? _syncNextBlockFunc!.Match(f => f(context), f => f(context, value))
                    : AsyncContext.Run(async () => await _asyncNextBlockFunc!.MatchAsync(f => f(context), f => f(context, value))),
                context
            );
    }

    /// <summary>
    /// Executes the block asynchronously by determining the next block based on the context and invoking it.
    /// </summary>
    /// <param name="context">The context used to determine and invoke the next block.</param>
    /// <returns>A <see cref="ValueTask{C}"/> representing the updated context.</returns>
    public async ValueTask<C> ExecuteAsync(C context)
    {
        return await context.Value.MatchAsync(
            x => context.IsFlipped ? ExecuteAsync(context, x.Value) : ValueTask.FromResult(context),
            x => ExecuteAsync(context, x));

        async ValueTask<C> ExecuteAsync(C context, V value)
            => await BlockExecutor.ExecuteAsync(
                _isSyncFunc
                    ? _syncNextBlockFunc!.Match(f => f(context), f => f(context, value))
                    : await _asyncNextBlockFunc!.MatchAsync(f => f(context), f => f(context, value)),
                context);
    }
}
