using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Extensions.DependencyInjection;

namespace MM.PipeBlocks.Tester;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[WarmupCount(5)]
[IterationCount(15)]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class OverheadBenchmark
{
    [Params(Scenario.Happy, Scenario.Throw, Scenario.BadResponse)]
    public Scenario Scenario;

    [Params(true, false)]
    public bool HandleExceptions;

    private Request _request = null;
    private PipeBlock<Request> _pipe;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddTransient<IOB_Service, OB_Service>();
        services.AddPipeBlocks()
            .AddTransientBlock<OB_AsyncStep1>()
            .AddTransientBlock<OB_AsyncStep2>()
            .AddTransientBlock<OB_Validation1>()
            .AddTransientBlock<OB_Validation2>()
            ;

        var provider = services.BuildServiceProvider();

        _request = Scenario switch
        {
            Scenario.Happy => new Request(1, "ok"),
            Scenario.Throw => new Request(1, "throw"),
            Scenario.BadResponse => new Request(1, "bad-response"),
            _ => throw new InvalidOperationException()
        };

        var blockBuilder = provider.GetRequiredService<BlockBuilder<Request>>();
        _pipe = blockBuilder
            .CreatePipe(Options.Create(new PipeBlockOptions
            {
                PipeName = "Performance Benchmark Pipe",
                HandleExceptions = HandleExceptions
            }))
            .Then<OB_AsyncStep1>()
            .Then<OB_Validation1>()
            .Then<OB_AsyncStep2>()
            .Then<OB_Validation2>()
            ;
    }

    [Benchmark(Baseline = true)]
    public async Task<Response> PlainCSharp()
    {
        try
        {
            var v1 = await AsyncStep1(_request);

            if (v1 % 2 == 1)
                return new Response(v1, false);

            var v2 = await AsyncStep2(_request);

            if (String.IsNullOrEmpty(v2))
                return new Response(v1, false);

            return new Response(v1, true);
        }
        catch
        {
            return new Response(-1, false);
        }
    }

    [Benchmark]
    public async Task<Response> PipeBlocks()
    {
        if (!HandleExceptions)
        {
            try
            {
                var result = await _pipe.ExecuteAsync(_request);
                return result.Match(
                    f => new Response(result.Context.Get<int>("v1"), false),
                    s => new Response(result.Context.Get<int>("v1"), true));
            }
            catch
            {
                return new Response(-1, false);
            }
        }
        else
        {
            var result = await _pipe.ExecuteAsync(_request);
            return result.Match(
                f => new Response(result.Context.Get<int>("v1"), false),
                s => new Response(result.Context.Get<int>("v1"), true));
        }
    }

    private static ValueTask<int> AsyncStep1(Request request)
    {
        return ValueTask.FromResult(request.Id * 2);
    }

    private static ValueTask<string> AsyncStep2(Request request)
    {
        if (request.Name == "throw")
            throw new InvalidOperationException();

        return ValueTask.FromResult(
            request.Name == "bad-response" ? "" : "OK");
    }
}