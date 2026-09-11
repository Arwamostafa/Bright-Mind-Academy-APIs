namespace Domain.DTO;

public class OptionDto
{
    public int id { get; set; }
    public string Name { get; set; }
    public bool IsCorrect { get; set; }
    public int QuestionId { get; set; }

}
