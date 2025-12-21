using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks;

public class ContextBuilder
{
    private readonly List<Action> _setters = new();
    public ContextBuilder With<T>(string key, T value)
    {
        _setters.Add(() => Context.Set(key, value));
        return this;
    }

    public void Apply()
    {
        foreach (var setter in _setters)
            setter();
    }
}