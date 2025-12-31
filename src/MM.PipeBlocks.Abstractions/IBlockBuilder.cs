namespace MM.PipeBlocks.Abstractions;

public interface IBlockBuilder<V>
{
    IPipeBlock<V> CreatePipe(string pipeName);
    X ResolveInstance<X>() where X : IBlock<V>;
    IBlockBuilder<V2> CreateBlockBuilder<V2>();
}