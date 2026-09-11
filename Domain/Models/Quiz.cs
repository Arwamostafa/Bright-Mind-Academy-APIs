namespace Domain.Models;

public class Quiz
{
    public int Id { get; set; }
    public string Description { get; set; }
    public bool AssignedBefore { get; set; }
    public int TotalMarks { get; set; }
    public virtual ICollection<Question> Questions { get; set; } = new HashSet<Question>();
    public int LessonId { get; set; }
    public virtual Lesson Lesson { get; set; }

}
