using MM.PipeBlocks;
using MM.PipeBlocks.Extensions;
using StartFrom;

var builder = new BlockBuilder<MyContext, MyValue>();
var pipe = builder.CreatePipe("startfrom pipe", c => c.Step)
                .Then(b => b.Run(() => Console.WriteLine("Progress [▓▓░░░░░░░░] 20%")))
                .Then(b => b.Run(() => Console.WriteLine("Progress [▓▓▓▓░░░░░░] 40%")))
                .Then(b => b.Run(() => Console.WriteLine("Progress [▓▓▓▓▓▓░░░░] 60%")))
                .Then(b => b.Run(() => Console.WriteLine("Progress [▓▓▓▓▓▓▓▓░░] 80%")))
                .Then(b => b.Run(() => Console.WriteLine("Progress [▓▓▓▓▓▓▓▓▓▓] 100%")))
                ;

for (int i = 0; i < 5; i++)
{
    var result = pipe.Execute(new MyContext(new MyValue())
    {
        Step = i
    });

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
            Console.WriteLine("Success");
            Console.ResetColor();
        });
}