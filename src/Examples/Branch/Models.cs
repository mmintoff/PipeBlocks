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

public class BettingContext(Bet bet) : IContext<Bet>
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Either<IFailureState<Bet>, Bet> Value { get; set; } = bet;
    public bool IsFinished { get; set; }
    public bool IsFlipped { get; set; }
}