using MM.PipeBlocks;

namespace Branch;
public class TanzaniaTaxRateBlock : CodeBlock<BettingContext, Bet>
{
    protected override BettingContext Execute(BettingContext context, Bet value)
    {
        // 15% Tax Rate
        value.NetAmount = value.GrossAmount * 0.85M;
        return context;
    }
}