using Domain.DTO;
using E_LearningPlatform.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contract;

namespace E_LearningPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class UnitController : ControllerBase
    {
        private readonly IUnitService _unitService;

        public UnitController(IUnitService unitService)
        {
            _unitService = unitService;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<UnitDto>>> GetAllUnits(CancellationToken cancellationToken)
        {
            var units = await _unitService.GetAllAsync(cancellationToken);
            return Ok(units);
        }

        // POST: api/Unit
        [HttpPost("Create")]
        public async Task<ActionResult<UnitDto>> CreateUnit([FromBody] UnitCreateDto unitCreateDto, CancellationToken cancellationToken)
        {
            await _unitService.AddAsync(unitCreateDto, cancellationToken);

            return CreatedAtAction(nameof(GetUnitById), new { id = unitCreateDto.Id }, unitCreateDto);
        }


        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetUnitById(int id, CancellationToken cancellationToken)
        {
            var result = await _unitService.GetByIdAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }



        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateUnit(int id, [FromBody] UnitCreateDto unitCreateDto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _unitService.Update(unitCreateDto, id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteUnit(int id, CancellationToken cancellationToken)
        {
            var result = await _unitService.Delete(id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("by-lesson/{lessonId}")]
        public async Task<IActionResult> GetUnitByLessonId(int lessonId, CancellationToken cancellationToken)
        {
            var result = await _unitService.GetUnitByLessonId(lessonId, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("BySubjectId/{subjectId}")]
        public async Task<IActionResult> GetUnitsBySubjectId(int subjectId, CancellationToken cancellationToken)
        {
            var units = await _unitService.GetUnitsBySubjectId(subjectId, cancellationToken);

            if (units == null || !units.Any())
                return NotFound("No units found for this subject.");

            return Ok(units);
        }

        [HttpGet("BySubjectName/{subjectName}")]
        public async Task<IActionResult> GetUnitsBySubjectName(string subjectName, CancellationToken cancellationToken)
        {
            var units = await _unitService.GetUnitsBySubjectName(subjectName, cancellationToken);

            if (units == null || !units.Any())
                return NotFound("No units found for this subject name.");

            return Ok(units);
        }


        [HttpGet("CountBySubjectId/{subjectId}")]
        public async Task<IActionResult> GetNumberOfUnitsBySubjectId(int subjectId, CancellationToken cancellationToken)
        {
            var count = await _unitService.GetNumberOfUnitsBySubjectId(subjectId, cancellationToken);
            return Ok(new { SubjectId = subjectId, UnitCount = count });
        }

    }
}
