using MM.PipeBlocks;
using Reusable;

var builder = new BlockBuilder<ICustomContext, ICustomValue>();
var pipe = builder.CreatePipe("Generic pipe")
            .Then<GenericCodeBlock>()
            ;

pipe.Execute(new FileDownloadContext(new ConcreteValue_001())
{
    FileName = "mudkips.jpg",
    SourceUrl = "http://example.com/"
}).Value.Match(
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

pipe.Execute(new FileUploadContext(new ConcreteValue_002())
{
    FileName = "togekiss.jpg",
    DestinationUrl = "http://example.com/"
}).Value.Match(
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

pipe.Execute(new FileCompressionContext(new ConcreteValue_001())
{
    FileName = "infernape.jpg",
    ArchiveName = "infernape.gz"
}).Value.Match(
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