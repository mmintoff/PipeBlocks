using Microsoft.Extensions.DependencyInjection;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Extensions.DependencyInjection;

public class ServiceProviderBackedResolver<V>(IServiceProvider hostProvider) : IBlockResolver<V>
{
    public X ResolveInstance<X>() where X : IBlock<V> => hostProvider.GetRequiredService<X>();
    public IBlockBuilder<Y> CreateBlockBuilder<Y>() => hostProvider.GetRequiredService<BlockBuilder<Y>>();
    public IBlockBuilder<X, Y> CreateBlockBuilder<X, Y>() => hostProvider.GetRequiredService<BlockBuilder<X, Y>>();
}

public class ServiceProviderBackedResolver<VIn, VOut>(IServiceProvider hostProvider) : IBlockResolver<VIn, VOut>
{
    public X ResolveInstance<X>() where X : IBlock<VIn, VOut> => hostProvider.GetRequiredService<X>();
    public IBlockBuilder<X, Y> CreateBlockBuilder<X, Y>() => hostProvider.GetRequiredService<BlockBuilder<X, Y>>();
}