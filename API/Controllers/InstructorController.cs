using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Repositories;
using Application.Services.Contract;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IFileService fileService;

        public InstructorController(UserManager<ApplicationUser> _userManager, IUnitOfWork unitOfWork, IFileService fileService)
        {
            this.userManager = _userManager;
            this.unitOfWork = unitOfWork;
            this.fileService = fileService;
        }

        [HttpPost("addingInstructor")]
        public async Task<IActionResult> AddInstructor([FromForm] InstructorAddingDTO instructorAddingDTO, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(instructorAddingDTO.Password))
            {
                ModelState.AddModelError("Password", "Password is required");
                return BadRequest(ModelState);
            }

            if (ModelState.IsValid)
            {
                var existingUser = await userManager.FindByEmailAsync(instructorAddingDTO.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email is already in use");
                    return BadRequest(new
                    {
                        message = "Validation failed",
                        errors = new
                        {
                            Email = new[] { "Email is already in use" }
                        }
                    });
                }

                // Validate the image before creating anything, so a bad upload doesn't
                // leave behind an Identity user with no InstructorProfile and no role.
                string? imageUrl = null;
                if (instructorAddingDTO.Image != null)
                {
                    var uploadResult = await fileService.UploadAsync(instructorAddingDTO.Image, FileCategory.Image, "Uploads/Image", cancellationToken);
                    if (uploadResult.IsFailure)
                        return BadRequest(new { message = uploadResult.Error.Message });
                    imageUrl = uploadResult.Value;
                }

                ApplicationUser user = new ApplicationUser();
                user.Email = instructorAddingDTO.Email;
                user.PhoneNumber = instructorAddingDTO.PhoneNumber;
                user.Address = instructorAddingDTO.Address;
                user.FirstName = instructorAddingDTO.FirstName;
                user.LastName = instructorAddingDTO.LastName;
                user.Gender = instructorAddingDTO.Gender;
                user.UserName = instructorAddingDTO.FirstName + instructorAddingDTO.LastName;


                IdentityResult result = await userManager.CreateAsync(user, instructorAddingDTO.Password);



                if (result.Succeeded)
                {
                    InstructorProfile instructor = new InstructorProfile
                    {
                        UserId = user.Id,
                        Image = imageUrl
                    };

                    await unitOfWork.Repository<InstructorProfile>().AddAsync(instructor, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    await userManager.AddToRoleAsync(user, "Instructor");


                    return Ok(new { message = "Instructor created successfully" });
                }

                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("Password", item.Description);
                }

            }

            return BadRequest(new
            {
                message = "User creation failed",
                errors = ModelState
              .Where(kvp => kvp.Value.Errors.Count > 0)
              .ToDictionary(
                  kvp => kvp.Key,
                  kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
              )
            });
        }

        [HttpGet]
        public IActionResult GetAllInstructors()
        {
            List<InstructorAddingDTO> instructorAddingDTOs = new List<InstructorAddingDTO>();

            var users = userManager.Users.Include(u => u.InstructorProfile).Where(u => u.InstructorProfile != null).ToList();

            foreach (var user in users)
            {
                InstructorAddingDTO instructorAddingDTO = new InstructorAddingDTO();

                instructorAddingDTO.Id = user.Id;
                instructorAddingDTO.FirstName = user.FirstName;
                instructorAddingDTO.LastName = user.LastName;
                instructorAddingDTO.Address = user.Address;
                instructorAddingDTO.PhoneNumber = user.PhoneNumber;
                instructorAddingDTO.Email = user.Email;
                instructorAddingDTO.Gender = user.Gender;
                instructorAddingDTO.ImageURL = user.InstructorProfile.Image;

                instructorAddingDTOs.Add(instructorAddingDTO);
            }

            return Ok(instructorAddingDTOs);
        }

        [HttpGet("id/{id:int}")]
        public async Task<IActionResult> GetInstructorById(int id)
        {

            var user = await userManager.FindByIdAsync(id.ToString());

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound("Instructor Not Found");
        }

        [HttpGet("fullname/{name:alpha}")]
        public async Task<IActionResult> GetInstructorByFullName(string name)
        {

            var user = await userManager.FindByNameAsync(name);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound("Instructor Not Found");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateInstructor(int id, [FromForm] InstructorAddingDTO instructorAddingDTO, CancellationToken cancellationToken)
        {

            var user = await userManager.Users.Include(u => u.InstructorProfile)
                .SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Instructor Not Found");
            }

            var existingUser = await userManager.FindByEmailAsync(instructorAddingDTO.Email);
            if (existingUser != null && existingUser.Id != id)
            {
                ModelState.AddModelError("Email", "Email is already in use");
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = new
                    {
                        Email = new[] { "Email is already in use" }
                    }
                });
            }

            // Validate the image before persisting anything, so a bad upload doesn't
            // leave the profile fields partially updated.
            string? newImageUrl = null;
            if (user.InstructorProfile != null && instructorAddingDTO.Image != null)
            {
                var uploadResult = await fileService.UploadAsync(instructorAddingDTO.Image, FileCategory.Image, "Uploads/Images", cancellationToken);
                if (uploadResult.IsFailure)
                    return BadRequest(new { message = uploadResult.Error.Message });

                newImageUrl = uploadResult.Value;
            }

            user.Email = instructorAddingDTO.Email;
            user.PhoneNumber = instructorAddingDTO.PhoneNumber;
            user.Address = instructorAddingDTO.Address;
            user.FirstName = instructorAddingDTO.FirstName;
            user.LastName = instructorAddingDTO.LastName;
            user.Gender = instructorAddingDTO.Gender;
            user.UserName = instructorAddingDTO.FirstName + instructorAddingDTO.LastName;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (newImageUrl != null)
            {
                user.InstructorProfile!.Image = newImageUrl;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { message = "Instructor Updated successfully" });


        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteInstructor(int id, CancellationToken cancellationToken)
        {
            var user = await userManager.Users.Include(u => u.InstructorProfile)
                                  .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user == null)
                return NotFound("Instructor Not Found");

            if (user.InstructorProfile != null)
                unitOfWork.Repository<InstructorProfile>().Remove(user.InstructorProfile);

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok("Instructor is deleted Successfully");
        }


        [HttpGet("count")]
        public async Task<IActionResult> GetInstructorsCount(CancellationToken cancellationToken)
        {
            var count = await userManager.Users.Include(u => u.InstructorProfile)
                .Where(u => u.InstructorProfile != null).CountAsync(cancellationToken);
            return Ok(count);
        }
    }
}