using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure
/// <summary>
/// Represents a block that wraps execution in try-catch-finally semantics, supporting both synchronous and asynchronous execution.
/// </summary>
/// <typeparam name="V">The type of value associated with the parameter.</typeparam>
public class TryCatchBlock<V>(
    ILogger<TryCatchBlock<V>> logger,
    IBlock<V> tryBlock,
    IBlock<V>? catchBlock = null,
    IBlock<V>? finallyBlock = null
) : ISyncBlock<V>, IAsyncBlock<V>
{
    private static readonly Action<ILogger, Guid, string, Exception?> s_failure_occurred = LoggerMessage.Define<Guid, string>(LogLevel.Trace, default, "Failure occurred executing {CorrelationId} with '{FailureReason}'");
    private static readonly Action<ILogger, Guid, string, Exception?> s_exception_occurred = LoggerMessage.Define<Guid, string>(LogLevel.Trace, default, "Exception occurred executing {CorrelationId} with '{ExceptionMessage}'");


    /// <summary>
    /// Executes the block synchronously, applying try-catch-finally behavior.
    /// Logs and redirects to the catch or finally blocks if necessary.
    /// </summary>
    /// <param name="value">The parameter to execute the block with.</param>
    /// <returns>The updated parameter after execution.</returns>
    public Parameter<V> Execute(Parameter<V> value)
    {
        bool shouldFlip = false;
        try
        {
            value = BlockExecutor.ExecuteSync(tryBlock, value, false);

            value.Match(
                failure =>
                {
                    shouldFlip = true;
                    s_failure_occurred(logger, value.Context.CorrelationId, failure.FailureReason ?? "Unknown", null);
                    value = FlipExecute(shouldFlip, catchBlock, value);
                },
                _ => { });
        }
        catch (Exception ex)
        {
            shouldFlip = true;
            s_exception_occurred(logger, value.Context.CorrelationId, ex.Message, ex);
            value = FlipExecute(shouldFlip, catchBlock, value);
        }
        finally
        {
            value = FlipExecute(shouldFlip, finallyBlock, value);
        }
        return value;

        // Executes the given block if provided and toggles the <c>IsFlipped</c> flag based on <paramref name="shouldFlip"/>.
        static Parameter<V> FlipExecute(bool shouldFlip, IBlock<V>? block, Parameter<V> value)
        {
            if (block != null)
            {
                value.Context.IsFlipped = shouldFlip;
                var result = BlockExecutor.ExecuteSync(block, value, false);
                value.Context.IsFlipped = !shouldFlip;
                return result;
            }
            return value;
        }
    }

    /// <summary>
    /// Executes the block asynchronously, applying try-catch-finally behavior.
    /// Logs and redirects to the catch or finally blocks if necessary.
    /// </summary>
    /// <param name="value">The parameter to execute the block with.</param>
    /// <returns>The updated parameter after asynchronous execution.</returns>
    public async ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        bool shouldFlip = false;

        try
        {
            value = await BlockExecutor.ExecuteAsync(tryBlock, value, false);
            await value.MatchAsync(
                async failure =>
                {
                    shouldFlip = true;
                    s_failure_occurred(logger, value.Context.CorrelationId, failure.FailureReason ?? "Unknown", null);
                    value = await FlipExecuteAsync(shouldFlip, catchBlock, value);
                },
                _ => ValueTask.CompletedTask);
        }
        catch (Exception ex)
        {
            shouldFlip = true;
            s_exception_occurred(logger, value.Context.CorrelationId, ex.Message, ex);
            value = await FlipExecuteAsync(shouldFlip, catchBlock, value);
        }
        finally
        {
            value = await FlipExecuteAsync(shouldFlip, finallyBlock, value);
        }
        return value;

        // Executes the given block asynchronously if provided and toggles the <c>IsFlipped</c> flag based on <paramref name="shouldFlip"/>.
        static async Task<Parameter<V>> FlipExecuteAsync(bool shouldFlip, IBlock<V>? block, Parameter<V> value)
        {
            if (block != null)
            {
                value.Context.IsFlipped = shouldFlip;
                var result = await BlockExecutor.ExecuteAsync(block, value, false);
                value.Context.IsFlipped = !shouldFlip;
                return result;
            }
            return value;
        }
    }
}