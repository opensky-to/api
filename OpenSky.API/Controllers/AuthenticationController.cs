// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationController.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;

    using MimeKit;

    using OpenSky.API.DbModel;
    using OpenSky.API.Helpers;
    using OpenSky.API.Model;

    using Newtonsoft.Json.Linq;

    using OpenSky.API.Model.Authentication;
    using OpenSky.API.Services;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Authentication controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 05/05/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize]
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
        /// The send mail.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly SendMail sendMail;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The application configuration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly IConfiguration configuration;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The Google reCAPTCHAv3 service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly GoogleRecaptchaV3Service googleRecaptchaV3Service;

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
        /// <param name="sendMail">
        /// The send mail.
        /// </param>
        /// <param name="configuration">
        /// The application configuration.
        /// </param>
        /// <param name="googleRecaptchaV3Service">
        /// The Google reCAPTCHAv3 service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AuthenticationController(ILogger<AuthenticationController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager, RoleManager<IdentityRole> roleManager, SendMail sendMail, IConfiguration configuration, GoogleRecaptchaV3Service googleRecaptchaV3Service)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.sendMail = sendMail;
            this.configuration = configuration;
            this.googleRecaptchaV3Service = googleRecaptchaV3Service;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Register new OpenSky user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="registerUser">
        /// The register user.
        /// </param>
        /// <returns>
        /// An IActionResult object returning an ApiResponse object in the body.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> Register([FromBody] RegisterUser registerUser)
        {
            this.logger.LogInformation($"Processing new user registration for {registerUser.Username}, {registerUser.Email}");

            // Check if values are ok
            if (!Regex.IsMatch(registerUser.Username, @"^[ A-Za-z0-9.\-_]+$"))
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> { Message = "Username can only contain A-Z, 0-9 and the .-_ special characters!", IsError = true });
            }

            var userExists = await this.userManager.FindByNameAsync(registerUser.Username);
            if (userExists != null)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> { Message = "A user with this name already exists!", IsError = true });
            }

            userExists = await this.userManager.FindByEmailAsync(registerUser.Email);
            if (userExists != null)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> { Message = "A user with this email address already exists!", IsError = true });
            }

            // Check Google reCAPTCHAv3
            var reCAPTCHARequest = new ReCaptchaRequest(this.configuration["GoogleReCaptchaV3:ApiUrl"], this.configuration["GoogleReCaptchaV3:Secret"], registerUser.RecaptchaToken, this.Request.HttpContext.Connection.RemoteIpAddress?.ToString());
            var reCAPTCHAResponse = await this.googleRecaptchaV3Service.Execute(reCAPTCHARequest);
            if (!reCAPTCHAResponse.Success)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> { Message = "reCAPTCHA validation failed.", IsError = true });
            }

            // Create user
            var user = new OpenSkyUser
            {
                UserName = registerUser.Username,
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                RegisteredOn = DateTime.Now
            };

            var createResult = await this.userManager.CreateAsync(user, registerUser.Password);
            if (!createResult.Succeeded)
            {
                var errorDetails = createResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> { Message = $"User creation failed!{errorDetails}", IsError = true });
            }

            // Send email validation
            var emailBody = await System.IO.File.ReadAllTextAsync("EmailTemplates/ValidateEmail.html");

            // todo replace this with a config setting pointing at a page in the website project
            var emailToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
            var validationUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}{this.Url.Action("ValidateEmail", new { registerUser.Email, Token = emailToken.Base64Encode() })}";
            emailBody = emailBody.Replace("{UserName}", registerUser.Username);
            emailBody = emailBody.Replace("{ValidationUrl}", validationUrl);
            emailBody = emailBody.Replace("{LogoUrl}", this.configuration["OpenSky:LogoUrl"]);
            emailBody = emailBody.Replace("{DiscordUrl}", this.configuration["OpenSky:DiscordInviteUrl"]);
            this.sendMail.SendEmail(this.configuration["Email:FromAddress"], registerUser.Email, null, null, "OpenSky Account Email Validation", emailBody, true, MessagePriority.Normal);

            return this.Ok(new ApiResponse<string>("Your OpenSky user was created successfully, please check your inbox for a validation email we just sent you."));
        }

        [HttpPost]
        [Route("validateEmail")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> ValidateEmail([FromBody] ValidateEmail validateEmail)
        {
            this.logger.LogInformation($"Processing email validation for token {validateEmail.Token.Base64Decode()}");
            var user = await this.userManager.FindByEmailAsync(validateEmail.Email);
            if (user == null)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> { Message = "No user with specified email address exists!", IsError = true });
            }

            var confirmResult = await this.userManager.ConfirmEmailAsync(user, validateEmail.Token.Base64Decode());
            if (!confirmResult.Succeeded)
            {
                var errorDetails = confirmResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> { Message = $"Error validation email address!{errorDetails}", IsError = true });
            }

            // Add newly validated user to "User" role
            if (!await this.roleManager.RoleExistsAsync(UserRoles.User))
            {
                var roleResult = await this.roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                if (!roleResult.Succeeded)
                {
                    var errorDetails = roleResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                    return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> { Message = $"Error creating User role!{errorDetails}", IsError = true });
                }
            }

            var addToRoleResult = await this.userManager.AddToRoleAsync(user, UserRoles.User);
            if (!addToRoleResult.Succeeded)
            {
                var errorDetails = addToRoleResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                return this.StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string> { Message = $"Error adding user to \"User\" role!{errorDetails}", IsError = true });
            }

            return this.Ok(new ApiResponse<string>("Thank you for verifying for OpenSky user email address, your registration is now complete."));
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] Login login)
        {
            var user = await this.userManager.FindByNameAsync(login.Username) ?? await this.userManager.FindByEmailAsync(login.Username);
            if (user != null && await this.userManager.CheckPasswordAsync(user, login.Password))
            {
                if (!user.EmailConfirmed)
                {
                    return this.Ok(new ApiResponse<LoginResponse> { Message = "Please validate your email address first!", IsError = true, Data = new LoginResponse()});
                }

                var userRoles = await this.userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.Email, user.Email),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    this.configuration["JWT:ValidIssuer"],
                    this.configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha512)
                );

                user.LastLogin = DateTime.Now;
                user.LastLoginIP = this.Request.HttpContext.Connection.RemoteIpAddress?.ToString();

                // Try to geo-locate the IP if possible, todo turn this into a service, more scalable that way
                if (!string.IsNullOrEmpty(user.LastLoginIP))
                {
                    try
                    {
                        using var client = new WebClient { Headers = { ["User-Agent"] = "keycdn-tools:https://api.opensky.to" } };
                        var json = client.DownloadString($"https://tools.keycdn.com/geo.json?host={user.LastLoginIP}");
                        var geo = JObject.Parse(json);
                        var countryName = (string)geo["data"]?["geo"]?["country_name"];
                        var countryCode = (string)geo["data"]?["geo"]?["country_code"];
                        if (!string.IsNullOrEmpty(countryName) || !string.IsNullOrEmpty(countryCode))
                        {
                            user.LastLoginGeo = $"{countryName ?? "Unknown"} ({countryCode ?? "??"})";
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogWarning(ex, $"Unable to geo-locate login IP address {user.LastLoginIP}");
                    }
                }

                var updateResult = await this.userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errorDetails = updateResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                    return this.Ok(new ApiResponse<LoginResponse> { Message = $"Error saving login history!{errorDetails}", IsError = true, Data = new LoginResponse() });
                }

                return this.Ok(new ApiResponse<LoginResponse>("Success") { Data = new LoginResponse { Token = new JwtSecurityTokenHandler().WriteToken(token), Expiration = token.ValidTo } });
            }

            return this.Ok(new ApiResponse<LoginResponse> { Message = "Invalid login!", IsError = true, Data = new LoginResponse() });
        }
    }
}