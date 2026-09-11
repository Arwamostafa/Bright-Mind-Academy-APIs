using Domain.DTO;
using API.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.Services.Contract;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpGet("GetAll")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var lessons = await _lessonService.GetAllAsync(cancellationToken);
            if (lessons == null || !lessons.Any())
                return Ok("No lessons found.");
            return Ok(lessons);
        }

        [HttpGet("GetById/{id}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var result = await _lessonService.GetByIdAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

        // Video (7MB) + two documents (10MB each) plus multipart overhead - was 500MB, which
        // defeated the point of the per-file size limits enforced in IFileService.
        [RequestSizeLimit(30_000_000)]
        [HttpPost("Add")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> Add([FromForm] LessonCreateDto lessonDto, CancellationToken cancellationToken)
        {
            var result = await _lessonService.AddAsync(lessonDto, cancellationToken);
            return result.ToActionResult(this);
        }

        [RequestSizeLimit(30_000_000)]
        [HttpPut("Update/{id}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> Update(int id, [FromForm] LessonCreateDto lessonDto, CancellationToken cancellationToken)
        {
            var result = await _lessonService.Update(lessonDto, id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("Delete/{id}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _lessonService.Delete(id, cancellationToken);
            return result.ToActionResult(this);
        }


        [HttpGet("by-lesson-name/{unitName}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<ActionResult<IEnumerable<LessonDto>>> GetLessonsByUnitName(string unitName, CancellationToken cancellationToken)
        {
            var lessons = await _lessonService.GetLessonsByUnitName(unitName, cancellationToken);
            return Ok(lessons);
        }

        [HttpGet("by-unit-id/{unitId}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<ActionResult<IEnumerable<LessonDto>>> GetLessonsByUnitId(int unitId, CancellationToken cancellationToken)
        {
            if (unitId <= 0)
                return BadRequest("Invalid unit ID.");

            var lessons = await _lessonService.GetLessonsByUnitId(unitId, cancellationToken);
            return Ok(lessons);
        }

        [HttpGet("download/by-url")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public IActionResult DownloadByUrl([FromQuery] string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return BadRequest("File URL is required.");

            var uri = new Uri(fileUrl);
            var relativePath = uri.LocalPath.TrimStart('/');
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            var mimeType = "application/octet-stream";
            return PhysicalFile(filePath, mimeType, Path.GetFileName(filePath));
        }


    }
}
