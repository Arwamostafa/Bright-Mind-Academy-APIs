using Domain.Common;
using Domain.DTO;
using Domain.Models;

namespace Application.Services.Contract
{
    public interface ISubjectService
    {
        Task<Result<Subject>> GetSubjectByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Result<Subject>> GetSubjectByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<Result<CreatedSubjectDTO>> AddSubjectAsync(CreatedSubjectDTO addedSubject, CancellationToken cancellationToken = default);

        Task<Result> RemoveSubjectByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Result> UpdateSubjectByIdAsync(int id, CreatedSubjectDTO upSubjectDTO, CancellationToken cancellationToken = default);

        Task<int> GetTotalSubjectsCountAsync(CancellationToken cancellationToken = default);
    }
}
