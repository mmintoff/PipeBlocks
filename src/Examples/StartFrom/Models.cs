using MM.PipeBlocks.Abstractions;

namespace StartFrom;
public class MyValue { }

public class MyContext(MyValue value) : IContext<MyValue>
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Either<IFailureState<MyValue>, MyValue> Value { get; set; } = value;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }
    public int Step { get; set; }
}