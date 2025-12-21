using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;
using System;

namespace Branch;
public class NigerianTaxRateBlock : CodeBlock<Bet>
{
    protected override Parameter<Bet> Execute(Parameter<Bet> parameter, Bet value)
    {
        // 20% Tax Rate
        value.NetAmount = value.GrossAmount * 0.8M;
        return value;
    }
}