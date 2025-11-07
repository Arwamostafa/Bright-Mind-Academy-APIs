using Domain.DTO;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Contract;
using Repository.Implementation;
using Service.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementation
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository repo;
        private readonly IPaymentRepository _paymentRepository;
        private readonly AppDbContext _context;



        public SubjectService(ISubjectRepository _repo, AppDbContext context, IPaymentRepository paymentRepository)
        {
            this.repo = _repo;
            _paymentRepository = paymentRepository;
            _context = context;
        }

        public List<SubjectWithUnits> GetAllSubjects()
        {
            return repo.GetAll();
        }

        public Subject GetSubjectById(int id)
        {
            return repo.GetById(id);
        }

        public Subject GetSubjectByName(string name)
        {
            return repo.GetByName(name);
        }

        public CreatedSubjectDTO AddSubject(CreatedSubjectDTO addedSubjectDTO)
        {
            try
            {
                repo.Add(addedSubjectDTO);
                //repo.Save();
                return addedSubjectDTO;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string RemoveSubjectById(int id)
        {
            try
            {
                repo.RemoveById(id);
                repo.Save();
                return "Subject added Successfully";
            }
            catch (Exception ex)
            {
                return "Failed to be removed";
            }
        }



        public string UpdateSubjectById(int id, CreatedSubjectDTO updatedSubjectDTO)
        {
            try
            {
                repo.UpdateById(id, updatedSubjectDTO);
                repo.Save();
                return "Subject updated successfully";
            }
            catch (Exception ex)
            {
                return $"Failed to be updated: {ex.Message}";
            }
        }

        public async Task<List<SubjectDto>> TopThreeSubjects()
        {
            var subjects = await _paymentRepository.TopThreeSubjects();
            if (subjects == null)
                throw new Exception("No subjects found.");

            return subjects.Select(s => new SubjectDto
            {
                SubjectId = s.SubjectID,
                SubjectName = s.Subject?.SubjectName,
                SubjectDescription = s.Subject?.SubjectDescription,
                SubjectPrice = s.Subject?.Price,
                ImgUrl = s.Instructor.Image,
                ClassId = s.ClassID,
                ClassName = s.Class?.ClassName,
                InstructorName = s.Instructor?.User?.FirstName + " " + s.Instructor?.User?.LastName,
                TrackId = s.TrackID,
                TrackName = s.Track?.TrackName,
                unitCount = _context.Units.Count(u => u.SubjectId == s.SubjectID)
            }).ToList();
        }


        public async Task<List<SubjectDto>> GetPageOfSubjects(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;
            var subjects = await repo.GetAllSubjectPagination();
            if (subjects == null)
                throw new Exception("No subjects found.");
            var subjectsPages = subjects
               .Skip((pageNumber - 1) * pageSize)
               .Take(pageSize)
               .Select(cts => new SubjectDto
               {
                   SubjectId = cts.SubjectID,
                   InstructorName = cts.Subject.Instructor.User.FirstName + " " + cts.Subject.Instructor.User.LastName,
                   SubjectName = cts.Subject.SubjectName,
                   SubjectPrice = cts.Subject.Price,
                   SubjectDescription = cts.Subject.SubjectDescription,
                   ImgUrl = cts.Subject.Instructor.Image,
                   ClassId = cts.ClassID,
                   ClassName = cts.Class.ClassName,
                   TrackId = cts.TrackID,
                   TrackName = cts.Track.TrackName,
                   unitCount = _context.Units.Count(u => u.SubjectId == cts.SubjectID)
               })
               .ToList();
            return subjectsPages;
        }

        public Task<int> GetTotalSubjectsCount()
        {
            return repo.GetTotalSubjectsCount();
        }
    }
}