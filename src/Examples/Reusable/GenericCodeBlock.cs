using MM.PipeBlocks;
using MM.PipeBlocks.Abstractions;

namespace Reusable;
public class GenericCodeBlock : CodeBlock<ICustomValue>
{
    protected override Parameter<ICustomValue> Execute(Parameter<ICustomValue> parameter, ICustomValue value)
    {
        Console.WriteLine($"Processing file operation for '{value.FileName}'");
        switch (value)
        {
            case FileDownloadValue download:
                Console.WriteLine($"Handling download: {download.FileName} from {download.SourceUrl}");
                break;

            case FileUploadValue upload:
                Console.WriteLine($"Handling upload: {upload.FileName} to {upload.DestinationUrl}");
                break;

            case FileCompressionValue compression:
                Console.WriteLine($"Handling compression: {compression.FileName} into {compression.ArchiveName}");
                break;

            default:
                Console.WriteLine("Unknown file operation");
                break;
        }

        return parameter;
    }
}