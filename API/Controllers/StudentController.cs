using Domain.Common;
using Domain.DTO;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Repositories;
namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUnitOfWork unitOfWork;

        public StudentController(UserManager<ApplicationUser> _userManager, IUnitOfWork unitOfWork)
        {
            this.userManager = _userManager;
            this.unitOfWork = unitOfWork;
        }

        [HttpPost("studentRegister")]
        public async Task<IActionResult> Register([FromBody] StudentRegisterDTO studentRegisterDTO, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser();

                user.Email = studentRegisterDTO.Email;
                user.PhoneNumber = studentRegisterDTO.PhoneNumber;
                user.Address = studentRegisterDTO.Address;
                user.FirstName = studentRegisterDTO.FirstName;
                user.LastName = studentRegisterDTO.LastName;
                user.Gender = studentRegisterDTO.Gender;
                user.UserName = studentRegisterDTO.FirstName + studentRegisterDTO.LastName;

                IdentityResult result = await userManager.CreateAsync(user, studentRegisterDTO.Password);

                if (result.Succeeded)
                {
                    StudentProfile student = new StudentProfile();

                    student.UserId = user.Id;
                    student.Age = studentRegisterDTO.Age;

                    await unitOfWork.Repository<StudentProfile>().AddAsync(student, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    await userManager.AddToRoleAsync(user, "Student");


                    return Ok("Created");
                }

                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("Password", item.Description);
                }

            }

            return BadRequest(ModelState);
        }

        [HttpGet("GetAllStudents")]
        public IActionResult GetStudents()
        {

            List<StudentRegisterDTO> studentRegisterDTOs = new List<StudentRegisterDTO>();

            var users = userManager.Users.Include(u => u.StudentProfile).Where(u => u.StudentProfile != null).ToList();

            foreach (var user in users)
            {
                StudentRegisterDTO studentRegisterDTO = new StudentRegisterDTO();

                studentRegisterDTO.Id = user.Id;
                studentRegisterDTO.FirstName = user.FirstName;
                studentRegisterDTO.LastName = user.LastName;
                studentRegisterDTO.Address = user.Address;
                studentRegisterDTO.PhoneNumber = user.PhoneNumber;
                studentRegisterDTO.Email = user.Email;
                studentRegisterDTO.Gender = user.Gender;
                studentRegisterDTO.Age = user.StudentProfile.Age;

                studentRegisterDTOs.Add(studentRegisterDTO);
            }

            return Ok(studentRegisterDTOs);
        }

        [HttpGet("page")]
        public async Task<IActionResult> GetPageOfStudents([FromQuery] RequestFilters requestFilters, CancellationToken cancellationToken)
        {
            var page = await unitOfWork.Repository<ApplicationUser>().GetPaginatedListAsync(
                requestFilters,
                include: q => q.Include(u => u.StudentProfile),
                predicate: u => u.StudentProfile != null,
                orderBy: q => q.OrderBy(u => u.Id),
                cancellationToken: cancellationToken);

            var items = page.Items.Select(user => new StudentRegisterDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Gender = user.Gender,
                Age = user.StudentProfile!.Age,
            }).ToList();

            return Ok(new PaginatedList<StudentRegisterDTO>(items, page.PageNumber, page.TotalCount, page.PageSize));
        }

        [HttpGet("id/{id:int}")]
        public async Task<IActionResult> GetStudentById(int id)
        {

            var user = await userManager.FindByIdAsync(id.ToString());

            if (user != null)
            {

                UserDTO userDTO = new UserDTO()
                {
                    id = user.Id,
                    fullName = user.FirstName + "" + user.LastName,
                    email = user.Email,
                    nationalId = (user.AdminProfile != null) ? user.AdminProfile.NationalId : 0,
                    phoneNumber = user.PhoneNumber,
                    gender = user.Gender,
                    Address = user.Address,
                    age = (user.StudentProfile != null) ? user.StudentProfile.Age : 0
                };



                return Ok(userDTO);
            }

            return NotFound("Student Not Found");
        }

        [HttpGet("fullname/{name:alpha}")]
        public async Task<IActionResult> GetStudentByFullName(string name)
        {

            var user = await userManager.FindByNameAsync(name);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound("Student Not Found");
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] StudentRegisterDTO studentRegisterDTO, CancellationToken cancellationToken)
        {

            var user = await userManager.Users.Include(u => u.StudentProfile)
                .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user == null)
            {
                return NotFound("Student Not Found");
            }

            user.Email = studentRegisterDTO.Email;
            user.PhoneNumber = studentRegisterDTO.PhoneNumber;
            user.Address = studentRegisterDTO.Address;
            user.FirstName = studentRegisterDTO.FirstName;
            user.LastName = studentRegisterDTO.LastName;
            user.Gender = studentRegisterDTO.Gender;
            user.UserName = studentRegisterDTO.FirstName + studentRegisterDTO.LastName;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (user.StudentProfile != null)
            {
                user.StudentProfile.Age = studentRegisterDTO.Age;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok("Student Updated Successfully");


        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> DeleteStudent(int id, CancellationToken cancellationToken)
        {
            var user = await userManager.Users.Include(u => u.StudentProfile)
                                  .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (user == null)
                return NotFound("Student Not Found");

            if (user.StudentProfile != null)
                unitOfWork.Repository<StudentProfile>().Remove(user.StudentProfile);

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok("Student is deleted Successfully");
        }


        [HttpGet("count")]
        public async Task<IActionResult> GetStudentCount(CancellationToken cancellationToken)
        {
            var count = await userManager.Users.Include(u => u.StudentProfile)
                                .CountAsync(u => u.StudentProfile != null, cancellationToken);
            return Ok(count);
        }
    }
}
