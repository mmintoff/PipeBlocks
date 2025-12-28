//using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Diagnosers;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using MM.PipeBlocks.Abstractions;
//using MM.PipeBlocks.Extensions;

//namespace MM.PipeBlocks.Tester;
//[MemoryDiagnoser]
//[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses, HardwareCounter.InstructionRetired)]
//[DisassemblyDiagnoser(printSource: true)]
//public class PipelineBenchmark
//{
//    private PipeBlock<ICustomValue> _pipe;
//    private FuncBlock<ICustomValue> _func;
//    private AsyncFuncBlock<ICustomValue> _asyncFunc;

//    [GlobalSetup]
//    public void Setup()
//    {
//        var serviceCollection = new ServiceCollection();
//        serviceCollection.AddTransient<IBlockResolver<ICustomValue>, ServiceProviderBackedResolver<ICustomValue>>();
//        serviceCollection.AddTransient<BlockBuilder<ICustomValue>>();
//        serviceCollection.AddTransient<DummyBlock>();
//        serviceCollection.AddTransient<CustomCodeBlock>();
//        serviceCollection.AddLogging(configure =>
//        {
//            configure.ClearProviders();
//            configure.AddConsole();
//            configure.SetMinimumLevel(LogLevel.Warning);
//        });

//        var serviceProvider = serviceCollection.BuildServiceProvider();

//        var builder = serviceProvider.GetRequiredService<BlockBuilder<ICustomValue>>();

//        var adapterPipe = builder.CreatePipe("adapterPipe", builder, new MyAdapter())
//            .Then(b => b.Run(p => { p.Value.Start.AddMinutes(1); }))
//            ;

//        var startFromPipe = builder.CreatePipe("steppedPipe", c => c.Step)
//            .Then(builder.Run(c => { var a = 1 + 1; }))
//            .Then(builder.Run(c => { var a = 1 * 1; }))
//            .Then(builder.Run(c => { var a = 1 / 1; }))
//            .Then(builder.Run(c => { var a = 1 - 1; }))
//            ;

//        _pipe = builder
//            .CreatePipe("mainPipe")
//            .Then(startFromPipe)
//            .Then(b => b.Run(Do))
//            .Then(b => b.Run(DoAsync))
//            .Then(b => b.Run(DoAsync2))
//            .Then(b => b.Run(Fibonnaci))
//            .Then(b => b.Run(PrimeCheck))
//            ;

//        //_func = builder
//        //    .CreatePipe("compiled sync main pipe")
//        //    .Then(startFromPipe.CompileSync())
//        //    .Then(b => b.Run(Do))
//        //    .Then(b => b.Run(DoAsync))
//        //    .Then(b => b.Run(DoAsync2))
//        //    .Then(b => b.Run(Fibonnaci))
//        //    .Then(b => b.Run(PrimeCheck))
//        //    .CompileSync();

//        //_asyncFunc = builder
//        //    .CreatePipe("compiled async main pipe")
//        //    .Then(startFromPipe.CompileAsync())
//        //    .Then(b => b.Run(Do))
//        //    .Then(b => b.Run(DoAsync))
//        //    .Then(b => b.Run(DoAsync2))
//        //    .Then(b => b.Run(Fibonnaci))
//        //    .Then(b => b.Run(PrimeCheck))
//        //    .CompileAsync();
//    }

//    void Do()
//    {
//    }

//    async Task DoAsync()
//    {
//        await Task.Delay(0);
//    }

//    async Task<bool> DoAsync2()
//    {
//        await Task.Delay(0);
//        return false;
//    }

//    void Fibonnaci()
//    {
//        int Fib(int n) => n <= 1 ? n : Fib(n - 1) + Fib(n - 2);
//        var result = Fib(35);
//    }

//    void PrimeCheck()
//    {
//        bool IsPrime(int n)
//        {
//            if (n < 2) return false;
//            for (int i = 2; i * i <= n; i++)
//                if (n % i == 0)
//                    return false;
//            return true;
//        }

//        int count = 0;
//        for (int i = 2; i < 1_000_000; i++)
//            if (IsPrime(i))
//                count++;
//    }

//    [Benchmark]
//    public CustomContext SyncRegularExecution()
//    {
//        var ctx = new CustomContext(new CustomValue1
//        {
//            Count = 57,
//            Name = "Henry"
//        });

//        _pipe.Execute(ctx);
//        return ctx;
//    }

//    [Benchmark]
//    public async ValueTask<CustomContext> AsyncRegularExecution()
//    {
//        var ctx = new CustomContext(new CustomValue1
//        {
//            Count = 57,
//            Name = "Henry"
//        });

//        await _pipe.ExecuteAsync(ctx);
//        return ctx;
//    }

//    [Benchmark]
//    public CustomContext SyncCompiledExecution()
//    {
//        var ctx = new CustomContext(new CustomValue1
//        {
//            Count = 57,
//            Name = "Henry"
//        });

//        _func.Execute(ctx);
//        return ctx;
//    }

//    [Benchmark]
//    public async ValueTask<CustomContext> AsyncCompiledExecution()
//    {
//        var ctx = new CustomContext(new CustomValue1
//        {
//            Count = 57,
//            Name = "Henry"
//        });

//        await _asyncFunc.ExecuteAsync(ctx);
//        return ctx;
//    }
//}
