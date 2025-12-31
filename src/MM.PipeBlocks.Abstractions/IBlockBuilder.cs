using Microsoft.Extensions.Options;

namespace MM.PipeBlocks.Abstractions;

public interface IBlockBuilder<V>
{
    IPipeBlock<V> CreatePipe(IOptions<PipeBlockOptions> options);
    X ResolveInstance<X>() where X : IBlock<V>;
    IBlockBuilder<V2> CreateBlockBuilder<V2>();
}