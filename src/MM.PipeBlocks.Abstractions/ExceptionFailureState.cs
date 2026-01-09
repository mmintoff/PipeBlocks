using MM.PipeBlocks.Abstractions;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace MM.PipeBlocks;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public class ExceptionFailureState<V>(V value, Exception ex) : IFailureState<V>
{
    public V Value { get; set; } = value;
    public Guid CorrelationId{ get; set; }
    public string? FailureReason { get; set; } = ex.Message;
    public Exception Exception{get;set; } = ex;
    object? IFailureState.Value { get => Value; set => Value = (V)(value ?? throw new ArgumentNullException(nameof(value))); }
}
