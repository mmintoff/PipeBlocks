using System.Collections.Concurrent;
using System.Numerics;

namespace MM.PipeBlocks.Abstractions;

public static class Context
{
    private static class Storage<T>
    {
        public static readonly AsyncLocal<ConcurrentDictionary<string, T>> Values = new();
        public static readonly object Lock = new();

        static Storage()
        {
            CloneActions.Add((sourceDict) =>
            {
                if (sourceDict is ConcurrentDictionary<string, T> typedDict && !typedDict.IsEmpty)
                    Values.Value = new ConcurrentDictionary<string, T>(typedDict);
            });

            CaptureActions.Add(() => Values.Value!);
        }
    }

    private static readonly ConcurrentBag<Action<object>> CloneActions = [];
    private static readonly ConcurrentBag<Func<object>> CaptureActions = [];

    private const string CORRELATION_ID = "CorrelationId";
    private const string IS_FINISHED = "IsFinished";
    private const string IS_FLIPPED = "IsFlipped";

    public static Guid CorrelationId
    {
        get => Context.GetOrAdd(CORRELATION_ID, x => Guid.NewGuid());
    }

    public static bool IsFinished
    {
        get => Context.TryGet<bool>(IS_FINISHED, out var isFinished) && isFinished;
        set => Context.Set(IS_FINISHED, value);
    }

    public static bool IsFlipped
    {
        get => Context.TryGet<bool>(IS_FLIPPED, out var isFlipped) && isFlipped;
        set => Context.Set(IS_FLIPPED, value);
    }

    public static ConcurrentDictionary<string, T> GetDictionary<T>()
    {
        var dict = Storage<T>.Values.Value;
        if (dict == null)
        {
            lock (Storage<T>.Lock)
            {
                dict = Storage<T>.Values.Value;
                if (dict == null)
                {
                    dict = new ConcurrentDictionary<string, T>();
                    Storage<T>.Values.Value = dict;
                }
            }
        }
        return dict;
    }

    public static void Set<T>(string key, T value)
    {
        GetDictionary<T>()[key] = value;
    }

    public static T GetOrAdd<T>(string key, Func<string, T> valueFactory)
        => GetDictionary<T>().GetOrAdd(key, valueFactory);

    public static T GetOrAdd<T>(string key, T value)
        => GetDictionary<T>().GetOrAdd(key, value);

    public static T Get<T>(string key)
        => GetDictionary<T>().TryGetValue(key, out var value) ? value : throw new KeyNotFoundException(key);

    public static bool TryGet<T>(string key, out T? value)
    {
        var dict = GetDictionary<T>();
        return dict.TryGetValue(key, out value);
    }

    public static bool Remove<T>(string key)
    {
        var dict = Storage<T>.Values.Value; // We do not want to initialize for a Remove
        return dict?.TryRemove(key, out _) ?? false;
    }

    public static T Increment<T>(string key, T incrementBy)
        where T : INumber<T>
    {
        var dict = GetDictionary<T>();
        return dict.AddOrUpdate(key, incrementBy, (k, v) => v + incrementBy);
    }

    public static T Increment<T>(string key)
        where T : INumber<T>
    {
        var dict = GetDictionary<T>();
        return dict.AddOrUpdate(key, T.One, (k, v) => v + T.One);
    }

    public static T AddOrUpdate<T>(string key, T addValue, Func<string, T, T> updateFactory)
    {
        var dict = GetDictionary<T>();
        return dict.AddOrUpdate(key, addValue, updateFactory);
    }

    public static ContextSnapshot Capture()
    {
        var snapshots = CaptureActions.Select(capture => capture()).ToArray();
        return new ContextSnapshot(snapshots);
    }

    public class ContextSnapshot
    {
        private readonly object[] _snapshots;

        internal ContextSnapshot(object[] snapshots) => _snapshots = snapshots;

        public void Apply()
        {
            var cloneActionsList = CloneActions.ToArray();
            for (int i = 0; i < cloneActionsList.Length && i < _snapshots.Length; i++)
            {
                if (_snapshots[i] != null)
                {
                    cloneActionsList[i](_snapshots[i]);
                }
            }
        }
    }
}