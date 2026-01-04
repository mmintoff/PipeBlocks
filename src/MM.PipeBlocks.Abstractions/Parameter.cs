namespace MM.PipeBlocks.Abstractions;

public class Parameter<V> : IEither<IFailureState<V>, V>
{
    private Either<IFailureState<V>, V> _either;
    public Context Context { get; set; } = new();

    public Parameter(IFailureState<V> left) => _either = new(left);
    public Parameter(V right) => _either = new(right);

    public static implicit operator Parameter<V>(V right) => new(right);

    public V Value { get => _either.Match(x => x.Value, x => x); }
    public Guid CorrelationId { get => Context.CorrelationId; }

    public bool IsFailure { get => _either.IsLeft; }

    public void SignalBreak()
    {
        Context.IsFinished = true;
    }

    public void SignalBreak(string failureReason)
    {
        Context.IsFinished = true;
        _either = new Either<IFailureState<V>, V>(new DefaultFailureState<V>(Value)
        {
            CorrelationId = Context.CorrelationId,
            FailureReason = failureReason
        });
    }

    public void SignalBreak(IFailureState<V> failureState)
    {
        Context.IsFinished = true;

        if(failureState.CorrelationId == default)
            failureState.CorrelationId = Context.CorrelationId;

        _either = new Either<IFailureState<V>, V>(failureState);
    }

    public void Match(Action<IFailureState<V>> leftAction, Action<V> rightAction)
        => _either.Match(leftAction, rightAction);

    public T Match<T>(Func<IFailureState<V>, T> leftFunc, Func<V, T> rightFunc)
        => _either.Match(leftFunc, rightFunc);

    public ValueTask MatchAsync(Func<IFailureState<V>, ValueTask> leftAction, Func<V, ValueTask> rightAction)
        => _either.MatchAsync(leftAction, rightAction);

    public ValueTask<T> MatchAsync<T>(Func<IFailureState<V>, ValueTask<T>> leftTask, Func<V, ValueTask<T>> rightTask)
        => _either.MatchAsync(leftTask, rightTask);

    public Parameter<V> Clone(V? newValue = default)
    {
        var clonedValue = newValue ?? this.Value;
        var clonedContext = Context.Clone();

        var clonedEither = _either.Match(
            left => new Either<IFailureState<V>, V>(left),
            right => new Either<IFailureState<V>, V>(clonedValue)
        );

        var clonedParameter = new Parameter<V>(default(V))
        {
            _either = clonedEither,
            Context = clonedContext
        };

        return clonedParameter;
    }
}