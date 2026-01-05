using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Extensions;
using MM.PipeBlocks.Extensions.DependencyInjection;
using MM.PipeBlocks.Tester;

//BenchmarkRunner.Run<OverheadBenchmark>();
//return;

var serviceCollection = new ServiceCollection();

serviceCollection.AddPipeBlocks()
    .AddTransientBlock<DummyBlock>()
    .AddTransientBlock<CustomCodeBlock>()
    ;

serviceCollection.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Trace);
});

var serviceProvider = serviceCollection.BuildServiceProvider();

var builder = serviceProvider.GetRequiredService<BlockBuilder<CustomValue1>>();

var pipe = builder
    .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "testPipe" }))
        .Then(b => b.Run(_ => Console.WriteLine("1")))
        .Then(b => b.Run(_ => Console.WriteLine("2")))
    .Map(Options.Create(new PipeBlockOptions { PipeName = "map pipe 1" }), c => Map(c, c => new CustomValue2()))
    .Then(b => b.Run(_ => Console.WriteLine("3")))
    .Map(Options.Create(new PipeBlockOptions { PipeName = "map pipe 2" }), c => Map(c, c => new CustomValue3()))
    .Then(b => b.Run(_ => Console.WriteLine("4")))
    .Map(Options.Create(new PipeBlockOptions { PipeName = "map pipe 3" }), c => Map(c, c => new CustomValue4()))
    .Then(b => b.Run(_ => Console.WriteLine("5")))
    ;

pipe.Execute(new CustomValue1());
//pipe.Execute(new CustomValue2());

static Parameter<V2> Map<V1, V2>(Parameter<V1> from, Func<V1, V2> mapFunc)
    => from.Match(
        failure => new Parameter<V2>(new DefaultFailureState<V2>(mapFunc(failure.Value))),
        success => new Parameter<V2>(mapFunc(success))
    );

//public class MyAdapter : IAdapter<CustomValue1, CustomValue2>
//{
//    public Parameter<CustomValue2> Adapt(Parameter<CustomValue1> from, Parameter<CustomValue2>? original = null)
//        => new(new CustomValue2
//           {
//               Count = from.Match(_ => 0, x => x.Count),
//               Address = string.Empty,
//               Start = DateTime.Now,
//               Step = from.Match(_ => 0, x => x.Step)
//           })
//           {
//               Context = from.Context
//           };

//    public Parameter<CustomValue1> Adapt(Parameter<CustomValue2> from, Parameter<CustomValue1>? original = null)
//        => new(new CustomValue1
//            {
//                Count = from.Match(_ => 0, x => x.Count),
//                Name = original?.Match(_ => string.Empty, x => x.Name) ?? String.Empty,
//                Start = from.Match(_ => DateTime.MinValue, x => x.Start),
//                Step = from.Match(_ => 0, x => x.Step)
//            })
//            {
//                Context = from.Context
//            };
//}

public class DummyBlock : ISyncBlock<CustomValue1>
{
    public Parameter<CustomValue1> Execute(Parameter<CustomValue1> value)
    {
        Console.WriteLine($"Executing {value.Match(_ => 0, x => x.Count)}");
        return value;
    }
}

public class CustomCodeBlock : CodeBlock<CustomValue1>
{
    protected override Parameter<CustomValue1> Execute(Parameter<CustomValue1> parameter, CustomValue1 value)
    {
        Console.WriteLine($"Executing {parameter.Match(_ => 0, x => x.Count)}");
        return parameter;
    }
}