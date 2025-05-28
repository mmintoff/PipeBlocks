using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Tester;
using BenchmarkDotNet.Running;
using MM.PipeBlocks.Blocks;
using MM.PipeBlocks.Extensions;

//BenchmarkRunner.Run<PipelineBenchmark>();

var serviceCollection = new ServiceCollection();
serviceCollection.AddTransient<IBlockResolver<IContext<ICustomValue>, ICustomValue>, ServiceProviderBackedResolver<IContext<ICustomValue>, ICustomValue>>();
serviceCollection.AddTransient<BlockBuilder<IContext<ICustomValue>, ICustomValue>>();
serviceCollection.AddTransient<DummyBlock>();
serviceCollection.AddTransient<CustomCodeBlock>();
serviceCollection.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Trace);
});

var serviceProvider = serviceCollection.BuildServiceProvider();

var builder = serviceProvider.GetRequiredService<BlockBuilder<IContext<ICustomValue>, ICustomValue>>();

var pipe = builder
    .CreatePipe("testPipe")
    .Then(b => b.Run(_ => Console.WriteLine("1")))
    .Then(b => b.Return())
    .Then<DummyBlock>()
    .Then<CustomCodeBlock>()
    .Then(b => b.Run(_ => Console.WriteLine("2")))
    ;

pipe.Execute(new CustomContext(new CustomValue1()));
pipe.Execute(new CustomContext2(new CustomValue1()));


public class MyAdapter : IAdapter<CustomContext, ICustomValue, CustomContext2, ICustomValue>
{
    private CustomContext? _originalContext;

    public CustomContext2 Adapt(CustomContext from)
    {
        _originalContext = from;
        return new(from.Value)
        {
            Step = from.Step,
            CorrelationId = from.CorrelationId,
            Start = DateTime.Now
        };
    }

    public CustomContext Adapt(CustomContext2 from) => _originalContext ?? new(from.Value)
    {
        Step = from.Step,
        CorrelationId = from.CorrelationId
    };
}

public class DummyBlock : ISyncBlock<IContext<ICustomValue>, ICustomValue>
{
    public IContext<ICustomValue> Execute(IContext<ICustomValue> context)
    {
        Console.WriteLine($"Executing {context.Value.Match(_ => 0, x => x.Count)}");
        return context;
    }
}

public class CustomCodeBlock : CodeBlock<IContext<ICustomValue>, ICustomValue>
{
    protected override IContext<ICustomValue> Execute(IContext<ICustomValue> context, ICustomValue value)
    {
        Console.WriteLine($"Executing {context.Value.Match(_ => 0, x => x.Count)}");
        return context;
    }
}