using MM.PipeBlocks.Blocks;

namespace Branch;
public class NigerianTaxRateBlock : CodeBlock<BettingContext, Bet>
{
    protected override BettingContext Execute(BettingContext context, Bet value)
    {
        // 20% Tax Rate
        value.NetAmount = value.GrossAmount * 0.8M;
        return context;
    }
}