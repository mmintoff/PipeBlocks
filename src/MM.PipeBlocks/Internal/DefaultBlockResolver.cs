using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Internal;
internal class DefaultBlockResolver<V> : IBlockResolver<V>
{
    public X ResolveInstance<X>()
        where X : IBlock<V>
        => Activator.CreateInstance<X>();

    public IBlockBuilder<Y> CreateBlockBuilder<Y>()
        => new BlockBuilder<Y>();

    public IBlockBuilder<X, Y> CreateBlockBuilder<X, Y>()
        => new BlockBuilder<X, Y>();
}

internal class DefaultBlockResolver<VIn, VOut> : IBlockResolver<VIn, VOut>
{
    public X ResolveInstance<X>()
        where X : IBlock<VIn, VOut>
        => Activator.CreateInstance<X>();

    public IBlockBuilder<X, Y> CreateBlockBuilder<X, Y>()
        => new BlockBuilder<X, Y>();
}