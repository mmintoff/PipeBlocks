using MM.PipeBlocks.Abstractions;

namespace FailureState;
public class MyValueType
{
    public int Counter { get; set; }
}

public class MyContextType(MyValueType value) : IContext<MyValueType>
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Either<IFailureState<MyValueType>, MyValueType> Value { get; set; } = value;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }
}