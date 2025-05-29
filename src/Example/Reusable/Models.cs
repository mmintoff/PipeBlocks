using MM.PipeBlocks.Abstractions;

namespace Reusable;

public interface ICustomValue { }
public interface ICustomContext : IContext<ICustomValue>
{
    string FileName { get; set; }
}

public class ConcreteValue_001 : ICustomValue { }
public class ConcreteValue_002 : ICustomValue { }

public class FileDownloadContext(ConcreteValue_001 value) : ICustomContext
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Either<IFailureState<ICustomValue>, ICustomValue> Value { get; set; } = value;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }
    
    public string FileName { get; set; }
    public string SourceUrl { get; set; }
}

public class FileUploadContext(ConcreteValue_002 value) : ICustomContext
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Either<IFailureState<ICustomValue>, ICustomValue> Value { get; set; } = value;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }
    
    public string FileName { get; set; }
    public string DestinationUrl { get; set; }
}

public class FileCompressionContext(ConcreteValue_001 value) : ICustomContext
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Either<IFailureState<ICustomValue>, ICustomValue> Value { get; set; } = value;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }

    public string FileName { get; set; }
    public string ArchiveName { get; set; }
}