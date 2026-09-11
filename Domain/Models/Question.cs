using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models;

public class Question
{
    public int id { get; set; }
    public string Content { get; set; }
    public int mark { get; set; }
    [ForeignKey("Quiz")]
    public int QuizId { get; set; }
    public virtual Quiz? Quiz { get; set; }
    public virtual ICollection<Option>? Options { get; set; } = new HashSet<Option>();

}
