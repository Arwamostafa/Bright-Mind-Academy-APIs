using Domain.DTO;
using E_LearningPlatform.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contract;

namespace E_LearningPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService newSubject;
        public SubjectController(ISubjectService _subject)
        {
            newSubject = _subject;
        }

        [HttpGet]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> GetAllSubjects(CancellationToken cancellationToken)
        {
            var response = await newSubject.GetAllSubjectsAsync(cancellationToken);
            return Ok(response);
        }


        [HttpGet("GetSubjectByClassIdAndTrackId/{classId:int}/{trackId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubjectByClassIdAndTrackId(int classId, int trackId, CancellationToken cancellationToken)
        {
            var results = await newSubject.GetSubjectsByClassAndTrackAsync(classId, trackId, cancellationToken);
            return Ok(results);
        }

        [HttpGet("GetHomeSubjectById/{Id:int}")]
        //[AllowAnonymous]
        public async Task<IActionResult> GetHomeSubjectById(int id, CancellationToken cancellationToken)
        {
            var result = await newSubject.GetHomeSubjectByIdAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("GetHomeSubjects")]
        //[AllowAnonymous]
        public async Task<IActionResult> GetHomeSubjects(CancellationToken cancellationToken)
        {
            var results = await newSubject.GetHomeSubjectsAsync(cancellationToken);
            return Ok(results);
        }

        [HttpGet("id/{id:int}")]
        public async Task<IActionResult> GetSubjectById(int id, CancellationToken cancellationToken)
        {
            var result = await newSubject.GetSubjectByIdAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("name/{name:alpha}")]
        public async Task<IActionResult> GetSubjectByName(string name, CancellationToken cancellationToken)
        {
            var result = await newSubject.GetSubjectByNameAsync(name, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> PostSubject([FromBody] CreatedSubjectDTO subjectDTO, CancellationToken cancellationToken)
        {
            var result = await newSubject.AddSubjectAsync(subjectDTO, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPut("{id:int}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] CreatedSubjectDTO upSubjectDTO, CancellationToken cancellationToken)
        {
            var result = await newSubject.UpdateSubjectByIdAsync(id, upSubjectDTO, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("{id:int}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> DeleteSubjectById(int id, CancellationToken cancellationToken)
        {
            var result = await newSubject.RemoveSubjectByIdAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("Top3Subject")]
        public async Task<IActionResult> GetTop3Subject(CancellationToken cancellationToken)
        {
            var subjects = await newSubject.TopThreeSubjectsAsync(cancellationToken);
            return Ok(subjects);
        }



        [HttpGet("GetPageOfSubjects")]
        public async Task<IActionResult> GetPageOfSubjects(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var response = await newSubject.GetPageOfSubjectsAsync(pageNumber, pageSize, cancellationToken);
            return Ok(response);
        }

        [HttpGet("CountNymberOfSubjects")]
        public async Task<IActionResult> CountNymberOfSubjects(CancellationToken cancellationToken)
        {
            var response = await newSubject.GetTotalSubjectsCountAsync(cancellationToken);
            return Ok(response);
        }

        [HttpGet("GetStudentsBySubjectId/{subjectId:int}")]
        public async Task<IActionResult> GetStudentsBySubjectId(int subjectId, CancellationToken cancellationToken)
        {
            var response = await newSubject.GetStudentsBySubjectIdAsync(subjectId, cancellationToken);
            return Ok(response);
        }
    }
}
