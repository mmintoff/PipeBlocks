using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Extensions;
using MM.PipeBlocks.Extensions.DependencyInjection;

namespace MM.PipeBlocks.Tester;

[MemoryDiagnoser]
[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses, HardwareCounter.InstructionRetired)]
[DisassemblyDiagnoser(printSource: true)]
public class PipelineBenchmark
{
    private IPipeBlock<CustomValue1> _pipe;
    private CustomValue1 _value = new()
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

        var builder1 = serviceProvider.GetRequiredService<BlockBuilder<CustomValue1>>();
        var builder2 = serviceProvider.GetRequiredService<BlockBuilder<CustomValue2>>();

        var adapterPipe = builder1.CreatePipe("adapterPipe", builder2, new MyAdapter())
            .Then(b => b.Run(p => { p.Value.Start.AddMinutes(1); }))
            ;

        var startFromPipe = builder1.CreatePipe("steppedPipe", v => v.Value.Step)
            .Then(builder1.Run(c => { var a = 1 + 1; }))
            .Then(builder1.Run(c => { var a = 1 * 1; }))
            .Then(builder1.Run(c => { var a = 1 / 1; }))
            .Then(builder1.Run(c => { var a = 1 - 1; }))
            ;

        _pipe = builder1
            .CreatePipe("mainPipe")
            .Then(startFromPipe)
            .Then(b => b.Run(Do))
            .Then(b => b.Run(DoAsync))
            .Then(b => b.Run(DoAsync2))
            //.Then(b => b.Run(Fibonnaci))
            //.Then(b => b.Run(PrimeCheck))
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

    void Fibonnaci()
    {
        int Fib(int n) => n <= 1 ? n : Fib(n - 1) + Fib(n - 2);
        var result = Fib(35);
    }

    void PrimeCheck()
    {
        bool IsPrime(int n)
        {
            if (n < 2) return false;
            for (int i = 2; i * i <= n; i++)
                if (n % i == 0)
                    return false;
            return true;
        }

        int count = 0;
        for (int i = 2; i < 1_000_000; i++)
            if (IsPrime(i))
                count++;
    }

    [Benchmark]
    public Parameter<CustomValue1> SyncRegularExecution()
        => _pipe.Execute(_value);

    [Benchmark]
    public async ValueTask<Parameter<CustomValue1>> AsyncRegularExecution()
        => await _pipe.ExecuteAsync(_value);
}