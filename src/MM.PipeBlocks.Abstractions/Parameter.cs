namespace MM.PipeBlocks.Abstractions;

public class Parameter<V> : IEither<IFailureState, V>
{
    private Either<IFailureState, V> _either;
    public Context Context { get; set; } = new();

    public Parameter(IFailureState left) => _either = new(left);
    public Parameter(V right) => _either = new(right);

    public static implicit operator Parameter<V>(V right) => new(right);

    public V Value
    {
        get => _either.Match(
            f => f.TryGetValue<V>(out var result)
                ? result!
                : throw new InvalidOperationException($"Cannot access Value of type {typeof(V).Name} from failure state. The failure contains a value of type {f.Value.GetType().Name}."),
            s => s
        );
    }

    public IFailureState Failure => _either.Match(
        left => left,
        right => throw new InvalidOperationException("Not in failure state")
    );

    public bool TryGetValue(out V value)
    {
        if (!IsFailure)
        {
            value = _either.Match(
                f => f.TryGetValue<V>(out var result) ? result!
                    : throw new InvalidOperationException($"Cannot extract value of type {typeof(V).Name} from failure state containing {f.Value.GetType().Name}. This indicates an internal inconsistency - parameter is not in failure state but contains failure data."),
                right => right);
            return true;
        }
        value = default!;
        return false;
    }

    public bool TryGetFailure(out IFailureState failure)
    {
        if (IsFailure)
        {
            failure = _either.Match(
                left => left,
                _ => default!);
            return true;
        }
        failure = default!;
        return false;
    }

    public Guid CorrelationId { get => Context.CorrelationId; }

    public bool IsFailure { get => _either.IsLeft; }

    public void SignalBreak()
    {
        Context.IsFinished = true;
    }

    public void SignalBreak(string failureReason)
    {
        Context.IsFinished = true;
        _either = new Either<IFailureState, V>(new DefaultFailureState<V>(Value)
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

        _either = new Either<IFailureState, V>(failureState);
    }

    public void Match(Action<IFailureState> leftAction, Action<V> rightAction)
        => _either.Match(leftAction, rightAction);

    public T Match<T>(Func<IFailureState, T> leftFunc, Func<V, T> rightFunc)
        => _either.Match(leftFunc, rightFunc);

    public ValueTask MatchAsync(Func<IFailureState, ValueTask> leftAction, Func<V, ValueTask> rightAction)
        => _either.MatchAsync(leftAction, rightAction);

    public ValueTask<T> MatchAsync<T>(Func<IFailureState, ValueTask<T>> leftTask, Func<V, ValueTask<T>> rightTask)
        => _either.MatchAsync(leftTask, rightTask);

    public Parameter<V> Clone(V? newValue = default)
    {
        var clonedValue = newValue ?? this.Value;
        var clonedContext = Context.Clone();

        var clonedEither = _either.Match(
            left => new Either<IFailureState, V>(left),
            right => new Either<IFailureState, V>(clonedValue)
        );

        var clonedParameter = new Parameter<V>(default(V))
        {
            _either = clonedEither,
            Context = clonedContext
        };

        return clonedParameter;
    }
}