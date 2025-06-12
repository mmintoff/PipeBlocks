using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Blocks;

namespace MM.PipeBlocks.Test;
public class MyValue
{
    public Guid Identifier { get; set; }
    public int Counter { get; set; }
}

public class MyContext(MyValue value) : IContext<MyValue>
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }
    public Either<IFailureState<MyValue>, MyValue> Value { get; set; } = value;

    public int Step { get; set; }
    public int Counter { get; set; }
    public bool ExecutedCatch { get; set; }
    public bool ExecutedFinally { get; set; }
    public Guid SniffedId { get; set; }

    public bool Condition { get; set; }
    public string ResultText { get; set; }
}

public class IncrementValue_CodeBlock(int i) : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
    {
        value.Counter += i;
        return context;
    }
}

/**/

public class ReturnContext_CodeBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
        => context;
}

public class ReturnFailContext_CodeBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
    {
        context.Value.Match(
            _ => { },
            x => context.SignalBreak(new DefaultFailureState<MyValue>(x)
            {
                FailureReason = "Intentional",
                CorrelationId = context.CorrelationId
            }));
        return context;
    }
}

public class Exception_CodeBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
        => throw new Exception("Intentional");
}

public class IncrementCounter_CodeBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
    {
        value.Counter++;
        return context;
    }
}

public class IncrementCounter_AsyncCodeBlock : AsyncCodeBlock<MyContext, MyValue>
{
    protected override ValueTask<MyContext> ExecuteAsync(MyContext context, MyValue value)
    {
        value.Counter++;
        return ValueTask.FromResult(context);
    }
}

public class IncrementContextCounter_CodeBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
    {
        context.Counter++;
        return context;
    }
}

public class IncrementContextCounter_AsyncCodeBlock : AsyncCodeBlock<MyContext, MyValue>
{
    protected override ValueTask<MyContext> ExecuteAsync(MyContext context, MyValue value)
    {
        context.Counter++;
        return ValueTask.FromResult(context);
    }
}

public class IncrementContextCounterAndFail_CodeBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
    {
        context.Counter++;
        throw new InvalidOperationException();
    }
}

public class IncrementContextCounterAndFail_AsyncCodeBlock : AsyncCodeBlock<MyContext, MyValue>
{
    protected override ValueTask<MyContext> ExecuteAsync(MyContext context, MyValue value)
    {
        context.Counter++;
        throw new InvalidOperationException();
    }
}

public class SniffCatchBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
    {
        context.ExecutedCatch = true;
        context.SniffedId = value.Identifier;
        return context;
    }
}

public class SniffFinallyBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
    {
        context.ExecutedFinally = true;
        context.SniffedId = value.Identifier;
        return context;
    }
}

public class SniffCatchAsyncBlock : AsyncCodeBlock<MyContext, MyValue>
{
    protected override ValueTask<MyContext> ExecuteAsync(MyContext context, MyValue value)
    {
        context.ExecutedCatch = true;
        context.SniffedId = value.Identifier;
        return ValueTask.FromResult(context);
    }
}

public class SniffFinallyAsyncBlock : AsyncCodeBlock<MyContext, MyValue>
{
    protected override ValueTask<MyContext> ExecuteAsync(MyContext context, MyValue value)
    {
        context.ExecutedFinally = true;
        context.SniffedId = value.Identifier;
        return ValueTask.FromResult(context);
    }
}

/**/

public class ReturnContext_AsyncCodeBlock : AsyncCodeBlock<MyContext, MyValue>
{
    protected override ValueTask<MyContext> ExecuteAsync(MyContext context, MyValue value)
        => ValueTask.FromResult(context);
}

public class ReturnFailContext_AsyncCodeBlock : AsyncCodeBlock<MyContext, MyValue>
{
    protected override ValueTask<MyContext> ExecuteAsync(MyContext context, MyValue value)
    {
        context.Value.Match(
            _ => { },
            x => context.SignalBreak(new DefaultFailureState<MyValue>(x)
            {
                FailureReason = "Intentional",
                CorrelationId = context.CorrelationId
            }));
        return ValueTask.FromResult(context);
    }
}

public class Exception_AsyncCodeBlock : AsyncCodeBlock<MyContext, MyValue>
{
    protected override ValueTask<MyContext> ExecuteAsync(MyContext context, MyValue value)
        => throw new Exception("Intentional");
}

/**/

public class MyBlockResolver<C, V> : IBlockResolver<C, V>
    where C : IContext<V>

{
    public X ResolveInstance<X>()
        where X : IBlock<C, V>
        => Activator.CreateInstance<X>();
}

/**/

public class DoThisBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
    {
        context.ResultText = "DoThis";
        return context;
    }
}

public class ElseThisBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
    {
        if (context.Counter > 1)
        {
            context.SignalBreak(new DefaultFailureState<MyValue>(value)
            {
                CorrelationId = context.CorrelationId,
                FailureReason = "Intentional"
            });
        }
        else
        {
            context.ResultText = "ElseThis";
        }
        return context;
    }
}

public class FailBlock : CodeBlock<MyContext, MyValue>
{
    protected override MyContext Execute(MyContext context, MyValue value)
    {
        context.ResultText = "Fail";
        return context;
    }
}