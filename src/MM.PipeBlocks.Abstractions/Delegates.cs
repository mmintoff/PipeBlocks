namespace MM.PipeBlocks.Abstractions;

public delegate Parameter<V> PipeBlockDelegate<V>(Parameter<V> input);
public delegate ValueTask<Parameter<V>> PipeBlockAsyncDelegate<V>(Parameter<V> input);