namespace Domain.DTO;

public class QuizDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public bool AssignedBefore { get; set; }
    public int TotalMarks { get; set; }

    public int LessonId { get; set; }


    public List<QuestionDto>? Questions { get; set; } = new List<QuestionDto>();

}
