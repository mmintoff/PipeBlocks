using MM.PipeBlocks;

namespace FailureState;

public class FirstBlock : CodeBlock<MyContextType, MyValueType>
{
    protected override MyContextType Execute(MyContextType context, MyValueType value)
    {
        Console.WriteLine($"Executing {nameof(FirstBlock)}");
        value.Counter++;
        return context;
    }
}