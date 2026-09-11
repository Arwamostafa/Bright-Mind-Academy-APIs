using Domain.Models;
using E_LearningPlatform.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contract;

namespace E_LearningPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ClassController : ControllerBase
    {
        private readonly IClassService newClass;
        public ClassController(IClassService _class)
        {
            newClass = _class;
        }

        [HttpGet]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> GetAllClasses(CancellationToken cancellationToken)
        {
            var response = await newClass.GetAllClassesAsync(cancellationToken);
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> GetClassById(int id, CancellationToken cancellationToken)
        {
            var result = await newClass.GetClassByIdAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("{name:alpha}")]
        //[Authorize(Roles = ("Instructor, Admin"))]
        public async Task<IActionResult> GetClassByName(string name, CancellationToken cancellationToken)
        {
            var result = await newClass.GetClassByNameAsync(name, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPost]
        //[Authorize(Roles = ("Admin"))]
        public async Task<IActionResult> PostClass([FromBody] Class addedClass, CancellationToken cancellationToken)
        {
            var result = await newClass.AddClassAsync(addedClass, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = ("Admin"))]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] Class upClass, CancellationToken cancellationToken)
        {
            var result = await newClass.UpdateClassByIdAsync(id, upClass, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("{id}")]
        //[Authorize(Roles = ("Admin"))]
        public async Task<IActionResult> DeleteClassById(int id, CancellationToken cancellationToken)
        {
            var result = await newClass.RemoveClassByIdAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

    }
}
