using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure
/// <summary>
/// Represents a block that wraps execution in try-catch-finally semantics, supporting both synchronous and asynchronous execution.
/// </summary>
/// <typeparam name="C">The type of the context, implementing <see cref="IContext{V}"/>.</typeparam>
/// <typeparam name="V">The type of value associated with the context.</typeparam>
public class TryCatchBlock<V>(
    ILogger<TryCatchBlock<V>> logger,
    IBlock<V> tryBlock,
    IBlock<V>? catchBlock = null,
    IBlock<V>? finallyBlock = null
) : ISyncBlock<V>, IAsyncBlock<V>
{
    /// <summary>
    /// Executes the block synchronously, applying try-catch-finally behavior.
    /// Logs and redirects to the catch or finally blocks if necessary.
    /// </summary>
    /// <param name="context">The context to execute the block with.</param>
    /// <returns>The updated context after execution.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        bool shouldFlip = false;
        try
        {
            value = BlockExecutor.ExecuteSync(tryBlock, value);
            value.Match(
                failure =>
                {
                    shouldFlip = true;
                    logger.LogError("Failure occurred executing {CorrelationId} with '{FailureReason}'", Context.CorrelationId, failure.FailureReason);
                    FlipExecute(shouldFlip, catchBlock, value);
                },
                _ => { });
        }
        catch (Exception ex)
        {
            shouldFlip = true;
            logger.LogError(ex, "Exception occurred executing {CorrelationId}", Context.CorrelationId);
            FlipExecute(shouldFlip, catchBlock, value);
        }
        finally
        {
            FlipExecute(shouldFlip, finallyBlock, value);
        }
        return value;

        // Executes the given block if provided and toggles the <c>IsFlipped</c> flag based on <paramref name="shouldFlip"/>.
        static void FlipExecute(bool shouldFlip, IBlock<V>? block, Parameter<V> value)
        {
            if (block != null)
            {
                Context.IsFlipped = shouldFlip;
                _ = BlockExecutor.ExecuteSync(block, value);
                Context.IsFlipped = !shouldFlip;
            }
        }
    }

    /// <summary>
    /// Executes the block asynchronously, applying try-catch-finally behavior.
    /// Logs and redirects to the catch or finally blocks if necessary.
    /// </summary>
    /// <param name="context">The context to execute the block with.</param>
    /// <returns>The updated context after asynchronous execution.</returns>
    public async ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        bool shouldFlip = false;
        try
        {
            value = await BlockExecutor.ExecuteAsync(tryBlock, value);
            await value.MatchAsync(
                async failure =>
                {
                    shouldFlip = true;
                    logger.LogError("Failure occurred executing {CorrelationId} with '{FailureReason}'", Context.CorrelationId, failure.FailureReason);
                    await FlipExecuteAsync(shouldFlip, catchBlock, value);
                },
                _ => ValueTask.CompletedTask);
        }
        catch (Exception ex)
        {
            shouldFlip = true;
            logger.LogError(ex, "Exception occurred executing {CorrelationId}", Context.CorrelationId);
            await FlipExecuteAsync(shouldFlip, catchBlock, value);
        }
        finally
        {
            await FlipExecuteAsync(shouldFlip, finallyBlock, value);
        }
        return value;

        // Executes the given block asynchronously if provided and toggles the <c>IsFlipped</c> flag based on <paramref name="shouldFlip"/>.
        static async Task FlipExecuteAsync(bool shouldFlip, IBlock<V>? block, Parameter<V> value)
        {
            if (block != null)
            {
                Context.IsFlipped = shouldFlip;
                _ = await BlockExecutor.ExecuteAsync(block, value);
                Context.IsFlipped = !shouldFlip;
            }
        }
    }
}