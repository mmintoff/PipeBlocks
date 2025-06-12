using MM.PipeBlocks.Blocks;
using MM.PipeBlocks;

namespace TryCatch;
public class ExpectedFailureBlock : CodeBlock<MyContextType, MyValueType>
{
    protected override MyContextType Execute(MyContextType context, MyValueType value)
    {
        context.CurrentStatus = "Expected Failure";
        context.SignalBreak(new DefaultFailureState<MyValueType>(value)
        {
            FailureReason = "Expected Failure"
        });
        return context;
    }
}