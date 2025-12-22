using System.Collections.Concurrent;
using System.Numerics;

namespace MM.PipeBlocks.Abstractions;

public class Parameter<V> : IEither<IFailureState<V>, V>
{
    private Either<IFailureState<V>, V> _either;
    private readonly ConcurrentDictionary<string, object> _metaData = new();

    public Parameter(IFailureState<V> left) => _either = new(left);
    public Parameter(V right) => _either = new(right);

    public static implicit operator Parameter<V>(V right) => new(right);

    public V Value { get => _either.Match(x => x.Value, x => x); }
    public ConcurrentDictionary<string, object> MetaData => _metaData;
    public Guid CorrelationId { get => Context.TryGet("CorrelationId", out Guid id) ? id : default; }

    public void SetMetaData<T>(string key, T value) => _metaData[key] = value!;
    public T? GetMetaData<T>(string key) =>
        _metaData.TryGetValue(key, out var value) ? (T)value : default;

    public bool TryGetMetaData<T>(string key, out T? value)
    {
        if(_metaData.TryGetValue(key, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }
        value = default;
        return false;
    }

    public T IncrementMetaData<T>(string key, T incrementBy)
        where T : INumber<T>
    {
        return (T)_metaData.AddOrUpdate(key, incrementBy!, (k, v) => (T)v + incrementBy);
    }

    public T IncrementMetaData<T>(string key) where T : INumber<T>
    {
        return (T)_metaData.AddOrUpdate(key, T.One!, (k, v) => (T)v + T.One);
    }

    public void SignalBreak()
    {
        Context.IsFinished = true;
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
}