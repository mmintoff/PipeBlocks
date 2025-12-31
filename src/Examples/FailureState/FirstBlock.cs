using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace FailureState;

public class FirstBlock : CodeBlock<MyValueType>
{
    protected override Parameter<MyValueType> Execute(Parameter<MyValueType> parameter, MyValueType value)
    {
        Console.WriteLine($"Executing {nameof(FirstBlock)}");
        value.Counter++;
        return parameter;
    }
}