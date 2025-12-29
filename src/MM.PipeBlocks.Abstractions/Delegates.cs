namespace MM.PipeBlocks.Abstractions;

public delegate Parameter<V> PipeBlockDelegate<V>(Parameter<V> input, Action<Context>? configureContext = null);
public delegate ValueTask<Parameter<V>> PipeBlockAsyncDelegate<V>(Parameter<V> input, Action<Context>? configureContext = null);