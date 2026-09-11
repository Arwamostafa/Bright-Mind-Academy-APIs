using Domain.Common;
using Domain.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Service.Services.Contract;

namespace Service.Services.Implementation;

public class FileService(IOptions<FileUploadOptions> options) : IFileService
{
    private const string BaseUrl = "https://localhost:7092";

    private readonly FileUploadOptions _options = options.Value;

    public async Task<Result<string>> UploadAsync(IFormFile? file, FileCategory category, string folderName, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return Result.Failure<string>(Error.Validation("File.Empty", "No file was provided."));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = _options.AllowedExtensionsFor(category);
        if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return Result.Failure<string>(Error.Validation(
                "File.InvalidExtension",
                $"'{extension}' is not an allowed {category} extension. Allowed: {string.Join(", ", allowedExtensions)}"));

       
        var maxSizeBytes = _options.MaxSizeBytesFor(category);
        if (file.Length > maxSizeBytes)
            return Result.Failure<string>(Error.Validation("File.TooLarge",$"{category} files must be {maxSizeBytes / (1024 * 1024)} MB or smaller."));

        if (!await FileSignatureValidator.MatchesAsync(file, cancellationToken))
            return Result.Failure<string>(Error.Validation("File.ContentMismatch",$"File content does not match a valid {extension} file - it may have been renamed."));

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var finalPath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(finalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return Result.Success($"{BaseUrl}/{folderName}/{uniqueFileName}");
    }
}
