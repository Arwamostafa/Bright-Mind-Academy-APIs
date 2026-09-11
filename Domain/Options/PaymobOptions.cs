using System.ComponentModel.DataAnnotations;

namespace Domain.Options;

public class PaymobOptions
{
    public const string SectionName = "Paymob";

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string HmacSecret { get; set; } = string.Empty;

    public int IntegrationId { get; set; }

    [Required]
    public string IframeId { get; set; } = string.Empty;
}
