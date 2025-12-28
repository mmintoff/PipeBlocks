using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace Loop;
public class AnimationBlock : AsyncCodeBlock<MyValueType>
{
    protected override async ValueTask<Parameter<MyValueType>> ExecuteAsync(Parameter<MyValueType> parameter, MyValueType value)
    {
        Console.SetCursorPosition(0, 0);
        var counter = parameter.Context.Get<int>("Counter");

        for (int i = 0; i < 8; i++)
        {
            Console.ForegroundColor = (i % 4) switch
            {
                0 => ConsoleColor.Green,
                1 => ConsoleColor.Magenta,
                2 => ConsoleColor.Yellow,
                3 => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
            Console.Write(((counter + i) % 4) switch
            {
                0 => "|  ",
                1 => "/  ",
                2 => "-  ",
                3 => "\\  ",
                _ => "?"
            });
        }

        await Task.Delay(100);
        return parameter;
    }
}