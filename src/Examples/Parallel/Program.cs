using Microsoft.Extensions.Options;
using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Extensions;
using Parallel;

var builder = new BlockBuilder<MyValueType>();
var pipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "dice rolls" }))
                    .Then(b => b.Parallelize(
                        [
                            b.ResolveInstance<RandomNumberGenerationBlock>(),
                            new RandomNumberGenerationBlock(),
                            new RandomNumberGenerationBlock(),
                            new RandomNumberGenerationBlock()
                        ],
                        new Join<MyValueType>((v, pv) =>
                        {
                            foreach (var ppv in pv)
                                Console.WriteLine($"{ppv.CorrelationId} : {String.Join(',', ppv.Context.Get<int[]>("Digits"))}");
                            v.Context.Set<int[]>("Digits", [.. pv.SelectMany(x => x.Context.Get<int[]>("Digits"))]);
                            return v;
                        }),
                        null
                        //new Clone<MyValueType>(v =>
                        //{
                        //    return v.Match(
                        //        failure => v.Value,
                        //        success => new MyValueType());
                        //})
                        ))
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