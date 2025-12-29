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
    /// Executes a block synchronously. If the block implements both <see cref="ISyncBlock{V}"/> and <see cref="IAsyncBlock{V}"/>, 
    /// synchronous execution is preferred.
    /// </summary>
    /// <typeparam name="V">The type of the value held in the parameter.</typeparam>
    /// <param name="block">The block to execute.</param>
    /// <param name="value">The current execution parameter.</param>
    /// <returns>The updated execution parameter.</returns>
    public static Parameter<V> ExecuteSync<V>(IBlock<V> block, Parameter<V> value)
        => block switch
        {
            ISyncBlock<V> syncBlock => syncBlock.Execute(value),
            IAsyncBlock<V> asyncBlock => ExecuteValueTaskSynchronously(asyncBlock.ExecuteAsync(value)),
            _ => value
        };

    /// <summary>
    /// Executes a block asynchronously. If the block implements both <see cref="ISyncBlock{V}"/> and <see cref="IAsyncBlock{V}"/>, 
    /// asynchronous execution is preferred.
    /// </summary>
    /// <typeparam name="V">The type of the value held in the parameter.</typeparam>
    /// <param name="block">The block to execute.</param>
    /// <param name="value">The current execution parameter.</param>
    /// <returns>A task that represents the asynchronous operation, containing the updated execution parameter.</returns>
    public static ValueTask<Parameter<V>> ExecuteAsync<V>(IBlock<V> block, Parameter<V> value)
    {
        var result = block switch
        {
            IAsyncBlock<V> asyncBlock => asyncBlock.ExecuteAsync(value),
            ISyncBlock<V> syncBlock => ValueTask.FromResult(syncBlock.Execute(value)),
            _ => ValueTask.FromResult(value)
        };

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Parameter<V> ExecuteValueTaskSynchronously<V>(ValueTask<Parameter<V>> task)
        => task.IsCompleted ? task.Result : AsyncContext.Run(task.AsTask);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private static Parameter<V> ExecuteValueTaskSynchronously<V>(ValueTask<Parameter<V>> task)
    //{
    //    if (task.IsCompleted)
    //        return task.Result;

    //    var snapshot = Context.Capture();
    //    ContextSnapshot? innerSnapshot = null;

    //    var result = AsyncContext.Run(async () =>
    //    {
    //        snapshot.Apply();
    //        var r = await task;
    //        innerSnapshot = Context.Capture();
    //        return r;
    //    });

    //    innerSnapshot?.Apply();

    //    return result;
    //}
}