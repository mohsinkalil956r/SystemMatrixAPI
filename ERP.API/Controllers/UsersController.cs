using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ZATCA.API.Auth;
using ZATCA.API.Infrastructure;
using ZATCA.API.Models;
using ZATCA.API.Models.Users;
using ZATCA.BL.Helpers;
using ZATCA.DAL;
using ZATCA.DAL.DB.Entities;
using ZATCA.DAL.Repositories.Abstraction;

namespace ZATCA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<SystemUser> _userManager;
        private readonly IJWTManager _jwtManager;
        private readonly IUserRepository _userRepository;
        private readonly EmailService _emailService; // Add this
        public UsersController(UserManager<SystemUser> userManager, IJWTManager jwtManager, IUserRepository userRepository, EmailService emailService)
        {
            this._userManager = userManager;
            this._jwtManager = jwtManager;
            this._userRepository = userRepository;
            this._emailService = emailService; // Initialize the email service
        }


        [Route("authenticate")]
        [HttpPost]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateVM userData)
        {
            var user = await this._userManager.FindByNameAsync(userData.Username);

            if (user != null&& user.IsActive==true)
            {
                var isPasswordValid = await this._userManager.CheckPasswordAsync(user, userData.Password);
                if (isPasswordValid)
                {
                    var claims = new List<Claim>();
                    var userRoles = await this._userManager.GetRolesAsync(user);
                    claims.Add(new Claim("CompanyId", user.CompanyId.ToString()));
                    if (userRoles != null)
                    {
                        userRoles.ToList().ForEach(r =>
                        {
                            claims.Add(new Claim(ClaimTypes.Role, r));
                        });
                    }
              
                    claims.Add(new Claim(ClaimTypes.Name, userData.Username));
                    claims.Add(new Claim(ClaimTypes.Email, userData.Username));
                    var token = this._jwtManager.Authenticate(claims);

                    var userEntity = await this._userRepository.Get(user.Id).Include(u => u.Roles).Include(u => u.Permissions).FirstOrDefaultAsync();

                    var userDetails = new UserGetVM
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber,
                        Token = token.Token,
                        RequiredPasswordReset = user.RequiredPasswordReset,
                        Permissions = userEntity.Permissions.Select(p => p.Name).ToList(),
                        Roles = new List<string>(userRoles?.ToArray() ?? new List<string>().ToArray()),
                    };


                    return Ok(new APIResponse<UserGetVM>
                    {
                        IsError = false,
                        Message = "",
                        data = userDetails
                    });
                }
            }
            return Ok(new { IsError = true, Message = "Invalid username or password" });
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterVM model)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            var userEntity = UserRegistrationHelper.GetUserEntity(model.Email, model.FirstName, model.LastName, model.PhoneNumber, password,model.isActive,model.CompanyId);
            if (model.UserType == "Admin")
            {
                userEntity.UsZATCAermissions = model.Permissions.Select(p => new UsZATCAermission { PermissionId = (int)Enum.Parse(typeof(PERMISSIONS), p) }).ToList();
                userEntity.UserRoles = new List<UserRole> { new UserRole { RoleId = (int)ROLES.Admin } };
            }
            else
            {
                userEntity.UsZATCAermissions = model.Permissions.Select(p => new UsZATCAermission { PermissionId = (int)Enum.Parse(typeof(PERMISSIONS), p) }).ToList();
                userEntity.UserRoles = new List<UserRole> { new UserRole { RoleId = (int)ROLES.Company } };

            }

            // Optionally, you can redirect the user to a confirmation page.

             this._userRepository.Add(userEntity);
            await this._userRepository.SaveChanges();


            if (userEntity != null)
            {
                var email = model.Email;
                var url = "http://localhost:4200/login";
                var user = await this._userManager.FindByEmailAsync(email.ToUpper());
                await _userManager.UpdateSecurityStampAsync(user);
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // Create a URL for the reset password page with the token.
                var changePassword = Url.Action("Change Password", "Account", new { email = email, token = token }, protocol: HttpContext.Request.Scheme);

                // Send an email with the reset link.
                // You should have an email service set up and properly configured.
                // Here, we'll assume you have an email service class named EmailService.
                // You may need to install a package for email sending, such as SendGrid.
                var emailMessage = $":\n{url}\n\nEmail: {email}\nTemporary Password: {password}\n\n";

               

                // Send the email using the EmailService.
                await _emailService.SendPasswordResetEmail(model.Email, emailMessage);
            }

            return Created("", new { IsError = false, Message = "" });

        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Get(string? searchQuery = "", int pageNumber = 1, int pageSize = 10)
        {
            var query = this._userRepository.Get().Include(u => u.Roles).Include(u => u.Permissions).AsQueryable();
            // Apply search filter if searchQuery is provided and not null or empty
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p =>
                    p.Email.Contains(searchQuery)||p.FirstName.Contains(searchQuery)
                    || p.LastName.Contains(searchQuery) || p.PhoneNumber.Contains(searchQuery) );
            }
            var totalCount = await query.CountAsync();

            // Apply pagination
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
              var Users = await query.ToListAsync();
            var result = new List<UserGetVM>();
            foreach (var user in Users)
            {
                result.Add(new UserGetVM
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    
                    PhoneNumber = user.PhoneNumber,
                    CompanyId = user.CompanyId,
                    Roles = user.Roles.Select(x => x.Name).ToList(),
                    Permissions = user.Permissions.Select(x => x.Name).ToList()
                });;
            }
            var paginationResult = new PaginatedResult<UserGetVM>(result, totalCount);

            return Ok(new APIResponse<object>
            {
                IsError = false,
                Message = "",
                data = paginationResult
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await this._userRepository.Get(id).Include(u => u.Roles).Include(u => u.Permissions).FirstOrDefaultAsync();
            if (user != null)
            {
                var data = new UserGetVM
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    UserType = user.Roles.Select(x => x.Name).FirstOrDefault(),
                    PhoneNumber = user.PhoneNumber,
                    CompanyId = user.CompanyId,
                    Roles = user.Roles.Select(x => x.Name).ToList(),
                    Permissions = user.Permissions.Select(x => x.Name).ToList()
                };

                return Ok(new APIResponse<UserGetVM>
                {
                    IsError = false,
                    Message = "",
                    data = data
                });
            }

            return NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateVM model)
        {
            var user = await this._userRepository.Get(id).Include(a=>a.Roles).Include(r=>r.UserRoles).Include(u => u.UsZATCAermissions).FirstOrDefaultAsync();
            if (user != null)
            {
                if (model.UserType == "Admin")
                {
                    if (await this._userManager.IsInRoleAsync(user, ROLES.Company.ToString()))
                    {
                      await _userManager.RemoveFromRoleAsync(user, ROLES.Company.ToString());
                     await _userManager.AddToRoleAsync(user, ROLES.Admin.ToString());
                    }
                }
                else
                {
                    if (await this._userManager.IsInRoleAsync(user, ROLES.Admin.ToString()))
                    {
                    await    _userManager.RemoveFromRoleAsync(user, ROLES.Admin.ToString());
                    await    _userManager.AddToRoleAsync(user, ROLES.Company.ToString());
                    }
                }

              user.PhoneNumber = model.PhoneNumber;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.IsActive=model.isActive;
              
                user.CompanyId = model.CompanyId;
                user.UsZATCAermissions = model.Permissions.Select(p => new UsZATCAermission { PermissionId = (int)Enum.Parse(typeof(PERMISSIONS), p) }).ToList();

                this._userRepository.Update(user);
                await this._userRepository.SaveChanges();
                return Ok();
            }

            return NotFound();
        }


        [Route("entityexists/{data}")]
        [HttpGet]
        public async Task<bool> EntityExists(string data)
        {
            var userEntity = await this._userRepository.Get().Where(u => u.Email == data.ToLower()).FirstOrDefaultAsync();
            if (userEntity != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        [Route("resetpassword")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            var user = await this._userManager.FindByEmailAsync(User.GetUsername());
            if (user == null)
            {
                return NotFound();

            }
            var isPasswordValid = await this._userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (isPasswordValid)
            {
                var result = await this._userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
                if (result.Succeeded)
                {
                    var userEntity = this._userRepository.Get().Where(u => u.Email == User.GetUsername()).FirstOrDefault();
                    userEntity.RequiredPasswordReset = false;
                    await this._userRepository.SaveChanges();
                    return await Authenticate(new AuthenticateVM
                    {
                        Password = model.Password,
                        Username = User.GetUsername(),
                    });
                }
                else
                {
                    return Ok(new { isError = true, Message = string.Join("", result.Errors) });
                }
            }
            else
            {

                return Ok(new { isError = true, Message = "Current Password is Invalid" });
            }
        }
        [Route("changepassword")]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            var user = await this._userManager.FindByNameAsync(User.GetUsername());
            if (user == null)
            {

                return NotFound();
            }
            else
            {
                var isPasswordValid = await this._userManager.CheckPasswordAsync(user, model.CurrentPassword);
                if (isPasswordValid)
                {
                    var result = await this._userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
                    if (result.Succeeded)
                    {
                        var userEntity = this._userRepository.Get().Where(u => u.Email == User.GetUsername()).FirstOrDefault();
                        userEntity.RequiredPasswordReset = false;
                        await this._userRepository.SaveChanges();
                        return await Authenticate(new AuthenticateVM
                        {
                            Password = model.Password,
                            Username = User.GetUsername(),
                        });
                    }
                    else
                    {
                        return Ok(new { Iserror = true, Message = string.Join("", result.Errors) });
                    }
                }
                else
                {
                    return Ok(new { Iserror = true, Message = "Current Password is Invalid" });
                }

            }     
        }

        [Route("forgetpassword")]
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgotPasswordVM model)
        { //ya sary alphanumberic characterssave kr rha hy
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            //ya random password genrate kr rha hy
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            //ya email check kr rha hy jo api ko call main frontend sy ayi hy
            var user = await this._userManager.FindByEmailAsync(model.email.ToUpper());
            if (user == null)
            {

                return Ok();
            }
            else
            {
               //
                var url = "http://localhost:4200/login";
                //
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // Create a URL for the reset password page with the token.
                var changePassword = Url.Action("Change Password", "Account", new { email = model.email, token = token }, protocol: HttpContext.Request.Scheme);
                //ya par code ay ga user k exiting password ko random password sy overwrite or save krny ka

                var result = await _userManager.ResetPasswordAsync(user, token, password);

                user.RequiredPasswordReset = true;
                this._userRepository.Update(user);
                await this._userRepository.SaveChanges();
                //ya email jy gi user ko random password or email template k sath
                var emailMessage = $":\n{url}\n\nEmail: {model.email}\nTemporary Password: {password}\n\n";



                // Send the email using the EmailService.
                //yaha par email ja rhi hy send SendForgetPasswordEmail() k through
                await _emailService.SendForgetPasswordEmail(model.email, emailMessage);

                // Optionally, you can redirect the user to a confirmation page.
                return Ok(new
                {
                    Iserror = false,
                    Message = "Email Sent"
                });


            }

        }

     
        [HttpDelete("{id}")]
        public async Task<APIResponse<object>> Delete(int id)
        {
            var asset = await this._userRepository.Get(id).FirstOrDefaultAsync();
            if (asset != null)
            {
                asset.IsActive = false;
                await this._userRepository.SaveChanges();
                return new APIResponse<object>
                {
                    IsError = false,
                    Message = "",
                };
            }
            return new APIResponse<object>
            {
                IsError = false,
                Message = "",
            };
        }
    }
}
