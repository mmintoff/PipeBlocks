namespace MM.PipeBlocks.Abstractions;

public delegate C PipeBlockDelegate<C, V>(C input)
    where C : IContext<V>;
public delegate ValueTask<C> PipeBlockAsyncDelegate<C, V>(C input)
    where C : IContext<V>;