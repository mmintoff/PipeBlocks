using MM.PipeBlocks;

namespace Branch;
public class ChadTaxRateBlock : CodeBlock<BettingContext, Bet>
{
    protected override BettingContext Execute(BettingContext context, Bet value)
    {
        // 10% Tax Rate
        value.NetAmount = value.GrossAmount * 0.9M;
        return context;
    }
}