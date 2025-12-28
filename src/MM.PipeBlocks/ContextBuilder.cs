using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks;

public class ContextBuilder
{
    private readonly List<Action> _setters = new();
    private readonly Context _context;

    public ContextBuilder(Context context)
    {
        _context = context;
    }

    public ContextBuilder With<T>(string key, T value)
    {
        _setters.Add(() => _context.Set(key, value));
        return this;
    }

    public void Apply()
    {
        foreach (var setter in _setters)
            setter();
    }
}