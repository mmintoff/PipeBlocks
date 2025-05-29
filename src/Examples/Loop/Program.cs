using Loop;
using MM.PipeBlocks;
using MM.PipeBlocks.Extensions;

var builder = new BlockBuilder<MyContextType, MyValueType>();
var framePipe = builder.CreatePipe("Frame Pipe")
                    .Then<AnimationBlock>()
                    .Then<IncrementBlock>()
                    ;

var animationPipe = builder.CreatePipe("Animation Pipe")
                        .Then(b => b.Run(() => Console.CursorVisible = false))
                        .Then(b => b.Loop()
                                    .Do(framePipe, c => c.Counter <= 100))
                        .Then(b => b.Run(() => Console.CursorVisible = true))
                        .Then(b => b.Run(() => Console.ResetColor()))
                        .Then(b => b.Run(() => Console.WriteLine()))
                        ;

var result = animationPipe.Execute(new MyContextType(new MyValueType()));
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
        Console.WriteLine(result.Counter);
    });