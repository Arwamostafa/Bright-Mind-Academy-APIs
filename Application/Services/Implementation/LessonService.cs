using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Application.Repositories;
using Application.Services.Contract;

namespace Application.Services.Implementation
{
    public class LessonService(IUnitOfWork unitOfWork, IFileService fileService) : ILessonService
    {
        private IGenericRepository<Lesson> Repo => unitOfWork.Repository<Lesson>();

        public async Task<IEnumerable<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var lessons = await Repo.FindAllAsync(include: q => q.Include(l => l.Unit), cancellationToken: cancellationToken);
            return lessons.Select(MapToLessonDto).ToList();
        }

        public async Task<Result> AddAsync(LessonCreateDto lessonCreateDto, CancellationToken cancellationToken = default)
        {
            var videoResult = await UploadIfProvidedAsync(lessonCreateDto.VideoUrl, FileCategory.Video, "Uploads/Videos", cancellationToken);
            if (videoResult.IsFailure) return Result.Failure(videoResult.Error);

            var pdfResult = await UploadIfProvidedAsync(lessonCreateDto.PdfUrl, FileCategory.Document, "Uploads/Pdfs", cancellationToken);
            if (pdfResult.IsFailure) return Result.Failure(pdfResult.Error);

            var assignmentResult = await UploadIfProvidedAsync(lessonCreateDto.AssigmentUrl, FileCategory.Document, "Uploads/Assignments", cancellationToken);
            if (assignmentResult.IsFailure) return Result.Failure(assignmentResult.Error);

            Lesson lesson = new Lesson
            {
                Id = lessonCreateDto.Id,
                Title = lessonCreateDto.Title,
                Description = lessonCreateDto.Description,
                UnitId = lessonCreateDto.UnitId,
                VideoUrl = videoResult.Value,
                AssigmentDeadLine = lessonCreateDto.AssigmentDeadLine,
                AssigmentUrl = assignmentResult.Value,
                PdfUrl = pdfResult.Value,
            };
            await Repo.AddAsync(lesson, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result> Delete(int id, CancellationToken cancellationToken = default)
        {
            var lesson = await Repo.GetByIdAsync(id, cancellationToken);
            if (lesson == null)
                return Result.Failure(Error.NotFound("Lesson.NotFound", $"Lesson with id {id} not found"));

            Repo.Remove(lesson);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<LessonDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var lesson = await Repo.FindAsync(l => l.Id == id, include: q => q.Include(l => l.Unit), cancellationToken: cancellationToken);
            return lesson is null
                ? Result.Failure<LessonDto>(Error.NotFound("Lesson.NotFound", $"Lesson with id {id} not found"))
                : Result.Success(MapToLessonDto(lesson));
        }

        public async Task<Result> Update(LessonCreateDto lessonCreateDto, int id, CancellationToken cancellationToken = default)
        {
            var lesson = await Repo.GetByIdAsync(id, cancellationToken);
            if (lesson == null)
                return Result.Failure(Error.NotFound("Lesson.NotFound", $"Lesson with id {id} not found"));

            var videoResult = await UploadIfProvidedAsync(lessonCreateDto.VideoUrl, FileCategory.Video, "Uploads/Videos", cancellationToken);
            if (videoResult.IsFailure) return Result.Failure(videoResult.Error);

            var pdfResult = await UploadIfProvidedAsync(lessonCreateDto.PdfUrl, FileCategory.Document, "Uploads/Pdfs", cancellationToken);
            if (pdfResult.IsFailure) return Result.Failure(pdfResult.Error);

            var assignmentResult = await UploadIfProvidedAsync(lessonCreateDto.AssigmentUrl, FileCategory.Document, "Uploads/Assignments", cancellationToken);
            if (assignmentResult.IsFailure) return Result.Failure(assignmentResult.Error);

            lesson.Title = lessonCreateDto.Title;
            lesson.Description = lessonCreateDto.Description;
            lesson.AssigmentDeadLine = lessonCreateDto.AssigmentDeadLine;
            // Keep the existing file when no replacement was uploaded, instead of nulling it out.
            lesson.PdfUrl = pdfResult.Value ?? lesson.PdfUrl;
            lesson.AssigmentUrl = assignmentResult.Value ?? lesson.AssigmentUrl;
            lesson.VideoUrl = videoResult.Value ?? lesson.VideoUrl;
            lesson.UnitId = lessonCreateDto.UnitId;

            Repo.Update(lesson);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByUnitId(int unitId, CancellationToken cancellationToken = default)
        {
            var lessons = await Repo.FindAllAsync(l => l.UnitId == unitId, include: q => q.Include(l => l.Unit), cancellationToken: cancellationToken);
            return lessons.Select(MapToLessonDto).ToList();
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByUnitName(string unitname, CancellationToken cancellationToken = default)
        {
            var lessons = await Repo.FindAllAsync(l => l.Unit.Title == unitname, include: q => q.Include(l => l.Unit), cancellationToken: cancellationToken);
            return lessons.Select(MapToLessonDto).ToList();
        }

        private async Task<Result<string?>> UploadIfProvidedAsync(IFormFile? file, FileCategory category, string folderName, CancellationToken cancellationToken)
        {
            if (file is null)
                return Result.Success<string?>(null);

            var result = await fileService.UploadAsync(file, category, folderName, cancellationToken);
            return result.IsSuccess ? Result.Success<string?>(result.Value) : Result.Failure<string?>(result.Error);
        }

        private static LessonDto MapToLessonDto(Lesson lesson)
        {
            return new LessonDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                UnitId = lesson.UnitId,
                VideoUrl = lesson.VideoUrl,
                AssigmentDeadLine = lesson.AssigmentDeadLine,
                AssigmentUrl = lesson.AssigmentUrl,
                PdfUrl = lesson.PdfUrl,
                UnitName = lesson?.Unit?.Title ?? "no unit "
            };
        }
    }
}
