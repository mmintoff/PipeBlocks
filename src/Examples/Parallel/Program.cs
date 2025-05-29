using MM.PipeBlocks;
using MM.PipeBlocks.Extensions;
using Parallel;

var builder = new BlockBuilder<MyContextType, MyValueType>();
var pipe = builder.CreatePipe("dice rolls")
                    .Then(b => b.Parallelize(
                        [
                            new RandomNumberGenerationBlock(),
                            new RandomNumberGenerationBlock(),
                            new RandomNumberGenerationBlock()
                        ],
                        new Clone<MyContextType, MyValueType>(c => new MyContextType(c.Value)),
                        new Join<MyContextType, MyValueType>((c, v) =>
                        {
                            c.Digits = v.SelectMany(x => x.Digits).ToArray();
                            return c;
                        })))
                    ;

var result = pipe.Execute(new MyContextType(new MyValueType { }));

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
        Console.WriteLine(String.Join(',', result.Digits));
    });