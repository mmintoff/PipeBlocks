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
        try
        {
            return block switch
            {
                ISyncBlock<V> syncBlock => syncBlock.Execute(value),
                IAsyncBlock<V> asyncBlock => ExecuteValueTaskSynchronously(value, asyncBlock.ExecuteAsync(value), handleExceptions),
                _ => value
            };
        }
        catch (Exception ex) when (handleExceptions)
        {
            value.SignalBreak(new ExceptionFailureState<V>(value.Value, ex));
            return value;
        }
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
        if (!handleExceptions)
        {
            return block switch
            {
                IAsyncBlock<V> asyncBlock => asyncBlock.ExecuteAsync(value),
                ISyncBlock<V> syncBlock => ValueTask.FromResult(syncBlock.Execute(value)),
                _ => ValueTask.FromResult(value)
            };
        }

        return ExecuteHandledAsync(block, value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ValueTask<Parameter<V>> ExecuteHandledAsync<V>(IBlock<V> block, Parameter<V> value)
    {
        // Your preferred handled implementation here (the async-await version).
        return ExecuteHandledCoreAsync(block, value);

        static async ValueTask<Parameter<V>> ExecuteHandledCoreAsync(IBlock<V> block, Parameter<V> value)
        {
            try
            {
                return block switch
                {
                    IAsyncBlock<V> asyncBlock => await asyncBlock.ExecuteAsync(value).ConfigureAwait(false),
                    ISyncBlock<V> syncBlock => syncBlock.Execute(value),
                    _ => value
                };
            }
            catch (Exception ex)
            {
                value.SignalBreak(new ExceptionFailureState<V>(value.Value, ex));
                return value;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Parameter<V> ExecuteValueTaskSynchronously<V>(Parameter<V> value, ValueTask<Parameter<V>> task, bool handleExceptions = false)
    {
        try
        {
            if (task.IsCompletedSuccessfully)
                return task.Result;

            if (task.IsCompleted)
                return task.GetAwaiter().GetResult();

            return AsyncContext.Run(() => task.AsTask());
        }
        catch (Exception ex) when (handleExceptions)
        {
            value.SignalBreak(new ExceptionFailureState<V>(value.Value, ex));
            return value;
        }
    }
}