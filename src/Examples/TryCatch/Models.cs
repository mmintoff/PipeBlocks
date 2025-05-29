using MM.PipeBlocks.Abstractions;

namespace TryCatch;
public class MyValueType { }
public class MyContextType(MyValueType value) : IContext<MyValueType>
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Either<IFailureState<MyValueType>, MyValueType> Value { get; set; } = value;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }

    public string CurrentStatus { get; set; }
}