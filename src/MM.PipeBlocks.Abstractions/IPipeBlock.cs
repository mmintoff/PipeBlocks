namespace MM.PipeBlocks.Abstractions;

public interface IPipeBlock<VIn, VOut> : ISyncBlock<VIn, VOut>, IAsyncBlock<VIn, VOut>
{
    PipeBlockOptions Options { get; }
    IBlockBuilder<VOut> BlockBuilder { get; }
    Parameter<VOut> Execute(Parameter<VIn> value, Action<Context>? configureContext);
    ValueTask<Parameter<VOut>> ExecuteAsync(Parameter<VIn> value, Action<Context>? configureContext);
}

public interface IPipeBlock<V> : IPipeBlock<V, V>, ISyncBlock<V>, IAsyncBlock<V> { }