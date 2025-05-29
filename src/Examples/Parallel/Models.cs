using MM.PipeBlocks.Abstractions;

namespace Parallel;
public class MyValueType { }
public class MyContextType(Either<IFailureState<MyValueType>, MyValueType> value) : IContext<MyValueType>
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Either<IFailureState<MyValueType>, MyValueType> Value { get; set; } = value;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }
    public int[] Digits { get; set; }
}