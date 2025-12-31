using System.Collections.Concurrent;
using System.Numerics;

namespace MM.PipeBlocks.Abstractions;

public class Context
{
    private static class Storage<T>
    {
        public static readonly int Index = Interlocked.Increment(ref _nextIndex) - 1;
    }

    private static int _nextIndex = 0;
    internal readonly ConcurrentDictionary<int, object> _storage = new();

    private ConcurrentDictionary<string, T> GetDictionary<T>()
    {
        var index = Storage<T>.Index;
        return (ConcurrentDictionary<string, T>)_storage.GetOrAdd(
            index,
            _ => new ConcurrentDictionary<string, T>());
    }

    private const string CORRELATION_ID = "CorrelationId";
    private const string IS_FINISHED = "IsFinished";
    private const string IS_FLIPPED = "IsFlipped";

    public Guid CorrelationId => GetOrAdd(CORRELATION_ID, _ => Guid.NewGuid());

    public bool IsFinished
    {
        get => TryGet<bool>(IS_FINISHED, out var val) && val;
        set => Set(IS_FINISHED, value);
    }

    public bool IsFlipped
    {
        get => TryGet<bool>(IS_FLIPPED, out var val) && val;
        set => Set(IS_FLIPPED, value);
    }

    public void Set<T>(string key, T value) => GetDictionary<T>()[key] = value;

    public T GetOrAdd<T>(string key, Func<string, T> valueFactory)
        => GetDictionary<T>().GetOrAdd(key, valueFactory);

    public T GetOrAdd<T>(string key, T value)
        => GetDictionary<T>().GetOrAdd(key, value);

    public T Get<T>(string key)
        => GetDictionary<T>().TryGetValue(key, out var value) ? value : throw new KeyNotFoundException(key);

    public bool TryGet<T>(string key, out T? value)
        => GetDictionary<T>().TryGetValue(key, out value);

    public bool Remove<T>(string key)
        => GetDictionary<T>().TryRemove(key, out _);

    public T Increment<T>(string key, T incrementBy) where T : INumber<T>
        => GetDictionary<T>().AddOrUpdate(key, incrementBy, (k, v) => v + incrementBy);

    public T Increment<T>(string key) where T : INumber<T>
        => GetDictionary<T>().AddOrUpdate(key, T.One, (k, v) => v + T.One);

    public T AddOrUpdate<T>(string key, T addValue, Func<string, T, T> updateFactory)
        => GetDictionary<T>().AddOrUpdate(key, addValue, updateFactory);

    internal Context Clone()
    {
        var clone = new Context();

        foreach (var kvp in _storage)
        {
            // ConcurrentDictionary constructor accepts IEnumerable<KeyValuePair>
            // and creates a copy
            var sourceDict = (dynamic)kvp.Value;
            var clonedDict = Activator.CreateInstance(kvp.Value.GetType(), sourceDict);

            clone._storage[kvp.Key] = clonedDict!;
        }

        return clone;
    }
}