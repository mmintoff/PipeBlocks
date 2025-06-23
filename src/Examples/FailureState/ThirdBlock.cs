using MM.PipeBlocks;

namespace FailureState;
public class ThirdBlock : CodeBlock<MyContextType, MyValueType>
{
    protected override MyContextType Execute(MyContextType context, MyValueType value)
    {
        Console.WriteLine($"Executing {nameof(ThirdBlock)}");
        value.Counter++;
        return context;
    }
}