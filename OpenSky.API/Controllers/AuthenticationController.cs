// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationController.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.Model;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Authentication controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 05/05/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
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
        private readonly ILogger<AuthenticationController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserManager<OpenSkyUser> userManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The role manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly RoleManager<IdentityRole> roleManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/05/2021.
        /// </remarks>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="db">
        /// The OpenSky database context.
        /// </param>
        /// <param name="userManager">
        /// The user manager.
        /// </param>
        /// <param name="roleManager">
        /// The role manager.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AuthenticationController(ILogger<AuthenticationController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterUser registerUser)
        {
            if (!Regex.IsMatch(registerUser.Username, @"^[ A-Za-z0-9.\-_]+$"))
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "Username can only contain A-Z, 0-9 and the .-_ special characters!" });
            }

            var userExists = await this.userManager.FindByNameAsync(registerUser.Username);
            if (userExists != null)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "A user with this name already exists!" });
            }

            userExists = await this.userManager.FindByEmailAsync(registerUser.Email);
            if (userExists != null)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "A user with this email address already exists!" });
            }

            var user = new OpenSkyUser
            {
                UserName = registerUser.Username,
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createResult = await this.userManager.CreateAsync(user, registerUser.Password);
            if (!createResult.Succeeded)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse { Message = "User creation failed! Please check user details and try again." });
            }

            // todo send email confirmation

            return this.Ok(new ApiResponse("Your OpenSky user was created successfully, please check your inbox for a confirmation email we just sent you."));
        }
    }
}