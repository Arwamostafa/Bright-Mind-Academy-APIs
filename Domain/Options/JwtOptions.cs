using System.ComponentModel.DataAnnotations;

namespace Domain.Options;

public class JwtOptions
{
    public const string SectionName = "JWT";

    [Required]
    public string AudienceIP { get; set; } = string.Empty;

    [Required]
    public string IssuerIP { get; set; } = string.Empty;

    [Required]
    public string SCRKey { get; set; } = string.Empty;
}
