using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace Branch;
public class MaliTaxRateBlock : CodeBlock<Bet>
{
    protected override Parameter<Bet> Execute(Parameter<Bet> parameter, Bet value)
    {
        // 5% Tax Rate
        value.NetAmount = value.GrossAmount * 0.95M;
        return value;
    }
}