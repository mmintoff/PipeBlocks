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
public class TryCatchBlock<C, V>(
    ILogger<TryCatchBlock<C, V>> logger,
    IBlock<C, V> tryBlock,
    IBlock<C, V>? catchBlock = null,
    IBlock<C, V>? finallyBlock = null
) : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    /// <summary>
    /// Executes the block synchronously, applying try-catch-finally behavior.
    /// Logs and redirects to the catch or finally blocks if necessary.
    /// </summary>
    /// <param name="context">The context to execute the block with.</param>
    /// <returns>The updated context after execution.</returns>
    public C Execute(C context)
    {
        bool shouldFlip = false;
        try
        {
            context = BlockExecutor.ExecuteSync(tryBlock, context);
            context.Value.Match(
                failure =>
                {
                    shouldFlip = true;
                    logger.LogError("Failure occurred executing {CorrelationId} with '{FailureReason}'", context.CorrelationId, failure.FailureReason);
                    FlipExecute(shouldFlip, catchBlock, context);
                },
                _ => { });
        }
        catch (Exception ex)
        {
            shouldFlip = true;
            logger.LogError(ex, "Exception occurred executing {CorrelationId}", context.CorrelationId);
            FlipExecute(shouldFlip, catchBlock, context);
        }
        finally
        {
            FlipExecute(shouldFlip, finallyBlock, context);
        }
        return context;

        // Executes the given block if provided and toggles the <c>IsFlipped</c> flag based on <paramref name="shouldFlip"/>.
        static void FlipExecute(bool shouldFlip, IBlock<C, V>? block, C context)
        {
            if (block != null)
            {
                context.IsFlipped = shouldFlip;
                _ = BlockExecutor.ExecuteSync(block, context);
                context.IsFlipped = !shouldFlip;
            }
        }
    }

    /// <summary>
    /// Executes the block asynchronously, applying try-catch-finally behavior.
    /// Logs and redirects to the catch or finally blocks if necessary.
    /// </summary>
    /// <param name="context">The context to execute the block with.</param>
    /// <returns>The updated context after asynchronous execution.</returns>
    public async ValueTask<C> ExecuteAsync(C context)
    {
        bool shouldFlip = false;
        try
        {
            context = await BlockExecutor.ExecuteAsync(tryBlock, context);
            await context.Value.MatchAsync(
                async failure =>
                {
                    shouldFlip = true;
                    logger.LogError("Failure occurred executing {CorrelationId} with '{FailureReason}'", context.CorrelationId, failure.FailureReason);
                    await FlipExecuteAsync(shouldFlip, catchBlock, context);
                },
                _ => ValueTask.CompletedTask);
        }
        catch (Exception ex)
        {
            shouldFlip = true;
            logger.LogError(ex, "Exception occurred executing {CorrelationId}", context.CorrelationId);
            await FlipExecuteAsync(shouldFlip, catchBlock, context);
        }
        finally
        {
            await FlipExecuteAsync(shouldFlip, finallyBlock, context);
        }
        return context;

        // Executes the given block asynchronously if provided and toggles the <c>IsFlipped</c> flag based on <paramref name="shouldFlip"/>.
        static async Task FlipExecuteAsync(bool shouldFlip, IBlock<C, V>? block, C context)
        {
            if (block != null)
            {
                context.IsFlipped = shouldFlip;
                _ = await BlockExecutor.ExecuteAsync(block, context);
                context.IsFlipped = !shouldFlip;
            }
        }
    }
}