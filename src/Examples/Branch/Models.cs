using MM.PipeBlocks.Abstractions;

namespace Branch;
public enum Country
{
    Nigeria,
    Tanzania,
    Chad,
    Mali,
    Botswana,
    Lesotho
}

public class Bet
{
    public Country Country { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal NetAmount { get; set; }
}