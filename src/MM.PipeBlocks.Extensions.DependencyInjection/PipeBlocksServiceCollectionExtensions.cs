using Microsoft.Extensions.DependencyInjection;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Extensions.DependencyInjection;

public static class PipeBlocksServiceCollectionExtensions
{
    public static PipeBlockConfigurator AddPipeBlocks(this IServiceCollection services)
    {
        services.AddTransient(typeof(IBlockResolver<>), typeof(ServiceProviderBackedResolver<>));
        services.AddTransient(typeof(BlockBuilder<>));

        return new PipeBlockConfigurator(services);
    }
}

public class PipeBlockConfigurator(IServiceCollection services)
{
    public PipeBlockConfigurator AddTransientBlock<T>() where T : class, IBlock
    {
        services.AddTransient<T>();
        return this;
    }

    public PipeBlockConfigurator AddSingletonBlock<T>() where T : class, IBlock
    {
        services.AddSingleton<T>();
        return this;
    }

    public PipeBlockConfigurator AddScopedBlock<T>() where T : class, IBlock
    {
        services.AddScoped<T>();
        return this;
    }
}