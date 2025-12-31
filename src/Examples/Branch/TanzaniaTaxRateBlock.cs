using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace Branch;
public class TanzaniaTaxRateBlock : CodeBlock<Bet>
{
    protected override Parameter<Bet> Execute(Parameter<Bet> parameter, Bet value)
    {
        // 15% Tax Rate
        value.NetAmount = value.GrossAmount * 0.85M;
        return value;
    }
}