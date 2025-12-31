using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Internal;
internal class DefaultBlockResolver<V> : IBlockResolver<V>
{
    public X ResolveInstance<X>()
        where X : IBlock<V>
        => Activator.CreateInstance<X>();

    public IBlockBuilder<Y> CreateBlockBuilder<Y>()
        => new BlockBuilder<Y>();
}