using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Test;
public class MyValue
{
    public Guid Identifier { get; set; }
    public int Counter { get; set; }
}

public class IncrementValue_CodeBlock(int i) : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
    {
        value.Counter += i;
        return value;
    }
}

/**/

public class ReturnValue_CodeBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
        => parameter;
}

public class ReturnFailContext_CodeBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Match(
            _ => { },
            x => parameter.SignalBreak(new DefaultFailureState<MyValue>(x)
            {
                FailureReason = "Intentional"
            }));
        return parameter;
    }
}

public class Exception_CodeBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
        => throw new Exception("Intentional");
}

public class IncrementCounter_CodeBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
    {
        value.Counter++;
        return value;
    }
}

public class IncrementCounter_AsyncCodeBlock : AsyncCodeBlock<MyValue>
{
    protected override ValueTask<Parameter<MyValue>> ExecuteAsync(Parameter<MyValue> parameter, MyValue value)
    {
        value.Counter++;
        return ValueTask.FromResult(parameter);
    }
}

public class IncrementContextCounter_CodeBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Context.Increment<int>("Counter");
        return parameter;
    }
}

public class IncrementContextCounter_AsyncCodeBlock : AsyncCodeBlock<MyValue>
{
    protected override ValueTask<Parameter<MyValue>> ExecuteAsync(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Context.Increment<int>("Counter");
        return ValueTask.FromResult(parameter);
    }
}

public class IncrementContextCounterAndFail_CodeBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Context.Increment<int>("Counter");
        throw new InvalidOperationException();
    }
}

public class IncrementContextCounterAndFail_AsyncCodeBlock : AsyncCodeBlock<MyValue>
{
    protected override ValueTask<Parameter<MyValue>> ExecuteAsync(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Context.Increment<int>("Counter");
        throw new InvalidOperationException();
    }
}

public class SniffCatchBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Context.Set("ExecutedCatch", true);
        parameter.Context.Set("SniffedId", value.Identifier);
        return parameter;
    }
}

public class SniffFinallyBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Context.Set("ExecutedFinally", true);
        parameter.Context.Set("SniffedId", value.Identifier);
        return parameter;
    }
}

public class SniffCatchAsyncBlock : AsyncCodeBlock<MyValue>
{
    protected override ValueTask<Parameter<MyValue>> ExecuteAsync(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Context.Set("ExecutedCatch", true);
        parameter.Context.Set("SniffedId", value.Identifier);
        return ValueTask.FromResult(parameter);
    }
}

public class SniffFinallyAsyncBlock : AsyncCodeBlock<MyValue>
{
    protected override ValueTask<Parameter<MyValue>> ExecuteAsync(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Context.Set("ExecutedFinally", true);
        parameter.Context.Set("SniffedId", value.Identifier);
        return ValueTask.FromResult(parameter);
    }
}

/**/

public class ReturnValue_AsyncCodeBlock : AsyncCodeBlock<MyValue>
{
    protected override ValueTask<Parameter<MyValue>> ExecuteAsync(Parameter<MyValue> parameter, MyValue value)
        => ValueTask.FromResult(parameter);
}

public class ReturnFail_AsyncCodeBlock : AsyncCodeBlock<MyValue>
{
    protected override ValueTask<Parameter<MyValue>> ExecuteAsync(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Match(
            _ => { },
            x => parameter.SignalBreak(new DefaultFailureState<MyValue>(x)
            {
                FailureReason = "Intentional"
            }));
        return ValueTask.FromResult(parameter);
    }
}

public class Exception_AsyncCodeBlock : AsyncCodeBlock<MyValue>
{
    protected override ValueTask<Parameter<MyValue>> ExecuteAsync(Parameter<MyValue> parameter, MyValue value)
        => throw new Exception("Intentional");
}

/**/

public class MyBlockResolver<V> : IBlockResolver<V>
{
    public X ResolveInstance<X>()
        where X : IBlock<V>
        => Activator.CreateInstance<X>();

    public IBlockBuilder<Y> CreateBlockBuilder<Y>()
        => new BlockBuilder<Y>();
}

/**/

public class DoThisBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Context.Set("ResultText", "DoThis");
        return parameter;
    }
}

public class ElseThisBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
    {
        if (parameter.Context.Get<int>("Counter") > 1)
        {
            parameter.SignalBreak(new DefaultFailureState<MyValue>(value)
            {
                FailureReason = "Intentional"
            });
        }
        else
        {
            parameter.Context.Set("ResultText", "ElseThis");
        }
        return parameter;
    }
}

public class FailBlock : CodeBlock<MyValue>
{
    protected override Parameter<MyValue> Execute(Parameter<MyValue> parameter, MyValue value)
    {
        parameter.Context.Set("ResultText", "Fail");
        return parameter;
    }
}