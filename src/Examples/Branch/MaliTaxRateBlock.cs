using MM.PipeBlocks.Blocks;
namespace Branch;
public class MaliTaxRateBlock : CodeBlock<BettingContext, Bet>
{
    protected override BettingContext Execute(BettingContext context, Bet value)
    {
        // 5% Tax Rate
        value.NetAmount = value.GrossAmount * 0.95M;
        return context;
    }
}