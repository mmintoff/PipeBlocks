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
    /// <param name="handleExceptions">Indicates whether exceptions should be caught and signaled as failures in the parameter.</param>
    /// <returns>The updated execution parameter.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Parameter<V> ExecuteSync<V>(IBlock<V> block, Parameter<V> value, bool handleExceptions = false)
    {
        return block switch
        {
            ISyncBlock<V> syncBlock when handleExceptions => ExecuteSyncHandled(syncBlock, value),
            ISyncBlock<V> syncBlock => syncBlock.Execute(value),
            IAsyncBlock<V> asyncBlock when handleExceptions => ExecuteAsyncSynchronouslyHandled(asyncBlock, value),
            IAsyncBlock<V> asyncBlock => ExecuteAsyncSynchronously(asyncBlock, value),
            _ => value
        };
    }

    /// <summary>
    /// Executes a block asynchronously. If the block implements both <see cref="ISyncBlock{V}"/> and <see cref="IAsyncBlock{V}"/>, 
    /// asynchronous execution is preferred.
    /// </summary>
    /// <typeparam name="V">The type of the value held in the parameter.</typeparam>
    /// <param name="block">The block to execute.</param>
    /// <param name="value">The current execution parameter.</param>
    /// <param name="handleExceptions">Indicates whether exceptions should be caught and signaled as failures in the parameter.</param>
    /// <returns>A task that represents the asynchronous operation, containing the updated execution parameter.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Parameter<V>> ExecuteAsync<V>(
        IBlock<V> block,
        Parameter<V> value,
        bool handleExceptions = false)
    {
        return block switch
        {
            IAsyncBlock<V> asyncBlock when handleExceptions => ExecuteAsyncHandled(asyncBlock, value),
            IAsyncBlock<V> asyncBlock => asyncBlock.ExecuteAsync(value),
            ISyncBlock<V> syncBlock when handleExceptions => ExecuteSyncAsValueTaskHandled(syncBlock, value),
            ISyncBlock<V> syncBlock => ValueTask.FromResult(syncBlock.Execute(value)),
            _ => ValueTask.FromResult(value)
        };
    }

    // No hint - let JIT decide (has try/catch)
    private static Parameter<V> ExecuteSyncHandled<V>(ISyncBlock<V> syncBlock, Parameter<V> value)
    {
        try
        {
            return syncBlock.Execute(value);
        }
        catch (Exception ex)
        {
            value.SignalBreak(new ExceptionFailureState<V>(value.Value, ex));
            return value;
        }
    }

    // No hint - let JIT decide (async method)
    private static async ValueTask<Parameter<V>> ExecuteAsyncHandled<V>(IAsyncBlock<V> asyncBlock, Parameter<V> value)
    {
        try
        {
            return await asyncBlock.ExecuteAsync(value).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            value.SignalBreak(new ExceptionFailureState<V>(value.Value, ex));
            return value;
        }
    }

    // No hint - let JIT decide (has try/catch)
    private static ValueTask<Parameter<V>> ExecuteSyncAsValueTaskHandled<V>(ISyncBlock<V> syncBlock, Parameter<V> value)
    {
        try
        {
            return ValueTask.FromResult(syncBlock.Execute(value));
        }
        catch (Exception ex)
        {
            value.SignalBreak(new ExceptionFailureState<V>(value.Value, ex));
            return ValueTask.FromResult(value);
        }
    }

    // No hint - let JIT decide (has try/catch)
    private static Parameter<V> ExecuteAsyncSynchronouslyHandled<V>(IAsyncBlock<V> asyncBlock, Parameter<V> value)
    {
        try
        {
            return ExecuteValueTaskSynchronously(asyncBlock.ExecuteAsync(value));
        }
        catch (Exception ex)
        {
            value.SignalBreak(new ExceptionFailureState<V>(value.Value, ex));
            return value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Parameter<V> ExecuteAsyncSynchronously<V>(IAsyncBlock<V> asyncBlock, Parameter<V> value)
    {
        return ExecuteValueTaskSynchronously(asyncBlock.ExecuteAsync(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Parameter<V> ExecuteValueTaskSynchronously<V>(ValueTask<Parameter<V>> task)
    {
        if (task.IsCompletedSuccessfully)
            return task.Result;

        return ExecuteValueTaskSynchronouslySlow(task);
    }

    // Separate slow path to keep inline size small
    private static Parameter<V> ExecuteValueTaskSynchronouslySlow<V>(ValueTask<Parameter<V>> task)
    {
        if (task.IsCompleted)
            return task.GetAwaiter().GetResult();

        return AsyncContext.Run(() => task.AsTask());
    }
}