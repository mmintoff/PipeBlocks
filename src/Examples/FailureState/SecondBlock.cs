using MM.PipeBlocks;

namespace FailureState;
public class SecondBlock : CodeBlock<MyContextType, MyValueType>
{
    protected override MyContextType Execute(MyContextType context, MyValueType value)
    {
        Console.WriteLine($"Executing {nameof(SecondBlock)}");
        value.Counter++;

        context.SignalBreak(new DefaultFailureState<MyValueType>(value)
        {
            CorrelationId = context.CorrelationId,
            FailureReason = "Arbitrarily failing the pipe"
        });
        return context;
    }
}