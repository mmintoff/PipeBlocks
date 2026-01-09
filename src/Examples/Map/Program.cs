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
    configure.SetMinimumLevel(LogLevel.Information);
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
    OrderId = 12345,
    Amount = 99.99M,
    CustomerEmail = "customer@example.com"
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
        Console.WriteLine("Success");
        Console.ResetColor();
        Console.WriteLine($"   Order    : {success.OrderId}");
        Console.WriteLine($"   Tracking : {success.TrackingNumber}");
        Console.WriteLine($"   Delivery : {success.EstimatedDelivery}");
    });