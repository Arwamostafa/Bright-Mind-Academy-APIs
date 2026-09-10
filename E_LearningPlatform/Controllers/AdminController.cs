using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository;
using Domain.DTO;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;


namespace E_LearningPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly AppDbContext context;

        public AdminController(UserManager<ApplicationUser> _userManager, AppDbContext _context)
        {
            this.userManager = _userManager;
            this.context = _context;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("addingAdmin")]
        public async Task<IActionResult> AddAdmin([FromBody] AdminAddingDTO adminAddingDTO)
        {
            if (string.IsNullOrWhiteSpace(adminAddingDTO.Password))
            {
                ModelState.AddModelError("Password", "Password is required");
                return BadRequest(ModelState);
            }

            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser();
                var existingUser = await userManager.FindByEmailAsync(adminAddingDTO.Email);
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
                user.Email = adminAddingDTO.Email;
                user.PhoneNumber = adminAddingDTO.PhoneNumber;
                user.FirstName = adminAddingDTO.FirstName;
                user.LastName = adminAddingDTO.LastName;
                user.UserName = adminAddingDTO.FirstName + adminAddingDTO.LastName;


                IdentityResult result = await userManager.CreateAsync(user, adminAddingDTO.Password);



                if (result.Succeeded)
                {
                    AdminProfile admin = new AdminProfile();


                    admin.UserId = user.Id;
                    admin.NationalId = adminAddingDTO.NationalId;

                    context.Add(admin);
                    context.SaveChanges();

                    await userManager.AddToRoleAsync(user, "Admin");


                    return Ok(new { message = "Admin created successfully" });
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult GetAllAdmins()
        {

            List<AdminAddingDTO> adminAddingDTOs = new List<AdminAddingDTO>();

            var users = userManager.Users.Include(u => u.AdminProfile).Where(u => u.AdminProfile != null).ToList();


            foreach (var user in users)
            {
                AdminAddingDTO adminAddingDTO = new AdminAddingDTO();

                adminAddingDTO.Id = user.Id;
                adminAddingDTO.FirstName = user.FirstName;
                adminAddingDTO.LastName = user.LastName;
                adminAddingDTO.fullName = user.FirstName + " " + user.LastName;
                adminAddingDTO.PhoneNumber = user.PhoneNumber;
                adminAddingDTO.Email = user.Email;
                adminAddingDTO.NationalId = user.AdminProfile.NationalId;
                adminAddingDTOs.Add(adminAddingDTO);
            }

            return Ok(adminAddingDTOs);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("id/{id:int}")]
        public async Task<IActionResult> GetAdminById(int id)
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
                    Address = user.Address
                };



                return Ok(userDTO);
            }

            return NotFound("Admin Not Found");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("fullname/{name:alpha}")]
        public async Task<IActionResult> GetAdminByFullName(string name)
        {

            var user = await userManager.FindByNameAsync(name);

            if (user != null)
            {
                return Ok(user);
            }

            return NotFound("Admin Not Found");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAdmin(int id, [FromBody] AdminAddingDTO adminAddingDTO)
        {

            var user = await userManager.Users.Include(u => u.AdminProfile)
                 .SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound("Admin Not Found");
            }
            var existingUser = await userManager.FindByEmailAsync(adminAddingDTO.Email);
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
            user.Email = adminAddingDTO.Email;
            user.PhoneNumber = adminAddingDTO.PhoneNumber;
            user.FirstName = adminAddingDTO.FirstName;
            user.LastName = adminAddingDTO.LastName;
            user.UserName = adminAddingDTO.FirstName + adminAddingDTO.LastName;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "User creation failed",
                    errors = ModelState
             .Where(kvp => kvp.Value.Errors.Count > 0)
             .ToDictionary(
                 kvp => kvp.Key,
                 kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
             )
                }); ;
            }

            if (user.AdminProfile != null)
            {
                user.AdminProfile.NationalId = adminAddingDTO.NationalId;
            }

            await context.SaveChangesAsync();

            return Ok(new { message = "Admin Updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var user = await userManager.Users.Include(u => u.AdminProfile)
                                 .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("Admin Not Found");

            if (user.AdminProfile != null)
                context.AdminProfiles.Remove(user.AdminProfile);

            var result = await userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await context.SaveChangesAsync();

            return Ok("Admin is deleted Successfully");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("count")]
        public IActionResult GetAllUsersCount()
        {
            var UsersCount = userManager.Users.Count();
            return Ok(UsersCount);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsersWithRoles")]
        public async Task<IActionResult> GetAllUsersWithRoles()
        {
            var users = userManager.Users.ToList();
            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                result.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    fullName = user.FirstName + " " + user.LastName,
                    user.Gender,


                    Roles = roles
                });
            }

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("CheckAdminEmail")]
        public async Task<IActionResult> CheckAdminEmail(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return Ok(new { IsAdmin = true });
            }
            return Ok(new { IsAdmin = false });


        }
    }
}
