using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace Loop;
public class IncrementBlock : CodeBlock<MyValueType>
{
    protected override Parameter<MyValueType> Execute(Parameter<MyValueType> parameter, MyValueType value)
    {
        var context = Context.Get<int>("Counter");
        counter++;
        return parameter;
    }
}