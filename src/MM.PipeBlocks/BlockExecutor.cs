using MM.PipeBlocks.Abstractions;
using Nito.AsyncEx;
using System.Runtime.CompilerServices;
using static MM.PipeBlocks.Abstractions.Context;

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
    public static Parameter<V> ExecuteSync<V>(IBlock<V> block, Parameter<V> value)
        => block switch
        {
            ISyncBlock<V> syncBlock => syncBlock.Execute(value),
            IAsyncBlock<V> asyncBlock => ExecuteValueTaskSynchronously(asyncBlock.ExecuteAsync(value)),
            _ => value
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
    public static ValueTask<Parameter<V>> ExecuteAsync<V>(IBlock<V> block, Parameter<V> value)
    {
        Console.WriteLine($"BlockExecutor.ExecuteAsync - Before - ResultText: {Context.TryGet<string>("ResultText", out var before)}, Value: {before}");

        var result = block switch
        {
            IAsyncBlock<V> asyncBlock => asyncBlock.ExecuteAsync(value),
            ISyncBlock<V> syncBlock => ValueTask.FromResult(syncBlock.Execute(value)),
            _ => ValueTask.FromResult(value)
        };

        Console.WriteLine($"BlockExecutor.ExecuteAsync - After - ResultText: {Context.TryGet<string>("ResultText", out var after)}, Value: {after}");

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