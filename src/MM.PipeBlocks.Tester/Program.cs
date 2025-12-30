using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Extensions;
using MM.PipeBlocks.Tester;

BenchmarkRunner.Run<PipelineBenchmark>();
return;

var serviceCollection = new ServiceCollection();
serviceCollection.AddTransient<IBlockResolver<ICustomValue>, ServiceProviderBackedResolver<ICustomValue>>();
serviceCollection.AddTransient<BlockBuilder<ICustomValue>>();
serviceCollection.AddTransient<DummyBlock>();
serviceCollection.AddTransient<CustomCodeBlock>();
serviceCollection.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Trace);
});

var serviceProvider = serviceCollection.BuildServiceProvider();

var builder = serviceProvider.GetRequiredService<BlockBuilder<ICustomValue>>();

var pipe = builder
    .CreatePipe("testPipe")
    .Then(b => b.Run(_ => Console.WriteLine("1")))
    //.Then(b => b.Return())
    .Then<DummyBlock>()
    .Then<CustomCodeBlock>()
    .Then(b => b.Run(_ => Console.WriteLine("2")))
    ;

pipe.Execute(new CustomValue1());
pipe.Execute(new CustomValue1());


public class MyAdapter : IAdapter<CustomValue1, CustomValue2>
{
    //private CustomContext? _originalContext;

    //public CustomContext2 Adapt(CustomContext from)
    //{
    //    _originalContext = from;
    //    return new(from.Value)
    //    {
    //        Step = from.Step,
    //        CorrelationId = from.CorrelationId,
    //        Start = DateTime.Now
    //    };
    //}

    //public CustomContext Adapt(CustomContext2 from) => _originalContext ?? new(from.Value)
    //{
    //    Step = from.Step,
    //    CorrelationId = from.CorrelationId
    //};
    public Parameter<CustomValue2> Adapt(Parameter<CustomValue1> from)
        => new(new CustomValue2
            {
                Count = from.Match(_ => 0, x => x.Count),
                Address = string.Empty,
                Start = DateTime.Now,
                Step = from.Match(_ => 0, x => x.Step)
            })
            {
                Context = from.Context
            };

    public Parameter<CustomValue1> Adapt(Parameter<CustomValue2> from)
        => new(new CustomValue1
            {
                Count = from.Match(_ => 0, x => x.Count),
                Name = string.Empty,
                Start = from.Match(_ => DateTime.MinValue, x => x.Start),
                Step = from.Match(_ => 0, x => x.Step)
            })
            {
                Context = from.Context
            };
}

public class DummyBlock : ISyncBlock<ICustomValue>
{
    public Parameter<ICustomValue> Execute(Parameter<ICustomValue> value)
    {
        Console.WriteLine($"Executing {value.Match(_ => 0, x => x.Count)}");
        return value;
    }
}

public class CustomCodeBlock : CodeBlock<ICustomValue>
{
    protected override Parameter<ICustomValue> Execute(Parameter<ICustomValue> parameter, ICustomValue value)
    {
        Console.WriteLine($"Executing {parameter.Match(_ => 0, x => x.Count)}");
        return parameter;
    }
}