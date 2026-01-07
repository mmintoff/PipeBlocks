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
    .AddTransientBlock<V2MapBlock>()
    .AddTransientBlock<V2CodeBlock>()
    .AddTransientBlock<V3MapBlock>()
    .AddTransientBlock<V3CodeBlock>()
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
    .ThenMap<CustomValue2>(b => b.ResolveInstance<V2MapBlock>())
    .Then<V2CodeBlock>()
    .ThenMap<CustomValue3, V3MapBlock>()
    .Then<V3CodeBlock>()
    ;

var r = pipe.Execute(new CustomValue1());
Console.WriteLine(r.ToString());

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

public class V2MapBlock : CodeBlock<CustomValue1, CustomValue2>
{
    protected override Parameter<CustomValue2> Execute(Parameter<CustomValue1> parameter, CustomValue1 extractedValue)
    {
        Console.WriteLine($"Received {parameter}, sending CustomValue2");
        return new CustomValue2();
    }
}

public class V2CodeBlock : CodeBlock<CustomValue2>
{
    protected override Parameter<CustomValue2> Execute(Parameter<CustomValue2> parameter, CustomValue2 value)
    {
        Console.WriteLine($"Executing {parameter.Match(_ => 0, x => x.Count)}");
        return parameter;
    }
}

public class V3MapBlock : CodeBlock<CustomValue2, CustomValue3>
{
    protected override Parameter<CustomValue3> Execute(Parameter<CustomValue2> parameter, CustomValue2 extractedValue)
    {
        Console.WriteLine($"Received {parameter}, sending CustomValue3");
        return new CustomValue3();
    }
}

public class V3CodeBlock : CodeBlock<CustomValue3>
{
    protected override Parameter<CustomValue3> Execute(Parameter<CustomValue3> parameter, CustomValue3 value)
    {
        Console.WriteLine($"Executing {parameter.Match(_ => 0, x => x.Count)}");
        return parameter;
    }
}