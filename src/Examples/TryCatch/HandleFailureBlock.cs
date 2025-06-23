using MM.PipeBlocks;

namespace TryCatch;
public class HandleFailureBlock : CodeBlock<MyContextType, MyValueType>
{
    protected override MyContextType Execute(MyContextType context, MyValueType value)
    {
        context.CurrentStatus = "Rolled back";
        return context;
    }
}