using Microsoft.Extensions.DependencyInjection;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Extensions.DependencyInjection;

public class ServiceProviderBackedResolver<V>(IServiceProvider hostProvider) : IBlockResolver<V>
{
    public X ResolveInstance<X>() where X : IBlock<V> => hostProvider.GetRequiredService<X>();
    public IBlockBuilder<Y> CreateBlockBuilder<Y>() => hostProvider.GetRequiredService<BlockBuilder<Y>>();
}