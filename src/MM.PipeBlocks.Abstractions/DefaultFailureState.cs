namespace MM.PipeBlocks.Abstractions;
public sealed class DefaultFailureState<V>(V value) : IFailureState<V>
{
    public V Value { get; set; } = value;
    public Guid CorrelationId { get; set; }
    public string? FailureReason { get; set; }
}