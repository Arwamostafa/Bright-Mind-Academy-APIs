using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Application.Repositories;
using Application.Services.Contract;

namespace Application.Services.Implementation
{
    public class SubjectService(IUnitOfWork unitOfWork, ISubjectQueryService queryService) : ISubjectService
    {
        private IGenericRepository<Subject> Repo => unitOfWork.Repository<Subject>();
        private IGenericRepository<StudentClassSubject> ClassSubjectRepo => unitOfWork.Repository<StudentClassSubject>();

        public async Task<Result<Subject>> GetSubjectByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var subject = await Repo.GetByIdAsync(id, cancellationToken);
            return subject is null
                ? Result.Failure<Subject>(Error.NotFound("Subject.NotFound", $"Subject with id {id} was not found."))
                : Result.Success(subject);
        }

        public async Task<Result<Subject>> GetSubjectByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var subject = await Repo.FindAsync(c => c.SubjectName == name, cancellationToken: cancellationToken);
            return subject is null
                ? Result.Failure<Subject>(Error.NotFound("Subject.NotFound", $"Subject named '{name}' was not found."))
                : Result.Success(subject);
        }

        public async Task<Result<CreatedSubjectDTO>> AddSubjectAsync(CreatedSubjectDTO addedSubjectDTO, CancellationToken cancellationToken = default)
        {
            var subject = new Subject
            {
                SubjectName = addedSubjectDTO.SubjectName,
                SubjectDescription = addedSubjectDTO.SubjectDescription,
                InstructorID = addedSubjectDTO.InstructorID,
                Price = addedSubjectDTO.Price
            };

            await Repo.AddAsync(subject, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var studentClassSubject = new StudentClassSubject
            {
                SubjectID = subject.SubjectID,
                InstructorID = addedSubjectDTO.InstructorID,
                ClassID = addedSubjectDTO.ClassID,
                TrackID = addedSubjectDTO.TrackID
            };

            await ClassSubjectRepo.AddAsync(studentClassSubject, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await queryService.InvalidateCachesAsync(cancellationToken);

            addedSubjectDTO.SubjectID = subject.SubjectID;
            return Result.Success(addedSubjectDTO);
        }

        public async Task<Result> RemoveSubjectByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var subject = await Repo.GetByIdAsync(id, cancellationToken);
            if (subject is null)
                return Result.Failure(Error.NotFound("Subject.NotFound", $"Subject with id {id} was not found."));

            Repo.Remove(subject);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await queryService.InvalidateCachesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result> UpdateSubjectByIdAsync(int id, CreatedSubjectDTO updatedSubjectDTO, CancellationToken cancellationToken = default)
        {
            var subject = await Repo.GetByIdAsync(id, cancellationToken);
            var oldClassSubject = await ClassSubjectRepo.FindAsync(sc => sc.SubjectID == id, asNoTracking: false, cancellationToken: cancellationToken);

            if (subject is null || oldClassSubject is null)
                return Result.Failure(Error.NotFound("Subject.NotFound", $"Subject with id {id} was not found."));

            subject.SubjectName = updatedSubjectDTO.SubjectName;
            subject.SubjectDescription = updatedSubjectDTO.SubjectDescription;
            subject.InstructorID = updatedSubjectDTO.InstructorID;
            subject.Price = updatedSubjectDTO.Price;
            Repo.Update(subject);

            ClassSubjectRepo.Remove(oldClassSubject);
            await ClassSubjectRepo.AddAsync(new StudentClassSubject
            {
                SubjectID = id,
                InstructorID = updatedSubjectDTO.InstructorID,
                ClassID = updatedSubjectDTO.ClassID,
                TrackID = updatedSubjectDTO.TrackID
            }, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await queryService.InvalidateCachesAsync(cancellationToken);
            return Result.Success();
        }

        public Task<int> GetTotalSubjectsCountAsync(CancellationToken cancellationToken = default) =>
            Repo.CountAsync(cancellationToken: cancellationToken);
    }
}
