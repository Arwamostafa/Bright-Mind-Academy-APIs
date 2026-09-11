using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public class Option
{
    public int id { get; set; }
    public string Name { get; set; }
    public bool IsCorrect { get; set; }

    [ForeignKey("Question")]
    public int QuestionId { get; set; }
    public virtual Question? Question { get; set; }
}
