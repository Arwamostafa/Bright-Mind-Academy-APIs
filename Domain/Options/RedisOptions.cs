using System.ComponentModel.DataAnnotations;

namespace Domain.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    public string InstanceName { get; set; } = "BrightMindAcademy:";

    public int DefaultExpirationMinutes { get; set; } = 5;
}
