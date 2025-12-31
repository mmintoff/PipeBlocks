using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace Branch;

public class ChadTaxRateBlock : CodeBlock<Bet>
{
    protected override Parameter<Bet> Execute(Parameter<Bet> parameter, Bet value)
    {
        // 10% Tax Rate
        value.NetAmount = value.GrossAmount * 0.9M;
        return value;
    }
}