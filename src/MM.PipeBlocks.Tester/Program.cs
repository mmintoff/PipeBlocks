using MM.PipeBlocks.Tester;
using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Blocks;
using MM.PipeBlocks.Extensions;

var serviceCollection = new ServiceCollection();
serviceCollection.AddTransient<IBlockResolver<CustomContext, ICustomValue>, ServiceProviderBackedResolver<CustomContext, ICustomValue>>();
serviceCollection.AddTransient<IBlockResolver<CustomContext2, ICustomValue>, ServiceProviderBackedResolver<CustomContext2, ICustomValue>>();
serviceCollection.AddTransient<BlockBuilder<CustomContext, ICustomValue>>();
serviceCollection.AddTransient<BlockBuilder<CustomContext2, ICustomValue>>();
serviceCollection.AddTransient<DummyBlock>();
serviceCollection.AddTransient<CustomCodeBlock>();
serviceCollection.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Trace);
});

var serviceProvider = serviceCollection.BuildServiceProvider();

var builder = serviceProvider.GetRequiredService<BlockBuilder<CustomContext, ICustomValue>>();
var builder2 = serviceProvider.GetRequiredService<BlockBuilder<CustomContext2, ICustomValue>>();

var adapterPipe = builder.CreatePipe("adapterPipe", builder2, new MyAdapter())
    .Then(b => b.Run(c => Console.WriteLine(c.Start)))
    ;

var startFromPipe = builder.CreatePipe("steppedPipe", c => c.Step)
    .Then(builder.Run(c => Console.WriteLine("0")))
    .Then(builder.Run(c => Console.WriteLine("1")))
    .Then(builder.Run(c => Console.WriteLine("2")))
    .Then(builder.Run(c => Console.WriteLine("3")))
    .Then(adapterPipe)
    ;

var pipe = builder
    .CreatePipe("mainPipe")
    .Then(b => b.Run(startFromPipe.ToFunc()))
    .Then(b => b.Run(Do))
    .Then(b => b.Run(DoAsync))
    .Then(b => b.Run(DoAsync2))
    ;

pipe.Execute(new CustomContext(new CustomValue1
{
    Count = 57,
    Name = "Henry"
})
{
    Step = 2
});

void Do()
{
    Console.WriteLine("Method: Do");
}

async Task DoAsync()
{
    Console.WriteLine("Method: DoAsync");
    await Task.Delay(1);
}

async Task<bool> DoAsync2()
{
    Console.WriteLine("Method: DoAsync2");
    await Task.Delay(1);
    return false;
}

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

public class DummyBlock : ISyncBlock<CustomContext, ICustomValue>
{
    public CustomContext Execute(CustomContext context)
    {
        Console.WriteLine($"Executing {context.Value.Match(_ => 0, x => x.Count)}");
        return context;
    }
}

public class CustomCodeBlock : CodeBlock<CustomContext, ICustomValue>
{
    protected override CustomContext Execute(CustomContext context, ICustomValue value)
    {
        Console.WriteLine($"Executing {context.Value.Match(_ => 0, x => x.Count)}");
        return context;
    }
}