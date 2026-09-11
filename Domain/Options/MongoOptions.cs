using System.ComponentModel.DataAnnotations;

namespace Domain.Options;

public class MongoOptions
{
    public const string SectionName = "Mongo";

    [Required]
    public string RagDbConnection { get; set; } = string.Empty;
}
