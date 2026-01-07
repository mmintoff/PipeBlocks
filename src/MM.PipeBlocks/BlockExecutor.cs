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
    /// Executes a block synchronously. If the block implements both <see cref="ISyncBlock{VIn,VOut}"/> and <see cref="IAsyncBlock{VIn,VOut}"/>, 
    /// synchronous execution is preferred.
    /// </summary>
    /// <typeparam name="VIn">The type of the input value held in the parameter.</typeparam>
    /// <typeparam name="VOut">The type of the output value held in the parameter.</typeparam>
    /// <param name="block">The block to execute.</param>
    /// <param name="value">The current execution parameter.</param>
    /// <param name="handleExceptions">Indicates whether exceptions should be caught and signaled as failures in the parameter.</param>
    /// <returns>The updated execution parameter.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Parameter<VOut> ExecuteSync<VIn, VOut>(
        IBlock<VIn, VOut> block,
        Parameter<VIn> value,
        bool handleExceptions = false)
    {
        return block switch
        {
            ISyncBlock<VIn, VOut> syncBlock when handleExceptions => ExecuteSyncHandled(syncBlock, value),
            ISyncBlock<VIn, VOut> syncBlock => syncBlock.Execute(value),
            IAsyncBlock<VIn, VOut> asyncBlock when handleExceptions => ExecuteAsyncSynchronouslyHandled(asyncBlock, value),
            IAsyncBlock<VIn, VOut> asyncBlock => ExecuteAsyncSynchronously(asyncBlock, value),
            _ => throw new InvalidOperationException($"Block does not implement ISyncBlock<{typeof(VIn).Name}, {typeof(VOut).Name}> or IAsyncBlock<{typeof(VIn).Name}, {typeof(VOut).Name}>")
        };
    }

    /// <summary>
    /// Executes a block asynchronously. If the block implements both <see cref="ISyncBlock{VIn,VOut}"/> and <see cref="IAsyncBlock{VIn,VOut}"/>, 
    /// asynchronous execution is preferred.
    /// </summary>
    /// <typeparam name="VIn">The type of the input value held in the parameter.</typeparam>
    /// <typeparam name="VOut">The type of the output value held in the parameter.</typeparam>
    /// <param name="block">The block to execute.</param>
    /// <param name="value">The current execution parameter.</param>
    /// <param name="handleExceptions">Indicates whether exceptions should be caught and signaled as failures in the parameter.</param>
    /// <returns>A task that represents the asynchronous operation, containing the updated execution parameter.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Parameter<VOut>> ExecuteAsync<VIn, VOut>(
        IBlock<VIn, VOut> block,
        Parameter<VIn> value,
        bool handleExceptions = false)
    {
        return block switch
        {
            IAsyncBlock<VIn, VOut> asyncBlock when handleExceptions => ExecuteAsyncHandled(asyncBlock, value),
            IAsyncBlock<VIn, VOut> asyncBlock => asyncBlock.ExecuteAsync(value),
            ISyncBlock<VIn, VOut> syncBlock when handleExceptions => ExecuteSyncAsValueTaskHandled(syncBlock, value),
            ISyncBlock<VIn, VOut> syncBlock => ValueTask.FromResult(syncBlock.Execute(value)),
            _ => throw new InvalidOperationException($"Block does not implement ISyncBlock<{typeof(VIn).Name}, {typeof(VOut).Name}> or IAsyncBlock<{typeof(VIn).Name}, {typeof(VOut).Name}>")
        };
    }

    // No hint - let JIT decide (has try/catch)
    private static Parameter<VOut> ExecuteSyncHandled<VIn, VOut>(
        ISyncBlock<VIn, VOut> syncBlock,
        Parameter<VIn> value)
    {
        try
        {
            return syncBlock.Execute(value);
        }
        catch (Exception ex)
        {
            var outValue = default(VOut);
            var outParam = new Parameter<VOut>(outValue!);
            outParam.SignalBreak(new ExceptionFailureState<VOut>(outValue!, ex));
            return outParam;
        }
    }

    // No hint - let JIT decide (async method)
    private static async ValueTask<Parameter<VOut>> ExecuteAsyncHandled<VIn, VOut>(
        IAsyncBlock<VIn, VOut> asyncBlock,
        Parameter<VIn> value)
    {
        try
        {
            return await asyncBlock.ExecuteAsync(value).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var outValue = default(VOut);
            var outParam = new Parameter<VOut>(outValue!);
            outParam.SignalBreak(new ExceptionFailureState<VOut>(outValue!, ex));
            return outParam;
        }
    }

    // No hint - let JIT decide (has try/catch)
    private static ValueTask<Parameter<VOut>> ExecuteSyncAsValueTaskHandled<VIn, VOut>(
        ISyncBlock<VIn, VOut> syncBlock,
        Parameter<VIn> value)
    {
        try
        {
            return ValueTask.FromResult(syncBlock.Execute(value));
        }
        catch (Exception ex)
        {
            var outValue = default(VOut);
            var outParam = new Parameter<VOut>(outValue!);
            outParam.SignalBreak(new ExceptionFailureState<VOut>(outValue!, ex));
            return ValueTask.FromResult(outParam);
        }
    }

    // No hint - let JIT decide (has try/catch)
    private static Parameter<VOut> ExecuteAsyncSynchronouslyHandled<VIn, VOut>(
        IAsyncBlock<VIn, VOut> asyncBlock,
        Parameter<VIn> value)
    {
        try
        {
            return ExecuteValueTaskSynchronously(asyncBlock.ExecuteAsync(value));
        }
        catch (Exception ex)
        {
            var outValue = default(VOut);
            var outParam = new Parameter<VOut>(outValue!);
            outParam.SignalBreak(new ExceptionFailureState<VOut>(outValue!, ex));
            return outParam;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Parameter<VOut> ExecuteAsyncSynchronously<VIn, VOut>(
        IAsyncBlock<VIn, VOut> asyncBlock,
        Parameter<VIn> value)
    {
        return ExecuteValueTaskSynchronously(asyncBlock.ExecuteAsync(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Parameter<VOut> ExecuteValueTaskSynchronously<VOut>(ValueTask<Parameter<VOut>> task)
    {
        if (task.IsCompletedSuccessfully)
            return task.Result;

        return ExecuteValueTaskSynchronouslySlow(task);
    }

    // Separate slow path to keep inline size small
    private static Parameter<VOut> ExecuteValueTaskSynchronouslySlow<VOut>(ValueTask<Parameter<VOut>> task)
    {
        if (task.IsCompleted)
            return task.GetAwaiter().GetResult();

        return AsyncContext.Run(() => task.AsTask());
    }
}