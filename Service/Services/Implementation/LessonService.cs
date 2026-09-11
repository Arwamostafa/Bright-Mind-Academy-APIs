using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Repository.Contract;
using Repository.Generic;
using Service.Services.Contract;
using System.IO.Compression;

namespace Service.Services.Implementation
{
    public class LessonService(ILessonRepository lessonRepository, IUnitOfWork unitOfWork) : ILessonService
    {
        public async Task<IEnumerable<LessonDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var lessons = await lessonRepository.GetAllWithUnitAsync(cancellationToken);
            return lessons.Select(MapToLessonDto).ToList();
        }

        public async Task<List<string>> SaveFileAsync(IFormFile zipFile, string folderName, CancellationToken cancellationToken = default)
        {
            if (zipFile == null || zipFile.Length == 0 || Path.GetExtension(zipFile.FileName).ToLower() != ".zip")
                return new List<string>();

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string zipPath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString() + ".zip");

            using (var stream = new FileStream(zipPath, FileMode.Create))
            {
                await zipFile.CopyToAsync(stream, cancellationToken);
            }

            List<string> allowedUrls = new List<string>();
            string extractPath = Path.Combine(uploadsFolder, Guid.NewGuid().ToString());

            ZipFile.ExtractToDirectory(zipPath, extractPath);

            var allowedExtensions = new[] { ".pdf", ".mp4", ".mov", ".avi", ".mkv", ".png", ".jpg", ".jpeg" };

            foreach (var file in Directory.GetFiles(extractPath))
            {
                var ext = Path.GetExtension(file).ToLower();
                if (allowedExtensions.Contains(ext))
                {
                    string uniqueFileName = Guid.NewGuid().ToString() + ext;
                    string finalPath = Path.Combine(uploadsFolder, uniqueFileName);
                    File.Move(file, finalPath);

                    string baseUrl = "https://localhost:7092";
                    string url = $"{baseUrl}/{folderName}/{uniqueFileName}";
                    allowedUrls.Add(url);
                }
            }

            File.Delete(zipPath);

            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }

            return allowedUrls;
        }

        public async Task AddAsync(LessonCreateDto lessonCreateDto, CancellationToken cancellationToken = default)
        {
            List<string> videoUrls = new List<string>();
            List<string> pdfUrls = new List<string>();
            List<string> assignmentUrls = new List<string>();
            if (lessonCreateDto.VideoUrl != null)
            {
                var files = await SaveFileAsync(lessonCreateDto.VideoUrl, "Uploads/Videos", cancellationToken);
                videoUrls.AddRange(files.Where(url => url.EndsWith(".mp4")));
            }
            if (lessonCreateDto.PdfUrl != null)
            {
                var files = await SaveFileAsync(lessonCreateDto.PdfUrl, "Uploads/Pdfs", cancellationToken);
                pdfUrls.AddRange(files.Where(url => url.EndsWith(".pdf")));
            }

            if (lessonCreateDto.AssigmentUrl != null)
            {
                var files = await SaveFileAsync(lessonCreateDto.AssigmentUrl, "Uploads/Assignments", cancellationToken);
                assignmentUrls.AddRange(files.Where(url => url.EndsWith(".pdf") || url.EndsWith(".docx")));
            }

            Lesson lesson = new Lesson
            {
                Id = lessonCreateDto.Id,
                Title = lessonCreateDto.Title,
                Description = lessonCreateDto.Description,
                UnitId = lessonCreateDto.UnitId,

                VideoUrl = videoUrls.FirstOrDefault(),
                AssigmentDeadLine = lessonCreateDto.AssigmentDeadLine,
                AssigmentUrl = assignmentUrls.FirstOrDefault(),
                PdfUrl = pdfUrls.FirstOrDefault(),
            };
            await lessonRepository.AddAsync(lesson, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> Delete(int id, CancellationToken cancellationToken = default)
        {
            var lesson = await lessonRepository.GetByIdAsync(id, cancellationToken);
            if (lesson == null)
                return Result.Failure(Error.NotFound("Lesson.NotFound", $"Lesson with id {id} not found"));

            lessonRepository.Remove(lesson);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result<LessonDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var lesson = await lessonRepository.GetWithUnitAsync(id, cancellationToken);
            return lesson is null
                ? Result.Failure<LessonDto>(Error.NotFound("Lesson.NotFound", $"Lesson with id {id} not found"))
                : Result.Success(MapToLessonDto(lesson));
        }

        public async Task<Result> Update(LessonCreateDto lessonCreateDto, int id, CancellationToken cancellationToken = default)
        {
            var lesson = await lessonRepository.GetByIdAsync(id, cancellationToken);
            if (lesson == null)
                return Result.Failure(Error.NotFound("Lesson.NotFound", $"Lesson with id {id} not found"));

            List<string> videoUrls = new List<string>();
            List<string> pdfUrls = new List<string>();
            List<string> assignmentUrls = new List<string>();
            if (lessonCreateDto.VideoUrl != null)
            {
                var files = await SaveFileAsync(lessonCreateDto.VideoUrl, "Uploads/Videos", cancellationToken);
                videoUrls.AddRange(files.Where(url => url.EndsWith(".mp4")));
            }
            if (lessonCreateDto.PdfUrl != null)
            {
                var files = await SaveFileAsync(lessonCreateDto.PdfUrl, "Uploads/Pdfs", cancellationToken);
                pdfUrls.AddRange(files.Where(url => url.EndsWith(".pdf")));
            }

            if (lessonCreateDto.AssigmentUrl != null)
            {
                var files = await SaveFileAsync(lessonCreateDto.AssigmentUrl, "Uploads/Assignments", cancellationToken);
                assignmentUrls.AddRange(files.Where(url => url.EndsWith(".pdf") || url.EndsWith(".docx")));
            }

            lesson.Title = lessonCreateDto.Title;
            lesson.Description = lessonCreateDto.Description;
            lesson.AssigmentDeadLine = lessonCreateDto.AssigmentDeadLine;
            lesson.PdfUrl = pdfUrls.FirstOrDefault();
            lesson.AssigmentUrl = assignmentUrls.FirstOrDefault();
            lesson.VideoUrl = videoUrls.FirstOrDefault();
            lesson.UnitId = lessonCreateDto.UnitId;

            lessonRepository.Update(lesson);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByUnitId(int unitId, CancellationToken cancellationToken = default)
        {
            var lessons = await lessonRepository.GetLessonsByUnitIdAsync(unitId, cancellationToken);
            return lessons.Select(MapToLessonDto).ToList();
        }

        public async Task<IEnumerable<LessonDto>> GetLessonsByUnitName(string unitname, CancellationToken cancellationToken = default)
        {
            var lessons = await lessonRepository.GetLessonsByUnitNameAsync(unitname, cancellationToken);
            return lessons.Select(MapToLessonDto).ToList();
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
