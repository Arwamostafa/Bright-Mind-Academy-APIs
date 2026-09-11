using Domain.Common;
using Domain.DTO;

namespace Application.Services.Contract;

public interface IQuizService
{
    Task<List<QuizDto>> GetAllQuizzesAsync(CancellationToken cancellationToken = default);

    Task<Result<QuizDto>> GetQuizByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<QuizDto>> AddQuizAsync(QuizDto quizDto, CancellationToken cancellationToken = default);

    Task<Result> UpdateQuizAsync(QuizDto quizDto, CancellationToken cancellationToken = default);

    Task<Result> DeleteQuizAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<QuizDto>> GetQuizByLessonIdAsync(int lessonId, CancellationToken cancellationToken = default);
}
