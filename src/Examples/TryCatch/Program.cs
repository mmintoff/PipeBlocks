using MM.PipeBlocks;
using TryCatch;

var builder = new BlockBuilder<MyContextType, MyValueType>();
var pipe = builder.CreatePipe("try catch pipe")
                .Then(b => b.TryCatch<ExpectedFailureBlock, HandleFailureBlock>())
                .Then(b => b.Run(() => Console.WriteLine("Should not be executing")))
                ;

var result = pipe.Execute(new MyContextType(new MyValueType()));

result.Value.Match(
    failure =>
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Failure: ");
        Console.ResetColor();
        Console.WriteLine($"{failure.FailureReason} with CurrentStatus: {result.CurrentStatus}");
    },
    success =>
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Success: ");
        Console.ResetColor();
        Console.WriteLine(result.CurrentStatus);
    });