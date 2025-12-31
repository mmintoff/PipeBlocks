using FailureState;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MM.PipeBlocks.Extensions.DependencyInjection;
using MM.PipeBlocks;

var serviceCollection = new ServiceCollection();
serviceCollection
    .AddPipeBlocks()
    .AddTransientBlock<FirstBlock>()
    .AddTransientBlock<SecondBlock>()
    .AddTransientBlock<ThirdBlock>()
    ;
serviceCollection.AddLogging(configure =>
{
    configure.ClearProviders();
    configure.AddConsole();
    configure.SetMinimumLevel(LogLevel.Trace);
});
var serviceProvider = serviceCollection.BuildServiceProvider();

var builder = serviceProvider.GetRequiredService<BlockBuilder<MyValueType>>();
var pipe = builder.CreatePipe("failure state example pipe")
                .Then<FirstBlock>()
                .Then<SecondBlock>()
                .Then<ThirdBlock>()
                ;

var result = pipe.Execute(new MyValueType());
result.Match(
    failure =>
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Failure: ");
        Console.ResetColor();
        Console.WriteLine($"{failure.FailureReason} with counter of {failure.Value.Counter}");
    },
    success =>
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Success: ");
        Console.ResetColor();
        Console.WriteLine(success.Counter);
    });