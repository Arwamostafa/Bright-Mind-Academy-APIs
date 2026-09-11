namespace Domain.DTO;

public class QuestionDto
{
    public int id { get; set; }
    public string Content { get; set; }
    public int mark { get; set; }
    public int QuizId { get; set; }
    public virtual List<OptionDto> Options { get; set; } = new List<OptionDto>();

}
