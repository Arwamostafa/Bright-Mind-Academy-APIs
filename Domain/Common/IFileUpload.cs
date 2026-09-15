namespace Domain.Common;

/// <summary>
/// Domain-safe abstraction over an uploaded file, so Domain/Application don't need to
/// reference ASP.NET Core's IFormFile. The API layer adapts IFormFile to this at the edge.
/// </summary>
public interface IFileUpload
{
    string FileName { get; }
    string ContentType { get; }
    long Length { get; }
    Stream OpenReadStream();
}
