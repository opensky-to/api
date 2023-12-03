// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#pragma warning disable CA1416
namespace OpenSky.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Account;
    using OpenSky.API.Model.Authentication;
    using SkiaSharp;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky account controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/07/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky database context.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly OpenSkyDbContext db;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<AccountController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The role manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly RoleManager<IdentityRole> roleManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserManager<OpenSkyUser> userManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/07/2021.
        /// </remarks>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="userManager">
        /// The user manager.
        /// </param>
        /// <param name="roleManager">
        /// The role manager.
        /// </param>
        /// <param name="db">
        /// The OpenSky database context.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AccountController(
            ILogger<AccountController> logger,
            UserManager<OpenSkyUser> userManager,
            RoleManager<IdentityRole> roleManager,
            OpenSkyDbContext db)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.db = db;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user's account overview including profile image.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/07/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the account overview.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("accountOverview", Name = "GetAccountOverview")]
        public async Task<ActionResult<ApiResponse<AccountOverview>>> GetAccountOverview()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Account/accountOverview");

                // ReSharper disable once AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<AccountOverview> { Message = "Unable to find user record!", IsError = true, Data = new AccountOverview() };
                }

                var accountOverview = new AccountOverview
                {
                    Name = user.UserName,
                    Joined = user.RegisteredOn,
                    ProfileImage = user.ProfileImage,
                    TokenRenewalCountryVerification = user.TokenRenewalCountryVerification
                };

                if (user.Airline != null)
                {
                    accountOverview.AirlineName = user.Airline.Name;
                }

                return new ApiResponse<AccountOverview>(accountOverview);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Account/accountOverview");
                return new ApiResponse<AccountOverview>(ex) { Data = new AccountOverview() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the user's linked accounts.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/07/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the linked accounts.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("linkedAccounts", Name = "GetLinkedAccounts")]
        public async Task<ActionResult<ApiResponse<LinkedAccounts>>> GetLinkedAccounts()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Account/linkedAccounts");

                // ReSharper disable once AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<LinkedAccounts> { Message = "Unable to find user record!", IsError = true, Data = new LinkedAccounts() };
                }

                var linkedAccounts = new LinkedAccounts
                {
                    BingMapsKey = user.BingMapsKey,
                    SimbriefUsername = user.SimbriefUsername,
                    VatsimID = user.VatsimID
                };

                return new ApiResponse<LinkedAccounts>(linkedAccounts);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Account/linkedAccounts");
                return new ApiResponse<LinkedAccounts>(ex) { Data = new LinkedAccounts() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get profile image for the specified user ID.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/11/2023.
        /// </remarks>
        /// <param name="userId">
        /// The ID of the OpenSky user.
        /// </param>
        /// <returns>
        /// The profile image.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("profileImage/{userId}", Name = "GetProfileImage")]
        public async Task<ActionResult<ApiResponse<byte[]>>> GetProfileImage(Guid userId)
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | GET Account/profileImage/{userId}");
            try
            {
                var user = await this.userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    throw new Exception("No such user ID.");
                }

                return new ApiResponse<byte[]> { Data = user.ProfileImage };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Account/profileImage/{userId}");
                return new ApiResponse<byte[]>(ex) { Data = Array.Empty<byte>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get the list of all OpenSky users.
        /// </summary>
        /// <remarks>
        /// sushi.at, 22/11/2023.
        /// </remarks>
        /// <returns>
        /// The list of all OpenSky users.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("users", Name = "GetUsers")]
        public async Task<ActionResult<ApiResponse<IEnumerable<User>>>> GetUsers()
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | GET Account/users");
            try
            {
                var users = this.db.Users.Select(
                    u => new User
                    {
                        ID = Guid.Parse(u.Id),
                        Username = u.UserName,
                        Email = u.Email,
                        EmailConfirmed = u.EmailConfirmed,
                        RegisteredOn = u.RegisteredOn,
                        LastLogin = u.LastLogin,
                        LastLoginIP = u.LastLoginIP,
                        LastLoginGeo = u.LastLoginGeo,
                        AccessFailedCount = u.AccessFailedCount,
                        Roles = new List<string>()
                    }).ToList();
                foreach (var user in users)
                {
                    var userManagerUser = await this.userManager.FindByIdAsync(user.ID.ToString());
                    if (userManagerUser != null)
                    {
                        var roles = await this.userManager.GetRolesAsync(userManagerUser);
                        user.Roles.AddRange(roles);
                    }
                }

                return new ApiResponse<IEnumerable<User>> { Data = users };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Account/users");
                return new ApiResponse<IEnumerable<User>>(ex) { Data = new List<User>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds or removes the moderator role from the specified user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 21/11/2023.
        /// </remarks>
        /// <param name="moderatorRole">
        /// The moderator role model.
        /// </param>
        /// <returns>
        /// An IActionResult object returning an ApiResponse object in the body.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("moderatorRole", Name = "SetModeratorRole")]
        public async Task<ActionResult<ApiResponse<string>>> SetModeratorRole([FromBody] ModeratorRole moderatorRole)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Account/moderatorRole");
                var user = await this.userManager.FindByNameAsync(moderatorRole.Username) ?? await this.userManager.FindByEmailAsync(moderatorRole.Username);

                if (user == null)
                {
                    throw new Exception($"Unable to find user \"{moderatorRole.Username}\".");
                }

                // Make sure the moderator role exists
                if (!await this.roleManager.RoleExistsAsync(UserRoles.Moderator))
                {
                    var roleResult = await this.roleManager.CreateAsync(new IdentityRole(UserRoles.Moderator));
                    if (!roleResult.Succeeded)
                    {
                        var roleErrorDetails = roleResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                        return new ApiResponse<string> { Message = $"Error creating User role!{roleErrorDetails}", IsError = true };
                    }
                }

                var result = moderatorRole.IsModerator ? await this.userManager.AddToRoleAsync(user, UserRoles.Moderator) : await this.userManager.RemoveFromRoleAsync(user, UserRoles.Moderator);

                if (result.Succeeded)
                {
                    return new ApiResponse<string>(moderatorRole.IsModerator ? $"User \"{moderatorRole.Username}\" is now a moderator!" : $"User \"{moderatorRole.Username}\" is no longer a moderator!");
                }

                var errorDetails = result.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                return new ApiResponse<string> { Message = $"User role modification failed!{errorDetails}", IsError = true };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Account/moderatorRole");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the token renewal country verification.
        /// </summary>
        /// <remarks>
        /// sushi.at, 30/11/2021.
        /// </remarks>
        /// <param name="enableVerification">
        /// True to enable, false to disable the verification.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut("tokenRenewalCountryVerification/{enableVerification:bool}", Name = "SetTokenRenewalCountryVerification")]
        public async Task<ActionResult<ApiResponse<string>>> SetTokenRenewalCountryVerification(bool enableVerification)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT Account/tokenRenewalCountryVerification/{enableVerification}");

                // ReSharper disable once AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                user.TokenRenewalCountryVerification = enableVerification;
                await this.userManager.UpdateAsync(user);
                return new ApiResponse<string>("Successfully updated token renewal country verification setting.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | PUT Account/tokenRenewalCountryVerification/{enableVerification}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates user's linked accounts.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/07/2021.
        /// </remarks>
        /// <param name="linkedAccounts">
        /// The linked accounts.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut("linkedAccounts", Name = "UpdateLinkedAccounts")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateLinkedAccounts([FromBody] LinkedAccounts linkedAccounts)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT Account/linkedAccounts");

                // ReSharper disable once AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                user.BingMapsKey = linkedAccounts.BingMapsKey;
                user.SimbriefUsername = linkedAccounts.SimbriefUsername;
                user.VatsimID = linkedAccounts.VatsimID;

                await this.userManager.UpdateAsync(user);
                return new ApiResponse<string>("Successfully updated linked accounts and keys.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | PUT Account/linkedAccounts");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Upload profile image.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/07/2021.
        /// </remarks>
        /// <param name="fileUpload">
        /// The file upload.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("profileImage", Name = "UploadProfileImage")]
        public async Task<ActionResult<ApiResponse<string>>> UploadProfileImage(IFormFile fileUpload)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Account/profileImage");

                // ReSharper disable once AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                if (fileUpload.ContentType is not "image/png" and not "image/jpeg")
                {
                    return new ApiResponse<string> { Message = "Image has to be JPG or PNG!", IsError = true };
                }

                if (fileUpload.Length > 1 * 1024 * 1024)
                {
                    return new ApiResponse<string> { Message = "Maximum image size is 1MB!", IsError = true };
                }

                var memoryStream = new MemoryStream();
                await fileUpload.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var image = SKBitmap.Decode(memoryStream);
                if (image.Width > 300 || image.Height > 300)
                {
                    image = image.Resize(new SKSizeI(300, 300), SKFilterQuality.High);
                    memoryStream = new MemoryStream();
                    image.Encode(memoryStream, SKEncodedImageFormat.Png, 100);
                }

                user.ProfileImage = memoryStream.ToArray();
                await this.userManager.UpdateAsync(user);
                return new ApiResponse<string>("Successfully updated profile image.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Account/profileImage");
                return new ApiResponse<string>(ex);
            }
        }
    }
}