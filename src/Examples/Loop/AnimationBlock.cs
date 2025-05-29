using MM.PipeBlocks.Blocks;

namespace Loop;
public class AnimationBlock : AsyncCodeBlock<MyContextType, MyValueType>
{
    protected override async ValueTask<MyContextType> ExecuteAsync(MyContextType context, MyValueType value)
    {
        Console.SetCursorPosition(0, 0);

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
            Console.Write(((context.Counter + i) % 4) switch
            {
                0 => "|  ",
                1 => "/  ",
                2 => "-  ",
                3 => "\\  ",
                _ => "?"
            });
        }

        await Task.Delay(100);
        return context;
    }
}