using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Internal;
internal class DefaultBlockResolver<C, V> : IBlockResolver<C, V>
    where C : IContext<V>
{
    public X ResolveInstance<X>()
        where X : IBlock<C, V>
        => Activator.CreateInstance<X>();
}