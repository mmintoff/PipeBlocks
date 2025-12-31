using Loop;
using Microsoft.Extensions.Options;
using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;
using MM.PipeBlocks.Extensions;

var builder = new BlockBuilder<MyValueType>();
var framePipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "Frame Pipe" }))
                    .Then<AnimationBlock>()
                    .Then<IncrementBlock>()
                    ;

var animationPipe = builder.CreatePipe(Options.Create(new PipeBlockOptions { PipeName = "Animation Pipe" }))
                        .Then(b => b.Run(() => Console.CursorVisible = false))
                        .Then(b => b.Loop()
                                    .Do(framePipe, v => v.Context.Get<int>("Counter") <= 100))
                        .Then(b => b.Run(() => Console.CursorVisible = true))
                        .Then(b => b.Run(() => Console.ResetColor()))
                        .Then(b => b.Run(() => Console.WriteLine()))
                        ;

var result = animationPipe.Execute(new MyValueType(), ctx =>
{
    ctx.Set("Counter", 0);
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
        Console.Write("Success: ");
        Console.ResetColor();
    });