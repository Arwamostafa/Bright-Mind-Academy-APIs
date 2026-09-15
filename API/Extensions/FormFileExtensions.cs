using API.Adapters;
using Domain.Common;
using Microsoft.AspNetCore.Http;

namespace API.Extensions;

public static class FormFileExtensions
{
    public static IFileUpload? ToFileUpload(this IFormFile? file) =>
        file is null ? null : new FormFileUpload(file);
}
