using MM.PipeBlocks;
using MM.PipeBlocks.Extensions;
using Parallel;

var builder = new BlockBuilder<MyValueType>();
var pipe = builder.CreatePipe("dice rolls")
                    .Then(b => b.Parallelize(
                        [
                            new RandomNumberGenerationBlock(),
                            new RandomNumberGenerationBlock(),
                            new RandomNumberGenerationBlock()
                        ],
                        new Clone<MyValueType>(v => v.Value),
                        new Join<MyValueType>((v, pv) =>
                        {
                            foreach (var ppv in pv)
                                Console.WriteLine($"{ppv.CorrelationId} : {String.Join(',', ppv.Context.Get<int[]>("Digits"))}");
                            v.Context.Set<int[]>("Digits", [.. pv.SelectMany(x => x.Context.Get<int[]>("Digits"))]);
                            return v;
                        })))
                    ;

var result = pipe.Execute(new MyValueType());

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
        Console.WriteLine(String.Join(',', result.Context.Get<int[]>("Digits")));
    });