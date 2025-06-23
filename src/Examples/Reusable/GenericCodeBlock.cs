using MM.PipeBlocks;

namespace Reusable;
public class GenericCodeBlock : CodeBlock<ICustomContext, ICustomValue>
{
    protected override ICustomContext Execute(ICustomContext context, ICustomValue value)
    {
        Console.WriteLine($"Processing file operation for '{context.FileName}'");
        switch (context)
        {
            case FileDownloadContext download:
                Console.WriteLine($"Handling download: {download.FileName} from {download.SourceUrl}");
                break;

            case FileUploadContext upload:
                Console.WriteLine($"Handling upload: {upload.FileName} to {upload.DestinationUrl}");
                break;

            case FileCompressionContext compression:
                Console.WriteLine($"Handling compression: {compression.FileName} into {compression.ArchiveName}");
                break;

            default:
                Console.WriteLine("Unknown file operation");
                break;
        }

        return context;
    }
}