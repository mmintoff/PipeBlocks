using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace Parallel;
public class RandomNumberGenerationBlock : CodeBlock<MyValueType>
{
    protected override Parameter<MyValueType> Execute(Parameter<MyValueType> parameter, MyValueType extractedValue)
    {
        int numOfDigits = Random.Shared.Next(1, 11);
        Console.WriteLine($"Generating {numOfDigits} random digits");

        var digits = new int[numOfDigits];
        for (int i = 0; i < numOfDigits; i++)
            digits[i] = Random.Shared.Next(0, 10);

        parameter.Context.Set("Digits", digits);

        return parameter;
    }
}