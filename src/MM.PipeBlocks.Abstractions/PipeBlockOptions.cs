namespace MM.PipeBlocks.Abstractions;

public class PipeBlockOptions
{
    public required string PipeName { get; set; }
    public bool HandleExceptions { get; set; } = false;
    public Action<Context>? ConfigureContextConstants { get; set; } = null;
}