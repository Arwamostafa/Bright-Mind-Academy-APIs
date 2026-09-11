using Domain.Common;

namespace Domain.Options;

public class FileUploadOptions
{
    public const string SectionName = "FileUpload";

    public long ImageMaxSizeBytes { get; set; } = 5 * 1024 * 1024;
    public long VideoMaxSizeBytes { get; set; } = 7 * 1024 * 1024;
    public long DocumentMaxSizeBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedImageExtensions { get; set; } = [".png", ".jpg", ".jpeg"];
    public string[] AllowedVideoExtensions { get; set; } = [".mp4", ".mov", ".avi", ".mkv"];
    public string[] AllowedDocumentExtensions { get; set; } = [".pdf", ".docx"];

    public long MaxSizeBytesFor(FileCategory category) => category switch
    {
        FileCategory.Image => ImageMaxSizeBytes,
        FileCategory.Video => VideoMaxSizeBytes,
        FileCategory.Document => DocumentMaxSizeBytes,
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    public string[] AllowedExtensionsFor(FileCategory category) => category switch
    {
        FileCategory.Image => AllowedImageExtensions,
        FileCategory.Video => AllowedVideoExtensions,
        FileCategory.Document => AllowedDocumentExtensions,
        _ => throw new ArgumentOutOfRangeException(nameof(category))
    };

    public FileCategory? CategoryForExtension(string extension)
    {
        if (AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return FileCategory.Image;
        if (AllowedVideoExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return FileCategory.Video;
        if (AllowedDocumentExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return FileCategory.Document;
        return null;
    }
}
