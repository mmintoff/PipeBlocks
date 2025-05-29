using Branch;
using MM.PipeBlocks;

var builder = new BlockBuilder<BettingContext, Bet>();
var pipe = builder.CreatePipe("tax pipe")
            .Then(b => b.Switch((c, v) => v.Country switch
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
    var result = pipe.Execute(new BettingContext(new Bet
    {
        Country = country,
        GrossAmount = 100M,
        NetAmount = 100M
    }));

    result.Value.Match(
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