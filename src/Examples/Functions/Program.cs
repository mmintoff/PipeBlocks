using Functions;
using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

var builder = new BlockBuilder<MyValue>();
var pipe = builder.CreatePipe("func pipe")
            .Then(b => b.Run(v => v.Value.Fibonacci = Fibonacci(Context.Get<int>("N"))))
            ;

var result = pipe.Execute(new MyValue(), c => c
    .With("N", 35));
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
        Console.WriteLine(success.Fibonacci);
    });

static int Fibonacci(int n)
{
    return n <= 1 ? n : Fibonacci(n - 1) + Fibonacci(n - 2);
}