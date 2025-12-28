using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace TryCatch;
public class HandleFailureBlock : CodeBlock<MyValueType>
{
    protected override Parameter<MyValueType> Execute(Parameter<MyValueType> parameter, MyValueType value)
    {
        parameter.Context.Set("CurrentStatus", "Rolled back");
        return parameter;
    }
}