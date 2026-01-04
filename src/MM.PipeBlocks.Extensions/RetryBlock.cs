using MM.PipeBlocks.Abstractions;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System.Runtime.CompilerServices;

namespace MM.PipeBlocks.Extensions;
/// <summary>
/// A block that wraps another block with retry behaviours.
/// </summary>
/// <typeparam name="V">The value type associated with the parameter.</typeparam>
public sealed class RetryBlock<V> : ISyncBlock<V>, IAsyncBlock<V>
{
    private readonly IBlock<V> _block;
    private readonly RetryPolicy<Parameter<V>> _retryPolicy;
    private readonly AsyncRetryPolicy<Parameter<V>> _asyncRetryPolicy;

    private readonly ExceptionHandler<V> _exceptionHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryBlock{V}"/> class.
    /// </summary>
    /// <param name="block">The block to be executed with retry logic.</param>
    /// <param name="delays">A sequence of retry delays.</param>
    /// <param name="exceptionHandler">The handler to be invoked when an exception occurs.</param>
    public RetryBlock(
        IBlock<V> block,
        IEnumerable<TimeSpan> delays,
        ExceptionHandler<V> exceptionHandler)
    {
        _block = block;
        _exceptionHandler = exceptionHandler;

        var delaysArray = delays as TimeSpan[] ?? [.. delays];

        _retryPolicy = Policy
            .Handle<Exception>()
            .OrResult<Parameter<V>>(ShouldRetry)
            .WaitAndRetry(delaysArray);

        _asyncRetryPolicy = Policy
            .Handle<Exception>()
            .OrResult<Parameter<V>>(ShouldRetry)
            .WaitAndRetryAsync(delaysArray);
    }

    public Parameter<V> Execute(Parameter<V> value)
    {
        if (value.IsFailure && !value.Context.IsFlipped)
            return value;

        return ExecuteRetry(value);
    }

    public ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value)
    {
        if (value.IsFailure && !value.Context.IsFlipped)
            return ValueTask.FromResult(value);

        return ExecuteRetryAsync(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Parameter<V> ExecuteRetry(Parameter<V> value)
    {
        var localValue = value;

        try
        {
            return _retryPolicy.Execute(() => BlockExecutor.ExecuteSync(_block, localValue));
        }
        catch (Exception ex)
        {
            _exceptionHandler(value, ex);
            return localValue;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<Parameter<V>> ExecuteRetryAsync(Parameter<V> value)
    {
        var localValue = value;
        try
        {
            return await _asyncRetryPolicy.ExecuteAsync(async () => await BlockExecutor.ExecuteAsync(_block, localValue));
        }
        catch (Exception ex)
        {
            _exceptionHandler(value, ex);
            return localValue;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldRetry(Parameter<V> value) => value.Match(
        static _ => true,
        static _ => false);
}

/// <summary>
/// A builder class for constructing <see cref="RetryBlock{V}"/> instances.
/// </summary>
/// <typeparam name="V">The value type associated with the parameter.</typeparam>
public class RetryBuilder<V>(BlockBuilder<V> blockBuilder)
{
    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <typeparam name="X">Block type to be retried</typeparam>
    /// <returns>RetryBlock</returns>
    public RetryBlock<V> Execute<X>()
        where X : IBlock<V>
        => Execute(blockBuilder.ResolveInstance<X>());

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <typeparam name="X">Block type to be retried</typeparam>
    /// <param name="exceptionHandler">Delegate to process unhandled exception after all retries have been attempted</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<V> Execute<X>(ExceptionHandler<V> exceptionHandler)
        where X : IBlock<V>
        => Execute(blockBuilder.ResolveInstance<X>(),
            exceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <typeparam name="X">Block type to be retried</typeparam>
    /// <param name="retryCount">Number of attempts to make with DecorrelatedJitterBackoffV2</param>
    /// <param name="exceptionHandler">Delegate to process unhandled exception after all retries have been attempted</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<V> Execute<X>(int retryCount, ExceptionHandler<V> exceptionHandler)
        where X : IBlock<V>
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
    public RetryBlock<V> Execute<X>(int retryCount)
        where X : IBlock<V>
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
    public RetryBlock<V> Execute<X>(IEnumerable<TimeSpan> delays, ExceptionHandler<V> exceptionHandler)
        where X : IBlock<V>
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
    public RetryBlock<V> Execute<X>(int retryCount, TimeSpan delay)
        where X : IBlock<V>
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
    public RetryBlock<V> Execute<X>(IEnumerable<TimeSpan> delays)
        where X : IBlock<V>
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
    public RetryBlock<V> Execute(IBlock<V> block, ExceptionHandler<V> exceptionHandler)
        => new(
            block,
            Retries(),
            exceptionHandler);

    /// <summary>
    /// Creates a retry block
    /// </summary>
    /// <param name="block">Block to be retried</param>
    /// <returns>RetryBlock</returns>
    public RetryBlock<V> Execute(IBlock<V> block)
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
    public RetryBlock<V> Execute(IBlock<V> block, int retryCount, ExceptionHandler<V> exceptionHandler)
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
    public RetryBlock<V> Execute(IBlock<V> block, int retryCount)
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
    public RetryBlock<V> Execute(IBlock<V> block, IEnumerable<TimeSpan> delays, ExceptionHandler<V> exceptionHandler)
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
    public RetryBlock<V> Execute(IBlock<V> block, int retryCount, TimeSpan delay)
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
    public RetryBlock<V> Execute(IBlock<V> block, IEnumerable<TimeSpan> delays)
        => Execute(
            block,
            delays,
            _defaultExceptionHandler);

    private static IEnumerable<TimeSpan> Retries(int retryCount = 3)
        => Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(1),
            retryCount: retryCount);

    private static readonly ExceptionHandler<V> _defaultExceptionHandler = (v, e) =>
        v.SignalBreak(new DefaultFailureState<V>(v.Value)
        {
            FailureReason = e.Message,
            CorrelationId = v.Context.CorrelationId
        });
}

/// <summary>
/// Extension methods for the <see cref="BlockBuilder{V}"/> to add retry capabilities.
/// </summary>
public static partial class BuilderExtensions
{
    public static RetryBuilder<V> Retry<V>(this BlockBuilder<V> builder)
        => new(builder);
}

/// <summary>
/// Delegate representing an exception handler for a block.
/// </summary>
/// <typeparam name="V">The value type.</typeparam>
/// <param name="value">The current parameter.</param>
/// <param name="exception">The exception that occurred.</param>
public delegate void ExceptionHandler<V>(Parameter<V> value, Exception exception);