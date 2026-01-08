using Map;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MM.PipeBlocks;
using MM.PipeBlocks.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
serviceCollection
    .AddPipeBlocks()
    .AddTransientBlock<ValidateOrderBlock>()
    .AddTransientBlock<PlaceOrderBlock>()
    .AddTransientBlock<ReceiptBlock>()
    .AddTransientBlock<ShipmentBlock>()
    ;
serviceCollection.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Trace);
});
var serviceProvider = serviceCollection.BuildServiceProvider();

var builder = serviceProvider.GetRequiredService<BlockBuilder<Order>>();
var pipe = builder.CreatePipe(Options.Create(new MM.PipeBlocks.Abstractions.PipeBlockOptions { PipeName = "order pipe" }))
                .Then<ValidateOrderBlock>()
                .Map<Payment>().Via<PlaceOrderBlock>()
                .Map<Receipt>().Via<ReceiptBlock>()
                .Map<Shipment>().Via<ShipmentBlock>()
                ;

var result = pipe.Execute(new Order
{
    OrderId = 12345
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
        Console.WriteLine($"Order {success.OrderId} Completed");
    });