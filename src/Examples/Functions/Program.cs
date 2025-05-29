using Functions;
using MM.PipeBlocks;

var builder = new BlockBuilder<MyContext, MyValue>();
var pipe = builder.CreatePipe("func pipe")
            .Then(b => b.Run((c, v) => v.Fibonacci = Fibonacci(c.N)))
            ;

var result = pipe.Execute(new MyContext(new MyValue()) { N = 35 });
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
        Console.WriteLine(success.Fibonacci);
    });

static int Fibonacci(int n)
{
    return n <= 1 ? n : Fibonacci(n - 1) + Fibonacci(n - 2);
}