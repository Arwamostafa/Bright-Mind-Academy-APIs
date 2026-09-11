using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Application.Repositories;
using Application.Services.Contract;

namespace Application.Services.Implementation
{
    public class QuizService(IUnitOfWork unitOfWork) : IQuizService
    {
        private IGenericRepository<Quiz> Repo => unitOfWork.Repository<Quiz>();

        public async Task<List<QuizDto>> GetAllQuizzesAsync(CancellationToken cancellationToken = default)
        {
            var quizzes = await Repo.FindAllAsync(include: q => q.Include(x => x.Questions).ThenInclude(x => x.Options), cancellationToken: cancellationToken);
            return quizzes.Select(MapToDto).ToList();
        }

        public async Task<Result<QuizDto>> GetQuizByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quiz = await Repo.FindAsync(
                q => q.Id == id,
                include: q => q.Include(x => x.Questions).ThenInclude(x => x.Options),
                cancellationToken: cancellationToken);
            return quiz is null
                ? Result.Failure<QuizDto>(Error.NotFound("Quiz.NotFound", $"No quiz with id {id}."))
                : Result.Success(MapToDto(quiz));
        }

        public async Task<Result<QuizDto>> AddQuizAsync(QuizDto quizDto, CancellationToken cancellationToken = default)
        {
            var exists = await Repo.AnyAsync(q => q.Id == quizDto.Id, cancellationToken);
            if (exists)
                return Result.Failure<QuizDto>(Error.Conflict("Quiz.AlreadyExists", "Quiz already exists."));

            var quiz = new Quiz
            {
                Description = quizDto.Description,
                AssignedBefore = quizDto.AssignedBefore,
                TotalMarks = quizDto.TotalMarks,
                LessonId = quizDto.LessonId,
                Questions = quizDto.Questions?.Select(q => new Question
                {
                    Content = q.Content,
                    mark = q.mark,
                    Options = q.Options.Select(o => new Option
                    {
                        Name = o.Name,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList() ?? new List<Question>()
            };

            await Repo.AddAsync(quiz, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            quizDto.Id = quiz.Id;
            return Result.Success(quizDto);
        }

        public async Task<Result> UpdateQuizAsync(QuizDto quizDto, CancellationToken cancellationToken = default)
        {
            var found = await Repo.FindAsync(
                q => q.Id == quizDto.Id,
                include: q => q.Include(x => x.Questions).ThenInclude(x => x.Options),
                asNoTracking: false,
                cancellationToken: cancellationToken);
            if (found is null)
                return Result.Failure(Error.NotFound("Quiz.NotFound", $"No quiz with id {quizDto.Id}."));

            found.AssignedBefore = quizDto.AssignedBefore;
            found.Description = quizDto.Description;
            found.TotalMarks = quizDto.TotalMarks;
            found.LessonId = quizDto.LessonId;

            var optionRepository = unitOfWork.Repository<Option>();
            var questionRepository = unitOfWork.Repository<Question>();

            foreach (var item in found.Questions.ToList())
            {
                var check = quizDto.Questions?.FirstOrDefault(q => q.id == item.id);
                if (check != null)
                {
                    item.mark = check.mark;
                    item.Content = check.Content;

                    foreach (var itemm in item.Options!.ToList())
                    {
                        var checkoption = check.Options.FirstOrDefault(q => q.id == itemm.id);
                        if (checkoption != null)
                        {
                            itemm.IsCorrect = checkoption.IsCorrect;
                            itemm.Name = checkoption.Name;
                        }
                        else
                        {
                            optionRepository.Remove(itemm);
                        }
                    }

                    foreach (var newOpt in check.Options.Where(o => o.id == 0))
                    {
                        item.Options!.Add(new Option
                        {
                            Name = newOpt.Name,
                            IsCorrect = newOpt.IsCorrect
                        });
                    }
                }
                else
                {
                    questionRepository.Remove(item);
                }
            }

            foreach (var newQuestion in quizDto.Questions?.Where(q => q.id == 0) ?? Enumerable.Empty<QuestionDto>())
            {
                var question = new Question
                {
                    Content = newQuestion.Content,
                    mark = newQuestion.mark,
                    Options = newQuestion.Options.Select(opt => new Option
                    {
                        Name = opt.Name,
                        IsCorrect = opt.IsCorrect
                    }).ToList()
                };

                found.Questions.Add(question);
            }

            Repo.Update(found);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result> DeleteQuizAsync(int id, CancellationToken cancellationToken = default)
        {
            var found = await Repo.FindAsync(
                q => q.Id == id,
                include: q => q.Include(x => x.Questions).ThenInclude(x => x.Options),
                asNoTracking: false,
                cancellationToken: cancellationToken);
            if (found is null)
                return Result.Failure(Error.NotFound("Quiz.NotFound", $"No quiz with id {id}."));

            Repo.Remove(found);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<QuizDto>> GetQuizByLessonIdAsync(int lessonId, CancellationToken cancellationToken = default)
        {
            var quiz = await Repo.FindAsync(
                q => q.LessonId == lessonId,
                include: q => q.Include(x => x.Questions).ThenInclude(x => x.Options),
                cancellationToken: cancellationToken);
            return quiz is null
                ? Result.Failure<QuizDto>(Error.NotFound("Quiz.NotFound", $"No quiz found for lesson {lessonId}."))
                : Result.Success(MapToDto(quiz));
        }

        private static QuizDto MapToDto(Quiz quiz) => new()
        {
            Id = quiz.Id,
            AssignedBefore = quiz.AssignedBefore,
            Description = quiz.Description,
            TotalMarks = quiz.TotalMarks,
            LessonId = quiz.LessonId,
            Questions = quiz.Questions.Select(question => new QuestionDto
            {
                id = question.id,
                mark = question.mark,
                Content = question.Content,
                Options = question.Options?.Select(option => new OptionDto
                {
                    id = option.id,
                    Name = option.Name,
                    IsCorrect = option.IsCorrect
                }).ToList() ?? new List<OptionDto>()
            }).ToList()
        };
    }
}
