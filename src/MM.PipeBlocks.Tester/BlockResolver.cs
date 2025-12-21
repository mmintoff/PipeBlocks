using Microsoft.Extensions.DependencyInjection;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Tester;
public class ServiceProviderBackedResolver<V>(IServiceProvider hostProvider) : IBlockResolver<V>
{
    public X ResolveInstance<X>() where X : IBlock<V>
        => hostProvider.GetRequiredService<X>();
}