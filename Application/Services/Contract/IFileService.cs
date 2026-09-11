using Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Contract;

public interface IFileService
{
    /// <summary>
    /// Validates and saves a single uploaded file into a wwwroot subfolder. Validates,
    /// in order: presence, extension against the category's allow-list, size against the
    /// category's limit (checked via IFormFile.Length before anything is written to disk),
    /// and the file's actual content via a magic-byte signature check so a renamed file
    /// (e.g. a .exe renamed to .jpg) can't slip through on extension/Content-Type alone.
    /// Returns the saved file's relative URL on success.
    /// </summary>
    Task<Result<string>> UploadAsync(IFormFile? file, FileCategory category, string folderName, CancellationToken cancellationToken = default);
}
