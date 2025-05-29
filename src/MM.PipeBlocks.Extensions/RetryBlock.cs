using MM.PipeBlocks.Abstractions;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System.Runtime.CompilerServices;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that wraps another block with retry behaviours.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
public sealed class RetryBlock<C, V> : ISyncBlock<C, V>, IAsyncBlock<C, V>
    where C : IContext<V>
{
    private readonly IBlock<C, V> _block;
    private readonly RetryPolicy<C> _retryPolicy;
    private readonly AsyncRetryPolicy<C> _asyncRetryPolicy;

    private readonly ExceptionHandler<C, V> _exceptionHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryBlock{C, V}"/> class.
    /// </summary>
    /// <param name="block">The block to be executed with retry logic.</param>
    /// <param name="delays">A sequence of retry delays.</param>
    /// <param name="exceptionHandler">The handler to be invoked when an exception occurs.</param>
    public RetryBlock(
        IBlock<C, V> block,
        IEnumerable<TimeSpan> delays,
        ExceptionHandler<C, V> exceptionHandler)
    {
        _block = block;
        _exceptionHandler = exceptionHandler;

        var delaysArray = delays as TimeSpan[] ?? [.. delays];

        _retryPolicy = Policy
            .Handle<Exception>()
            .OrResult<C>(ShouldRetry)
            .WaitAndRetry(delaysArray);

        _asyncRetryPolicy = Policy
            .Handle<Exception>()
            .OrResult<C>(ShouldRetry)
            .WaitAndRetryAsync(delaysArray);
    }

    public C Execute(C context) => context.Value.Match(
        x => context.IsFlipped ? Execute(context, x.Value) : context,
        x => Execute(context, x));

    public ValueTask<C> ExecuteAsync(C context) => context.Value.Match(
        x => context.IsFlipped ? ExecuteAsync(context, x.Value) : ValueTask.FromResult(context),
        x => ExecuteAsync(context, x));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private C Execute(C context, V value)
    {
        var localContext = context;
        var localValue = value;

        try
        {
            return _retryPolicy.Execute(() =>
            {
                localContext.IsFinished = false;
                localContext.Value = value;

                return BlockExecutor.ExecuteSync(_block, localContext);
            });
        }
        catch (Exception ex)
        {
            _exceptionHandler(context, value, ex);
            return localContext;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<C> ExecuteAsync(C context, V value)
    {
        var localContext = context;
        var localValue = value;

        try
        {
            return await _asyncRetryPolicy.ExecuteAsync(async () =>
            {
                localContext.IsFinished = false;
                localContext.Value = value;

                return await BlockExecutor.ExecuteAsync(_block, localContext);
            });
        }
        catch (Exception ex)
        {
            _exceptionHandler(context, value, ex);
            return localContext;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldRetry(C context) => context.Value.Match(
        static _ => true,
        static _ => false);
}

/// <summary>
/// A builder class for constructing <see cref="RetryBlock{C, V}"/> instances.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type associated with the context.</typeparam>
public class RetryBuilder<C, V>(BlockBuilder<C, V> blockBuilder)
    where C : IContext<V>
{
    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <typeparam name="X">Block type to be retried</typeparam>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute<X>()
        where X : IBlock<C, V>
        => Execute(blockBuilder.ResolveInstance<X>());

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <typeparam name="X">Block type to be retried</typeparam>
    /// <param name="exceptionHandler">Delegate to process unhandled exception after all retries have been attempted</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute<X>(ExceptionHandler<C, V> exceptionHandler)
        where X : IBlock<C, V>
        => Execute(blockBuilder.ResolveInstance<X>(),
            exceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <typeparam name="X">Block type to be retried</typeparam>
    /// <param name="retryCount">Number of attempts to make with DecorrelatedJitterBackoffV2</param>
    /// <param name="exceptionHandler">Delegate to process unhandled exception after all retries have been attempted</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute<X>(int retryCount, ExceptionHandler<C, V> exceptionHandler)
        where X : IBlock<C, V>
        => Execute(
            blockBuilder.ResolveInstance<X>(),
            retryCount,
            exceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <typeparam name="X">Block type to be retried</typeparam>
    /// <param name="retryCount">Number of attempts to make with DecorrelatedJitterBackoffV2</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute<X>(int retryCount)
        where X : IBlock<C, V>
        => Execute(
            blockBuilder.ResolveInstance<X>(),
            retryCount,
            _defaultExceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <typeparam name="X">Block type to be retried</typeparam>
    /// <param name="delays">Timespans to delay on retry</param>
    /// <param name="exceptionHandler">Delegate to process unhandled exception after all retries have been attempted</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute<X>(IEnumerable<TimeSpan> delays, ExceptionHandler<C, V> exceptionHandler)
        where X : IBlock<C, V>
        => Execute(
            blockBuilder.ResolveInstance<X>(),
            delays,
            exceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <typeparam name="X">Block type to be retried</typeparam>
    /// <param name="retryCount">Number of attempts to make with DecorrelatedJitterBackoffV2</param>
    /// <param name="delay">Delay to use for each attempt</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute<X>(int retryCount, TimeSpan delay)
        where X : IBlock<C, V>
        => Execute(
            blockBuilder.ResolveInstance<X>(),
            Enumerable.Repeat(delay, retryCount),
            _defaultExceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <typeparam name="X">Block type to be retried</typeparam>
    /// <param name="delays">Timespans to delay on retry</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute<X>(IEnumerable<TimeSpan> delays)
        where X : IBlock<C, V>
        => Execute(
            blockBuilder.ResolveInstance<X>(),
            delays,
            _defaultExceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <param name="block">Block to be retried</param>
    /// <param name="exceptionHandler">Delegate to process unhandled exception after all retries have been attempted</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute(IBlock<C, V> block, ExceptionHandler<C, V> exceptionHandler)
        => new(
            block,
            Retries(),
            exceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <param name="block">Block to be retried</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute(IBlock<C, V> block)
        => Execute(
            block,
            _defaultExceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <param name="block">Block to be retried</param>
    /// <param name="retryCount">Number of attempts to make with DecorrelatedJitterBackoffV2</param>
    /// <param name="exceptionHandler">Delegate to process unhandled exception after all retries have been attempted</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute(IBlock<C, V> block, int retryCount, ExceptionHandler<C, V> exceptionHandler)
        => new(
            block,
            Retries(retryCount),
            exceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <param name="block">Block to be retried</param>
    /// <param name="retryCount">Number of attempts to make with DecorrelatedJitterBackoffV2</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute(IBlock<C, V> block, int retryCount)
        => Execute(
            block,
            retryCount,
            _defaultExceptionHandler);


    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <param name="block">Block to be retried</param>
    /// <param name="delays">Timespans to delay on retry</param>
    /// <param name="exceptionHandler">Action to process unhandled exception after all retries have been attempted</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute(IBlock<C, V> block, IEnumerable<TimeSpan> delays, ExceptionHandler<C, V> exceptionHandler)
        => new(
            block,
            delays,
            exceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <param name="block">Block to be retried</param>
    /// <param name="retryCount">Number of attempts to make with DecorrelatedJitterBackoffV2</param>
    /// <param name="delay">Delay to use for each attempt</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute(IBlock<C, V> block, int retryCount, TimeSpan delay)
        => Execute(
            block,
            Enumerable.Repeat(delay, retryCount),
            _defaultExceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <param name="block">Block to be retried</param>
    /// <param name="delays">Timespans to delay on retry</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<C, V> Execute(IBlock<C, V> block, IEnumerable<TimeSpan> delays)
        => Execute(
            block,
            delays,
            _defaultExceptionHandler);

    private static IEnumerable<TimeSpan> Retries(int retryCount = 3)
        => Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(1),
            retryCount: retryCount);

    private static ExceptionHandler<C, V> _defaultExceptionHandler = (c, v, e) =>
        c.SignalBreak(new DefaultFailureState<V>(v)
        {
            FailureReason = e.Message,
            CorrelationId = c.CorrelationId
        });
}

/// <summary>
/// Extension methods for the <see cref="BlockBuilder{C, V}"/> to add retry capabilities.
/// </summary>
public static partial class BuilderExtensions
{
    public static RetryBuilder<C, V> Retry<C, V>(this BlockBuilder<C, V> builder)
        where C : IContext<V>
        => new(builder);
}

/// <summary>
/// Delegate representing an exception handler for a block.
/// </summary>
/// <typeparam name="C">The context type.</typeparam>
/// <typeparam name="V">The value type.</typeparam>
/// <param name="context">The current context.</param>
/// <param name="value">The input value being processed.</param>
/// <param name="exception">The exception that occurred.</param>
public delegate void ExceptionHandler<C, V>(C context, V value, Exception exception)
    where C : IContext<V>
    ;