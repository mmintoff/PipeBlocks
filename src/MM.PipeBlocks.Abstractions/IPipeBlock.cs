namespace MM.PipeBlocks.Abstractions;

public interface IPipeBlock<V> : ISyncBlock<V>, IAsyncBlock<V>
{
    Parameter<V> Execute(Parameter<V> value, Action<Context>? configureContext);
    ValueTask<Parameter<V>> ExecuteAsync(Parameter<V> value, Action<Context>? configureContext);
}