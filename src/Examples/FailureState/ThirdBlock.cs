using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace FailureState;
public class ThirdBlock : CodeBlock<MyValueType>
{
    protected override Parameter<MyValueType> Execute(Parameter<MyValueType> parameter, MyValueType value)
    {
        Console.WriteLine($"Executing {nameof(ThirdBlock)}");
        value.Counter++;
        return parameter;
    }
}