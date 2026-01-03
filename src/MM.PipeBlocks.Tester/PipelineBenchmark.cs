using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Extensions;
using MM.PipeBlocks.Extensions.DependencyInjection;

namespace MM.PipeBlocks.Tester;

[MemoryDiagnoser]
[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses, HardwareCounter.InstructionRetired)]
[DisassemblyDiagnoser(printSource: true)]
public class PipelineBenchmark
{
    private PipeBlock<CustomValue1> _pipe1;
    private PipeBlock<CustomValue1> _pipe2;
    private readonly CustomValue1 _value = new()
    {
        Count = 57,
        Name = "Henry"
    };

    [GlobalSetup]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddPipeBlocks()
            .AddTransientBlock<DummyBlock>()
            .AddTransientBlock<CustomCodeBlock>()
            ;

        serviceCollection.AddLogging(configure =>
        {
            configure.ClearProviders();
            configure.AddConsole();
            configure.SetMinimumLevel(LogLevel.Warning);
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var builder = serviceProvider.GetRequiredService<BlockBuilder<CustomValue1>>();

        var adapterPipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "adapterPipe" }), new MyAdapter())
            .Then(b => b.Run(p => { p.Value.Start.AddMinutes(1); }))
            ;

        var startFromPipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "steppedPipe" }), v => v.Value.Step)
            .Then(builder.Run(c => { _ = 1 + 1; }))
            .Then(builder.Run(c => { _ = 1 * 1; }))
            .Then(builder.Run(c => { _ = 1 / 1; }))
            .Then(builder.Run(c => { _ = 1 - 1; }))
            ;

        _pipe1 = builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "Performance Pipe without Exception Handling" }))
            .Then(startFromPipe)
            .Then(b => b.Run(Do))
            .Then(b => b.Run(DoAsync))
            .Then(b => b.Run(DoAsync2))
            ;

        _pipe2 = builder
            .CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "Performance Pipe with Exception Handling" , HandleExceptions = true}))
            .Then(startFromPipe)
            .Then(b => b.Run(Do))
            .Then(b => b.Run(DoAsync))
            .Then(b => b.Run(DoAsync2))
            ;
    }

    void Do()
    {
    }

    async Task DoAsync()
    {
        await Task.Delay(0);
    }

    async Task<bool> DoAsync2()
    {
        await Task.Delay(0);
        return false;
    }

    //void Fibonnaci()
    //{
    //    int Fib(int n) => n <= 1 ? n : Fib(n - 1) + Fib(n - 2);
    //    var result = Fib(35);
    //}

    //void PrimeCheck()
    //{
    //    bool IsPrime(int n)
    //    {
    //        if (n < 2) return false;
    //        for (int i = 2; i * i <= n; i++)
    //            if (n % i == 0)
    //                return false;
    //        return true;
    //    }

    //    int count = 0;
    //    for (int i = 2; i < 1_000_000; i++)
    //        if (IsPrime(i))
    //            count++;
    //}

    [Benchmark]
    public Parameter<CustomValue1> SyncRegularExecution_no_error_handling()
        => _pipe1.Execute(_value);

    [Benchmark]
    public async ValueTask<Parameter<CustomValue1>> AsyncRegularExecution_no_error_handling()
        => await _pipe1.ExecuteAsync(_value);

    [Benchmark]
    public Parameter<CustomValue1> SyncRegularExecution_error_handling()
        => _pipe2.Execute(_value);

    [Benchmark]
    public async ValueTask<Parameter<CustomValue1>> AsyncRegularExecution_error_handling()
        => await _pipe2.ExecuteAsync(_value);
}