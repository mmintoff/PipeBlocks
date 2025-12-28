using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;
using TryCatch;

var builder = new BlockBuilder<MyValueType>();
var pipe = builder.CreatePipe("try catch pipe")
                .Then(b => b.TryCatch<ExpectedFailureBlock, HandleFailureBlock>())
                .Then(b => b.Run(() => Console.WriteLine("Should not be executing")))
                ;

var result = pipe.Execute(new MyValueType());

result.Match(
    failure =>
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Failure: ");
        Console.ResetColor();
        Console.WriteLine($"{failure.FailureReason} with CurrentStatus: {(result.Context.TryGet<string>("CurrentStatus", out var value) ? value : null)}");
    },
    success =>
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Success: ");
        Console.ResetColor();
        Console.WriteLine(result.Context.TryGet<string>("CurrentStatus", out var value) ? value : null);
    });