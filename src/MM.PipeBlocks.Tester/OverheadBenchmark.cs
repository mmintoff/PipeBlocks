using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.Options;
using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Tester;

public sealed record Request(int Id, string Name);
public sealed record Response(int Value, bool IsValid);

public enum Scenario
{
    Happy,
    Throw,
    BadResponse
}

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
    private IPipeBlock<Request> _pipe = null;

    [GlobalSetup]
    public void Setup()
    {
        _request = Scenario switch
        {
            Scenario.Happy => new Request(1, "ok"),
            Scenario.Throw => new Request(1, "throw"),
            Scenario.BadResponse => new Request(1, "bad-response"),
            _ => throw new InvalidOperationException()
        };

        BlockBuilder<Request> blockBuilder = new BlockBuilder<Request>();
        _pipe = blockBuilder.CreatePipe(Options.Create(new PipeBlockOptions
        {
            PipeName = "Performance Benchmark Pipe",
            HandleExceptions = HandleExceptions
        }))
        .Then(b => b.Run(async v =>
        {
            var v1 = await AsyncStep1(v.Value);
            v.Context.Set("v1", v1);
        }))
        .Then(b => b.Run(v =>
        {
            if (v.Context.Get<int>("v1") % 2 == 1)
                v.SignalBreak(new DefaultFailureState<Request>(v.Value)
                {
                    FailureReason = "Step1 failure"
                });
        }))
        .Then(b => b.Run(async v =>
        {
            var v2 = await AsyncStep2(v.Value);
            v.Context.Set("v2", v2);
        }))
        .Then(b => b.Run(v =>
        {
            if (String.IsNullOrEmpty(v.Context.Get<string>("v2")))
                v.SignalBreak(new DefaultFailureState<Request>(v.Value)
                {
                    FailureReason = "Step2 failure"
                });
        }));
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
    public async Task<Response> PipeBlock()
    {
        var result = await _pipe.ExecuteAsync(_request);
        return result.Match(
            f => new Response(result.Context.Get<int>("v1"), false),
            s => new Response(result.Context.Get<int>("v1"), true));
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