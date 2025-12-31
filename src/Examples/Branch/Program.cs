using Branch;
using Microsoft.Extensions.Options;
using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

var builder = new BlockBuilder<Bet>();
var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "tax pipe" }))
            .Then(b => b.Switch(v => v.Value.Country switch
            {
                Country.Nigeria => b.ResolveInstance<NigerianTaxRateBlock>(),
                Country.Tanzania => b.ResolveInstance<TanzaniaTaxRateBlock>(),
                Country.Chad => b.ResolveInstance<ChadTaxRateBlock>(),
                Country.Mali => b.ResolveInstance<MaliTaxRateBlock>(),
                _ => b.Noop()
            }))
            ;

foreach (var country in Enum.GetValues<Country>())
{
    var result = pipe.Execute(new Bet
    {
        Country = country,
        GrossAmount = 100M,
        NetAmount = 100M
    });

    result.Match(
        failure =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Failure: ");
            Console.ResetColor();
            Console.WriteLine(failure.FailureReason);
        },
        success =>
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Success: ");
            Console.ResetColor();
            Console.WriteLine($"{success.Country} :- {success.NetAmount}");
        });
}