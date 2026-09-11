using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Repository.Contract;
using Repository.Generic;
using Service.Services.Contract;

namespace Service.Services.Implementation
{
    public class UnitService(IUnitRepository unitRepository, ILessonRepository lessonRepository, IUnitOfWork unitOfWork) : IUnitService
    {
        public async Task AddAsync(UnitCreateDto unitCreateDto, CancellationToken cancellationToken = default)
        {
            Unit unit = new Unit
            {
                Id = unitCreateDto.Id,
                Title = unitCreateDto.Title,
                Description = unitCreateDto.Description,
                SubjectId = unitCreateDto.SubjectId,
            };
            await unitRepository.AddAsync(unit, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<UnitDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var units = await unitRepository.GetAllWithSubjectAsync(cancellationToken);
            return units.Select(MapToDto).ToList();
        }

        public async Task<Result<UnitWithSubjectAndLessonsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var unit = await unitRepository.GetWithDetailsAsync(id, cancellationToken);
            return unit is null
                ? Result.Failure<UnitWithSubjectAndLessonsDto>(Error.NotFound("Unit.NotFound", $"Unit with id {id} not found"))
                : Result.Success(MapToDtoAll(unit));
        }

        public async Task<Result> Delete(int id, CancellationToken cancellationToken = default)
        {
            var unit = await unitRepository.GetByIdAsync(id, cancellationToken);
            if (unit is null)
                return Result.Failure(Error.NotFound("Unit.NotFound", $"Unit with id {id} not found"));

            unitRepository.Remove(unit);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result> Update(UnitCreateDto unitDto, int id, CancellationToken cancellationToken = default)
        {
            var unit = await unitRepository.GetByIdAsync(id, cancellationToken);
            if (unit is null)
                return Result.Failure(Error.NotFound("Unit.NotFound", $"Unit with id {id} not found"));

            unit.Title = unitDto.Title;
            unit.Description = unitDto.Description;
            unit.SubjectId = unitDto.SubjectId;
            unitRepository.Update(unit);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<UnitWithSubjectAndLessonsDto>> GetUnitByLessonId(int lessonId, CancellationToken cancellationToken = default)
        {
            var lesson = await lessonRepository.GetWithUnitAsync(lessonId, cancellationToken);
            if (lesson?.Unit is null)
                return Result.Failure<UnitWithSubjectAndLessonsDto>(Error.NotFound("Unit.NotFound", $"No unit found for lesson ID {lessonId}"));

            return Result.Success(MapToDtoAll(lesson.Unit));
        }

        public Task<List<Unit>> GetUnitsBySubjectId(int subjectId, CancellationToken cancellationToken = default) =>
            unitRepository.GetUnitsBySubjectIdAsync(subjectId, cancellationToken);

        public Task<List<Unit>> GetUnitsBySubjectName(string subjectname, CancellationToken cancellationToken = default) =>
            unitRepository.GetUnitsBySubjectNameAsync(subjectname, cancellationToken);

        public Task<int> GetNumberOfUnitsBySubjectId(int subjectId, CancellationToken cancellationToken = default) =>
            unitRepository.GetUnitsCountBySubjectIdAsync(subjectId, cancellationToken);

        private static UnitDto MapToDto(Unit unit)
        {
            return new UnitDto
            {
                Id = unit.Id,
                Title = unit.Title,
                Description = unit.Description,
                SubjectId = unit.SubjectId,
                SubjectName = unit.Subject?.SubjectName,
            };
        }

        private static UnitWithSubjectAndLessonsDto MapToDtoAll(Unit unit)
        {
            return new UnitWithSubjectAndLessonsDto
            {
                Id = unit.Id,
                Title = unit.Title,
                Description = unit.Description,
                SubjectId = unit.SubjectId,
                SubjectName = unit.Subject?.SubjectName,
                Lessons = unit.Lessons?.Select(lesson => new LessonDto
                {
                    Id = lesson.Id,
                    Title = lesson.Title,
                    Description = lesson.Description,
                    VideoUrl = lesson.VideoUrl,
                    PdfUrl = lesson.PdfUrl,
                    AssigmentUrl = lesson.AssigmentUrl,
                    AssigmentDeadLine = lesson.AssigmentDeadLine,
                    UnitId = lesson.UnitId,
                    UnitName = unit.Title
                }).ToList() ?? new List<LessonDto>()
            };
        }
    }
}
