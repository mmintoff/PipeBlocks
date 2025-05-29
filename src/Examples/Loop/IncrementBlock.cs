using MM.PipeBlocks.Blocks;

namespace Loop;
public class IncrementBlock : CodeBlock<MyContextType, MyValueType>
{
    protected override MyContextType Execute(MyContextType context, MyValueType value)
    {
        context.Counter++;
        return context;
    }
}