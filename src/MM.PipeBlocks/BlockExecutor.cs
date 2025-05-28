using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;
using System.Runtime.CompilerServices;

namespace MM.PipeBlocks;
/// <summary>
/// Executes blocks that implement either synchronous or asynchronous logic.
/// </summary>
public static class BlockExecutor
{
    /// <summary>
    /// Executes a block synchronously. If the block implements both <see cref="ISyncBlock{C, V}"/> and <see cref="IAsyncBlock{C, V}"/>, 
    /// synchronous execution is preferred.
    /// </summary>
    /// <typeparam name="C">The type of the execution context.</typeparam>
    /// <typeparam name="V">The type of the value held in the context.</typeparam>
    /// <param name="block">The block to execute.</param>
    /// <param name="context">The current execution context.</param>
    /// <returns>The updated execution context.</returns>
    public static C ExecuteSync<C, V>(IBlock<C, V> block, C context)
        where C : IContext<V>
        => block switch
        {
            ISyncBlock<C, V> syncBlock => syncBlock.Execute(context),
            IAsyncBlock<C, V> asyncBlock => ExecuteValueTaskSynchronously<C, V>(asyncBlock.ExecuteAsync(context)),
            _ => context
        };

    /// <summary>
    /// Executes a block asynchronously. If the block implements both <see cref="ISyncBlock{C, V}"/> and <see cref="IAsyncBlock{C, V}"/>, 
    /// asynchronous execution is preferred.
    /// </summary>
    /// <typeparam name="C">The type of the execution context.</typeparam>
    /// <typeparam name="V">The type of the value held in the context.</typeparam>
    /// <param name="block">The block to execute.</param>
    /// <param name="context">The current execution context.</param>
    /// <returns>A task that represents the asynchronous operation, containing the updated execution context.</returns>
    public static ValueTask<C> ExecuteAsync<C, V>(IBlock<C, V> block, C context)
        where C : IContext<V>
        => block switch
        {
            IAsyncBlock<C, V> asyncBlock => asyncBlock.ExecuteAsync(context),
            ISyncBlock<C, V> syncBlock => ValueTask.FromResult(syncBlock.Execute(context)),
            _ => ValueTask.FromResult(context)
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static C ExecuteValueTaskSynchronously<C, V>(ValueTask<C> task)
        where C: IContext<V>
        => task.IsCompleted ? task.Result : AsyncContext.Run(task.AsTask);
}