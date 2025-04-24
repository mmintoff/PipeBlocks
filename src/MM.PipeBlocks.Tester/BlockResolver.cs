using Microsoft.Extensions.DependencyInjection;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Tester;
public class ServiceProviderBackedResolver<C, V>(IServiceProvider hostProvider) : IBlockResolver<C, V>
    where C : IContext<V>
{
    public X ResolveInstance<X>() where X : IBlock<C, V>
        => hostProvider.GetRequiredService<X>();
}