using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace Loop;
public class IncrementBlock : CodeBlock<MyValueType>
{
    protected override Parameter<MyValueType> Execute(Parameter<MyValueType> parameter, MyValueType value)
    {
        parameter.Context.Increment<int>("Counter");
        return parameter;
    }
}