using MM.PipeBlocks;

namespace Parallel;
public class RandomNumberGenerationBlock : CodeBlock<MyContextType, MyValueType>
{
    protected override MyContextType Execute(MyContextType context, MyValueType value)
    {
        int numOfDigits = Random.Shared.Next(1, 11);
        Console.WriteLine($"Generating {numOfDigits} random digits");

        context.Digits = new int[numOfDigits];
        for (int i = 0; i < numOfDigits; i++)
            context.Digits[i] = Random.Shared.Next(0, 10);

        return context;
    }
}