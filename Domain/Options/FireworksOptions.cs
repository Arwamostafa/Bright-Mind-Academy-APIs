using System.ComponentModel.DataAnnotations;

namespace Domain.Options;

public class FireworksOptions
{
    public const string SectionName = "Fireworks";

    [Required]
    public string APIKey { get; set; } = string.Empty;

    [Required]
    public string ChatEndPoint { get; set; } = string.Empty;

    [Required]
    public string ChatModelName { get; set; } = string.Empty;

    public FireworksEmbeddingOptions Embedding { get; set; } = new();
}

public class FireworksEmbeddingOptions
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string ModelName { get; set; } = string.Empty;
}
