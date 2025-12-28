namespace Reusable;

public interface ICustomValue
{
    string FileName { get; set; }
}

public class FileDownloadValue : ICustomValue
{
    public required string FileName { get; set; }
    public required string SourceUrl { get; set; }
}

public class FileUploadValue : ICustomValue
{
    public required string FileName { get; set; }
    public required string DestinationUrl { get; set; }
}

public class FileCompressionValue : ICustomValue
{
    public required string FileName { get; set; }
    public required string ArchiveName { get; set; }
}