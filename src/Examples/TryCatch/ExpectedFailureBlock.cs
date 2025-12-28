using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace TryCatch;
public class ExpectedFailureBlock : CodeBlock<MyValueType>
{
    protected override Parameter<MyValueType> Execute(Parameter<MyValueType> parameter, MyValueType value)
    {
        parameter.Context.Set("CurrentStatus", "Expected Failure");
        parameter.SignalBreak(new DefaultFailureState<MyValueType>(value)
        {
            FailureReason = "Expected Failure"
        });
        return parameter;
    }
}