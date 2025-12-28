using MM.PipeBlocks;
using Reusable;

var builder = new BlockBuilder<ICustomValue>();
var pipe = builder.CreatePipe("Generic pipe")
            .Then<GenericCodeBlock>()
            ;

pipe.Execute(new FileDownloadValue
{
    FileName = "mudkips.jpg",
    SourceUrl = "http://example.com/"
}).Match(
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

pipe.Execute(new FileUploadValue
{
    FileName = "togekiss.jpg",
    DestinationUrl = "http://example.com/"
}).Match(
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

pipe.Execute(new FileCompressionValue
{
    FileName = "infernape.jpg",
    ArchiveName = "infernape.gz"
}).Match(
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