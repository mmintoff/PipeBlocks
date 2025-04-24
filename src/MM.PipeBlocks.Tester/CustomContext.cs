using MM.PipeBlocks.Abstractions;

namespace MM.PipeBlocks.Tester;
public class CustomContext
    (
        Either<IFailureState<ICustomValue>, ICustomValue> value,
        Guid? correlationId = null
    ) : IContext<ICustomValue>
{
    public Guid CorrelationId { get; set; } = correlationId ?? Guid.NewGuid();
    public Either<IFailureState<ICustomValue>, ICustomValue> Value { get; set; } = value;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }

    public int Step { get; set; }
}

public class CustomContext2
    (
        Either<IFailureState<ICustomValue>, ICustomValue> value,
        Guid? correlationId = null
    ) : IContext<ICustomValue>
{
    public Guid CorrelationId { get; set; } = correlationId ?? Guid.NewGuid();
    public Either<IFailureState<ICustomValue>, ICustomValue> Value { get; set; } = value;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }

    public int Step { get; set; }
    public DateTime Start { get; set; }
}

public interface ICustomValue
{
    int Count { get; set; }
}

public class CustomValue1 : ICustomValue
{
    public int Count { get; set; }
    public string Name { get; set; }
}

public class CustomValue2 : ICustomValue
{
    public int Count { get; set; }
    public string Address { get; set; }
}