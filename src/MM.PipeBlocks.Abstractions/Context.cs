using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MM.PipeBlocks.Abstractions;

public sealed class Context
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }

    private object?[] _slots = [];

    private static int _nextId;
    private static readonly ConcurrentDictionary<string, int> _ids =
        new(StringComparer.Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryIdFor(string key, out int id)
        => _ids.TryGetValue(key, out id);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int IdForCreate(string key)
        => _ids.GetOrAdd(key, _ => Interlocked.Increment(ref _nextId) - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Ensure(int id)
    {
        if ((uint)id < (uint)_slots.Length) return;
        Array.Resize(ref _slots, Math.Max(id + 1, _slots.Length == 0 ? 8 : _slots.Length * 2));
    }

    public void Set<T>(string key, T value)
    {
        var id = IdForCreate(key);
        Ensure(id);
        _slots[id] = value!;
    }

    public bool TryGet<T>(string key, out T? value)
    {
        if (!TryIdFor(key, out var id))
        {
            value = default;
            return false;
        }

        if ((uint)id < (uint)_slots.Length && _slots[id] is T t)
        {
            value = t;
            return true;
        }

        value = default;
        return false;
    }

    public T Get<T>(string key)
    {
        if (!TryIdFor(key, out var id) ||
            (uint)id >= (uint)_slots.Length ||
            _slots[id] is null)
            throw new KeyNotFoundException(key);

        if (_slots[id] is T t) return t;

        throw new InvalidCastException(
            $"Key '{key}' contains '{_slots[id]!.GetType().FullName}', not '{typeof(T).FullName}'.");
    }

    public T GetOrAdd<T>(string key, Func<string, T> factory)
    {
        var id = IdForCreate(key);
        Ensure(id);

        if (_slots[id] is T t) return t;

        var created = factory(key);
        _slots[id] = created!;
        return created;
    }

    public bool Remove(string key)
    {
        if (!TryIdFor(key, out var id)) return false;
        if ((uint)id >= (uint)_slots.Length) return false;
        _slots[id] = null;
        return true;
    }

    public T Increment<T>(string key, T incrementBy) where T : INumber<T>
    {
        var id = IdForCreate(key);
        Ensure(id);

        if (_slots[id] is T current)
        {
            var next = current + incrementBy;
            _slots[id] = next;
            return next;
        }

        if (_slots[id] is null)
        {
            _slots[id] = incrementBy;
            return incrementBy;
        }

        throw new InvalidCastException(
            $"Key '{key}' contains '{_slots[id]!.GetType().FullName}', not '{typeof(T).FullName}'.");
    }

    public T Increment<T>(string key) where T : INumber<T>
        => Increment(key, T.One);

    public Context Clone()
    {
        var c = new Context
        {
            CorrelationId = CorrelationId,
            IsFinished = IsFinished,
            IsFlipped = IsFlipped,
            _slots = (object?[])_slots.Clone()
        };
        return c;
    }
}