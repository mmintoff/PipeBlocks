using MM.PipeBlocks.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MM.PipeBlocks.Tester;

public sealed record Request(int Id, string Name);
public sealed record Response(int Value, bool IsValid);

public enum Scenario
{
    Happy,
    Throw,
    BadResponse
}

public interface IOB_Service
{
    ValueTask<int> AsyncStep1(Request request);
    ValueTask<string> AsyncStep2(Request request);
}

public class OB_Service : IOB_Service
{
    public ValueTask<int> AsyncStep1(Request request)
    {
        return ValueTask.FromResult(request.Id * 2);
    }

    public ValueTask<string> AsyncStep2(Request request)
    {
        if (request.Name == "throw")
            throw new InvalidOperationException();

        return ValueTask.FromResult(
            request.Name == "bad-response" ? "" : "OK");
    }
}

public class OB_AsyncStep1(IOB_Service service) : AsyncCodeBlock<Request>
{
    protected override async ValueTask<Parameter<Request>> ExecuteAsync(Parameter<Request> parameter, Request extractedValue)
    {
        var v1 = await service.AsyncStep1(extractedValue);
        parameter.Context.Set("v1", v1);
        return parameter;
    }
}

public class OB_Validation1 : CodeBlock<Request>
{
    protected override Parameter<Request> Execute(Parameter<Request> parameter, Request extractedValue)
    {
        if (parameter.Context.Get<int>("v1") % 2 == 1)
            parameter.SignalBreak("Step1 failure");
        return parameter;
    }
}

public class OB_AsyncStep2(IOB_Service service) : AsyncCodeBlock<Request>
{
    protected override async ValueTask<Parameter<Request>> ExecuteAsync(Parameter<Request> parameter, Request extractedValue)
    {
        var v2 = await service.AsyncStep2(extractedValue);
        parameter.Context.Set("v2", v2);
        return parameter;
    }
}

public class OB_Validation2 : CodeBlock<Request>
{
    protected override Parameter<Request> Execute(Parameter<Request> parameter, Request extractedValue)
    {
        if (parameter.Context.TryGet<string>("v2", out var v2) && !String.IsNullOrEmpty(v2))
            parameter.SignalBreak("Step2 failure");
        return parameter;
    }
}