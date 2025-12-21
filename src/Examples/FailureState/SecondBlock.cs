using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace FailureState;
public class SecondBlock : CodeBlock<MyValueType>
{
    protected override Parameter<MyValueType> Execute(Parameter<MyValueType> parameter, MyValueType value)
    {
        Console.WriteLine($"Executing {nameof(SecondBlock)}");
        value.Counter++;

        parameter.SignalBreak(new DefaultFailureState<MyValueType>(value)
        {
            CorrelationId = Context.CorrelationId,
            FailureReason = "Arbitrarily failing the pipe"
        });

        return parameter;
    }
}