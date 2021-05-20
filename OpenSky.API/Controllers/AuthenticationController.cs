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
    using System.Security.Claims;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;

    using MimeKit;

    using OpenSky.API.DbModel;
    using OpenSky.API.Helpers;
    using OpenSky.API.Model;
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
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<AuthenticationController> logger;

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
        /// The user manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserManager<OpenSkyUser> userManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The geo locate IP service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly GeoLocateIPService geoLocateIPService;

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
        /// <param name="geoLocateIPService">
        /// The geo locate IP service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AuthenticationController(
            ILogger<AuthenticationController> logger,
            UserManager<OpenSkyUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SendMail sendMail,
            IConfiguration configuration,
            GoogleRecaptchaV3Service googleRecaptchaV3Service,
            GeoLocateIPService geoLocateIPService)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.sendMail = sendMail;
            this.configuration = configuration;
            this.googleRecaptchaV3Service = googleRecaptchaV3Service;
            this.geoLocateIPService = geoLocateIPService;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Change OpenSky user password.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/05/2021.
        /// </remarks>
        /// <param name="changePassword">
        /// The change password model.
        /// </param>
        /// <returns>
        /// An IActionResult object returning an ApiResponse object in the body.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [Authorize(Roles = UserRoles.User)]
        [HttpPost]
        [Route("changePassword")]
        public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePassword changePassword)
        {
            this.logger.LogInformation($"Changing password for user {this.User.Identity?.Name}");

            var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
            if (user == null)
            {
                return this.Ok(new ApiResponse<LoginResponse> { Message = "Unable to find user record!", IsError = true });
            }

            var changeResult = await this.userManager.ChangePasswordAsync(user, changePassword.Password, changePassword.NewPassword);
            if (!changeResult.Succeeded)
            {
                var errorDetails = changeResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                return this.Ok(new ApiResponse<string> { Message = $"Error changing password!{errorDetails}", IsError = true });
            }

            return this.Ok(new ApiResponse<string>("Your password was changed."));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Forgot password request from OpenSky user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/05/2021.
        /// </remarks>
        /// <param name="forgotPassword">
        /// The forgot password model.
        /// </param>
        /// <returns>
        /// An IActionResult object returning an ApiResponse object in the body.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("forgotPassword")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPassword forgotPassword)
        {
            this.logger.LogInformation($"Processing forgot password request for {forgotPassword.Email}");

            // Check Google reCAPTCHAv3
            if (bool.Parse(this.configuration["GoogleReCaptchaV3:Enabled"]))
            {
                var reCAPTCHARequest = new ReCaptchaRequest(this.configuration["GoogleReCaptchaV3:ApiUrl"], this.configuration["GoogleReCaptchaV3:Secret"], forgotPassword.RecaptchaToken, this.GetRemoteIPAddress());
                var reCAPTCHAResponse = await this.googleRecaptchaV3Service.Execute(reCAPTCHARequest);
                if (!reCAPTCHAResponse.Success)
                {
                    return this.Ok(new ApiResponse<string> { Message = "reCAPTCHA validation failed.", IsError = true });
                }

                if (reCAPTCHAResponse.Score <= 0.3)
                {
                    return this.Ok(new ApiResponse<string> { Message = "reCAPTCHA v3 score too low, are you a bot?", IsError = true });
                }
            }

            var user = await this.userManager.FindByEmailAsync(forgotPassword.Email);
            if (user == null)
            {
                return this.Ok(new ApiResponse<string> { Message = "No user with specified email address exists!", IsError = true });
            }

            if (!await this.userManager.IsEmailConfirmedAsync(user))
            {
                return this.Ok(new ApiResponse<string> { Message = "You need to verify your email address first!" });
            }

            // Send password reset email
            var emailBody = await System.IO.File.ReadAllTextAsync("EmailTemplates/ResetPassword.html");
            var passwordToken = await this.userManager.GeneratePasswordResetTokenAsync(user);

            var resetPasswordUrl = $"{this.configuration["OpenSky:ResetPasswordUrl"]}?email={HttpUtility.UrlEncode(forgotPassword.Email)}&token={HttpUtility.UrlEncode(passwordToken.Base64Encode())}";
            emailBody = emailBody.Replace("{UserName}", user.UserName);
            emailBody = emailBody.Replace("{ResetPasswordUrl}", resetPasswordUrl);
            emailBody = emailBody.Replace("{LogoUrl}", this.configuration["OpenSky:LogoUrl"]);
            emailBody = emailBody.Replace("{DiscordUrl}", this.configuration["OpenSky:DiscordInviteUrl"]);

            //// ReSharper disable once AssignNullToNotNullAttribute
            this.sendMail.SendEmail(this.configuration["Email:FromAddress"], forgotPassword.Email, null, null, "OpenSky Account Password Reset", emailBody, true, MessagePriority.Normal);

            return this.Ok(new ApiResponse<string>("Please check your inbox for the password reset email we just sent you."));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Login to OpenSky API.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/05/2021.
        /// </remarks>
        /// <param name="login">
        /// The login model.
        /// </param>
        /// <returns>
        /// An IActionResult object returning an ApiResponse object in the body.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] Login login)
        {
            var user = await this.userManager.FindByNameAsync(login.Username) ?? await this.userManager.FindByEmailAsync(login.Username);
            if (user != null && await this.userManager.CheckPasswordAsync(user, login.Password))
            {
                // Check Google reCAPTCHAv3
                if (bool.Parse(this.configuration["GoogleReCaptchaV3:Enabled"]))
                {
                    var reCAPTCHARequest = new ReCaptchaRequest(this.configuration["GoogleReCaptchaV3:ApiUrl"], this.configuration["GoogleReCaptchaV3:Secret"], login.RecaptchaToken, this.GetRemoteIPAddress());
                    var reCAPTCHAResponse = await this.googleRecaptchaV3Service.Execute(reCAPTCHARequest);
                    if (!reCAPTCHAResponse.Success)
                    {
                        return this.Ok(new ApiResponse<LoginResponse> { Message = "reCAPTCHA validation failed.", IsError = true, Data = new LoginResponse() });
                    }

                    if (reCAPTCHAResponse.Score <= 0.3)
                    {
                        return this.Ok(new ApiResponse<LoginResponse> { Message = "reCAPTCHA v3 score too low, are you a bot?", IsError = true, Data = new LoginResponse() });
                    }
                }

                // If the email address isn't confirmed, deny the login
                if (!user.EmailConfirmed)
                {
                    return this.Ok(new ApiResponse<LoginResponse> { Message = "Please validate your email address first!", IsError = true, Data = new LoginResponse() });
                }

                // Fetch the user's roles
                var userRoles = await this.userManager.GetRolesAsync(user);
                
                // Check if this user is a global admin (from the config json file)
                var globalAdmins = this.configuration["OpenSky:GlobalAdmins"].Split(',');
                if (globalAdmins.Contains(user.Email) && !userRoles.Contains(UserRoles.Admin))
                {
                    userRoles.Add(UserRoles.Admin);

                    // Make sure the admin role exists
                    if (!await this.roleManager.RoleExistsAsync(UserRoles.Admin))
                    {
                        var roleResult = await this.roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                        if (!roleResult.Succeeded)
                        {
                            var errorDetails = roleResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                            return this.Ok(new ApiResponse<string> { Message = $"Error creating Admin role!{errorDetails}", IsError = true });
                        }
                    }

                    // Add the global admin user to "Admin" role
                    if (!await this.userManager.IsInRoleAsync(user, UserRoles.Admin))
                    {
                        var addToRoleResult = await this.userManager.AddToRoleAsync(user, UserRoles.Admin);
                        if (!addToRoleResult.Succeeded)
                        {
                            var errorDetails = addToRoleResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                            return this.Ok(new ApiResponse<string> { Message = $"Error adding user to \"Admin\" role!{errorDetails}", IsError = true });
                        }
                    }
                }

                // Build claims
                var authClaims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.UserName),
                    new(ClaimTypes.Email, user.Email),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                authClaims.AddRange(userRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

                // Create JWT token
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    this.configuration["JWT:ValidIssuer"],
                    this.configuration["JWT:ValidAudience"],
                    expires: DateTime.UtcNow.AddHours(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha512)
                );

                // Record last login details and save them
                user.LastLogin = DateTime.UtcNow;
                user.LastLoginIP = this.GetRemoteIPAddress();
                user.LastLoginGeo = await this.geoLocateIPService.Execute(this.GetRemoteIPAddress());

                var updateResult = await this.userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errorDetails = updateResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                    return this.Ok(new ApiResponse<LoginResponse> { Message = $"Error saving login history!{errorDetails}", IsError = true, Data = new LoginResponse() });
                }

                // All done, return the token to the client
                return this.Ok(new ApiResponse<LoginResponse>("Logged in successfully!") { Data = new LoginResponse { Token = new JwtSecurityTokenHandler().WriteToken(token), Expiration = token.ValidTo, Username = user.UserName } });
            }

            if (user != null)
            {
                await this.userManager.AccessFailedAsync(user);
            }

            return this.Ok(new ApiResponse<LoginResponse> { Message = "Invalid login!", IsError = true, Data = new LoginResponse() });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Register new OpenSky user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="registerUser">
        /// The register user model.
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
                return this.Ok(new ApiResponse<string> { Message = "Username can only contain A-Z, 0-9 and the .-_ special characters!", IsError = true });
            }

            // Check Google reCAPTCHAv3
            if (bool.Parse(this.configuration["GoogleReCaptchaV3:Enabled"]))
            {
                var reCAPTCHARequest = new ReCaptchaRequest(this.configuration["GoogleReCaptchaV3:ApiUrl"], this.configuration["GoogleReCaptchaV3:Secret"], registerUser.RecaptchaToken, this.GetRemoteIPAddress());
                var reCAPTCHAResponse = await this.googleRecaptchaV3Service.Execute(reCAPTCHARequest);
                if (!reCAPTCHAResponse.Success)
                {
                    return this.Ok(new ApiResponse<string> { Message = "reCAPTCHA validation failed.", IsError = true });
                }

                if (reCAPTCHAResponse.Score <= 0.3)
                {
                    return this.Ok(new ApiResponse<string> { Message = "reCAPTCHA v3 score too low, are you a bot?", IsError = true });
                }
            }

            // Check if user with name/email already exists
            var userExists = await this.userManager.FindByNameAsync(registerUser.Username);
            if (userExists != null)
            {
                return this.Ok(new ApiResponse<string> { Message = "A user with this name already exists!", IsError = true });
            }

            userExists = await this.userManager.FindByEmailAsync(registerUser.Email);
            if (userExists != null)
            {
                return this.Ok(new ApiResponse<string> { Message = "A user with this email address already exists!", IsError = true });
            }

            // Create user
            var user = new OpenSkyUser
            {
                UserName = registerUser.Username,
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                RegisteredOn = DateTime.UtcNow
            };

            var createResult = await this.userManager.CreateAsync(user, registerUser.Password);
            if (!createResult.Succeeded)
            {
                var errorDetails = createResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                return this.Ok(new ApiResponse<string> { Message = $"User creation failed!{errorDetails}", IsError = true });
            }

            // Send email validation
            var emailBody = await System.IO.File.ReadAllTextAsync("EmailTemplates/ValidateEmail.html");
            var emailToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
            var validationUrl = $"{this.configuration["OpenSky:ValidateEmailUrl"]}?email={HttpUtility.UrlEncode(registerUser.Email)}&token={HttpUtility.UrlEncode(emailToken.Base64Encode())}";
            emailBody = emailBody.Replace("{UserName}", registerUser.Username);
            emailBody = emailBody.Replace("{ValidationUrl}", validationUrl);
            emailBody = emailBody.Replace("{LogoUrl}", this.configuration["OpenSky:LogoUrl"]);
            emailBody = emailBody.Replace("{DiscordUrl}", this.configuration["OpenSky:DiscordInviteUrl"]);

            // ReSharper disable once AssignNullToNotNullAttribute
            this.sendMail.SendEmail(this.configuration["Email:FromAddress"], registerUser.Email, null, null, "OpenSky Account Email Validation", emailBody, true, MessagePriority.Normal);

            return this.Ok(new ApiResponse<string>("Your OpenSky user was created successfully, please check your inbox for a validation email we just sent you."));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Re-send the email validation.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/05/2021.
        /// </remarks>
        /// <param name="resendValidationEmail">
        /// The resend validation email model.
        /// </param>
        /// <returns>
        /// An IActionResult object returning an ApiResponse object in the body.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("resendValidationEmail")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> ResendValidationEmail([FromBody] ResendValidationEmail resendValidationEmail)
        {
            this.logger.LogInformation($"Sending new validation email to {resendValidationEmail.Email}");

            // Check Google reCAPTCHAv3
            if (bool.Parse(this.configuration["GoogleReCaptchaV3:Enabled"]))
            {
                var reCAPTCHARequest = new ReCaptchaRequest(this.configuration["GoogleReCaptchaV3:ApiUrl"], this.configuration["GoogleReCaptchaV3:Secret"], resendValidationEmail.RecaptchaToken, this.GetRemoteIPAddress());
                var reCAPTCHAResponse = await this.googleRecaptchaV3Service.Execute(reCAPTCHARequest);
                if (!reCAPTCHAResponse.Success)
                {
                    return this.Ok(new ApiResponse<string> { Message = "reCAPTCHA validation failed.", IsError = true });
                }

                if (reCAPTCHAResponse.Score <= 0.3)
                {
                    return this.Ok(new ApiResponse<string> { Message = "reCAPTCHA v3 score too low, are you a bot?", IsError = true });
                }
            }

            var user = await this.userManager.FindByEmailAsync(resendValidationEmail.Email);
            if (user == null)
            {
                return this.Ok(new ApiResponse<string> { Message = "No user with specified email address exists!", IsError = true });
            }

            if (await this.userManager.IsEmailConfirmedAsync(user))
            {
                return this.Ok(new ApiResponse<string> { Message = "You already verified your email address!" });
            }

            // Send email validation
            var emailBody = await System.IO.File.ReadAllTextAsync("EmailTemplates/ValidateEmail.html");
            var emailToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
            var validationUrl = $"{this.configuration["OpenSky:ValidateEmailUrl"]}?email={HttpUtility.UrlEncode(resendValidationEmail.Email)}&token={HttpUtility.UrlEncode(emailToken.Base64Encode())}";
            emailBody = emailBody.Replace("{UserName}", user.UserName);
            emailBody = emailBody.Replace("{ValidationUrl}", validationUrl);
            emailBody = emailBody.Replace("{LogoUrl}", this.configuration["OpenSky:LogoUrl"]);
            emailBody = emailBody.Replace("{DiscordUrl}", this.configuration["OpenSky:DiscordInviteUrl"]);

            // ReSharper disable once AssignNullToNotNullAttribute
            this.sendMail.SendEmail(this.configuration["Email:FromAddress"], resendValidationEmail.Email, null, null, "OpenSky Account Email Validation", emailBody, true, MessagePriority.Normal);

            return this.Ok(new ApiResponse<string>("Please check your inbox for the new validation email we just sent you."));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Reset OpenSky user password.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/05/2021.
        /// </remarks>
        /// <param name="resetPassword">
        /// The reset password model.
        /// </param>
        /// <returns>
        /// An IActionResult object returning an ApiResponse object in the body.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("resetPassword")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> ResetPassword([FromBody] ResetPassword resetPassword)
        {
            this.logger.LogInformation($"Processing password reset for {resetPassword.Email} with token {resetPassword.Token.Base64Decode()}");

            // Check Google reCAPTCHAv3
            if (bool.Parse(this.configuration["GoogleReCaptchaV3:Enabled"]))
            {
                var reCAPTCHARequest = new ReCaptchaRequest(this.configuration["GoogleReCaptchaV3:ApiUrl"], this.configuration["GoogleReCaptchaV3:Secret"], resetPassword.RecaptchaToken, this.GetRemoteIPAddress());
                var reCAPTCHAResponse = await this.googleRecaptchaV3Service.Execute(reCAPTCHARequest);
                if (!reCAPTCHAResponse.Success)
                {
                    return this.Ok(new ApiResponse<LoginResponse> { Message = "reCAPTCHA validation failed.", IsError = true, Data = new LoginResponse() });
                }

                if (reCAPTCHAResponse.Score <= 0.3)
                {
                    return this.Ok(new ApiResponse<LoginResponse> { Message = "reCAPTCHA v3 score too low, are you a bot?", IsError = true, Data = new LoginResponse() });
                }
            }

            // Find the user
            var user = await this.userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
            {
                return this.Ok(new ApiResponse<LoginResponse> { Message = "No user with specified email address exists!", IsError = true, Data = new LoginResponse() });
            }

            // Reset the user's password
            var resetResult = await this.userManager.ResetPasswordAsync(user, resetPassword.Token.Base64Decode(), resetPassword.Password);
            if (!resetResult.Succeeded)
            {
                var errorDetails = resetResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                return this.Ok(new ApiResponse<LoginResponse> { Message = $"Error resetting password!{errorDetails}", IsError = true, Data = new LoginResponse() });
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
                expires: DateTime.UtcNow.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha512)
            );

            user.LastLogin = DateTime.UtcNow;
            user.LastLoginIP = this.GetRemoteIPAddress();
            user.LastLoginGeo = await this.geoLocateIPService.Execute(this.GetRemoteIPAddress());

            var updateResult = await this.userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errorDetails = updateResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                return this.Ok(new ApiResponse<LoginResponse> { Message = $"Error saving login history!{errorDetails}", IsError = true, Data = new LoginResponse() });
            }

            return this.Ok(new ApiResponse<LoginResponse>("Success") { Data = new LoginResponse { Token = new JwtSecurityTokenHandler().WriteToken(token), Expiration = token.ValidTo, Username = user.UserName } });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Validate email address of previously registered OpenSky user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 08/05/2021.
        /// </remarks>
        /// <param name="validateEmail">
        /// The validate email model.
        /// </param>
        /// <returns>
        /// An IActionResult object returning an ApiResponse object in the body.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("validateEmail")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<string>>> ValidateEmail([FromBody] ValidateEmail validateEmail)
        {
            this.logger.LogInformation($"Processing email validation for {validateEmail.Email} with token {validateEmail.Token.Base64Decode()}");

            // Check Google reCAPTCHAv3
            if (bool.Parse(this.configuration["GoogleReCaptchaV3:Enabled"]))
            {
                var reCAPTCHARequest = new ReCaptchaRequest(this.configuration["GoogleReCaptchaV3:ApiUrl"], this.configuration["GoogleReCaptchaV3:Secret"], validateEmail.RecaptchaToken, this.GetRemoteIPAddress());
                var reCAPTCHAResponse = await this.googleRecaptchaV3Service.Execute(reCAPTCHARequest);
                if (!reCAPTCHAResponse.Success)
                {
                    return this.Ok(new ApiResponse<string> { Message = "reCAPTCHA validation failed.", IsError = true });
                }

                if (reCAPTCHAResponse.Score <= 0.3)
                {
                    return this.Ok(new ApiResponse<string> { Message = "reCAPTCHA v3 score too low, are you a bot?", IsError = true });
                }
            }

            // Find the user
            var user = await this.userManager.FindByEmailAsync(validateEmail.Email);
            if (user == null)
            {
                return this.Ok(new ApiResponse<string> { Message = "No user with specified email address exists!", IsError = true });
            }

            // Check if email isn't already verified
            if (await this.userManager.IsEmailConfirmedAsync(user))
            {
                return this.Ok(new ApiResponse<string> { Message = "You already verified your email address!" });
            }

            // Verify the token is valid
            var confirmResult = await this.userManager.ConfirmEmailAsync(user, validateEmail.Token.Base64Decode());
            if (!confirmResult.Succeeded)
            {
                var errorDetails = confirmResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                return this.Ok(new ApiResponse<string> { Message = $"Error validation email address!{errorDetails}", IsError = true });
            }

            // Make sure the user role exists
            if (!await this.roleManager.RoleExistsAsync(UserRoles.User))
            {
                var roleResult = await this.roleManager.CreateAsync(new IdentityRole(UserRoles.User));
                if (!roleResult.Succeeded)
                {
                    var errorDetails = roleResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                    return this.Ok(new ApiResponse<string> { Message = $"Error creating User role!{errorDetails}", IsError = true });
                }
            }

            // Add newly validated user to "User" role
            if (!await this.userManager.IsInRoleAsync(user, UserRoles.User))
            {
                var addToRoleResult = await this.userManager.AddToRoleAsync(user, UserRoles.User);
                if (!addToRoleResult.Succeeded)
                {
                    var errorDetails = addToRoleResult.Errors.Aggregate(string.Empty, (current, identityError) => current + $"\r\n{identityError.Description}");
                    return this.Ok(new ApiResponse<string> { Message = $"Error adding user to \"User\" role!{errorDetails}", IsError = true });
                }
            }

            return this.Ok(new ApiResponse<string>("Thank you for verifying for OpenSky user email address, your registration is now complete."));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get the remote IP address for the current request with support for reverse proxies.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/05/2021.
        /// </remarks>
        /// <returns>
        /// The remote IP address.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private string GetRemoteIPAddress()
        {
            string forwaredForIP = null;
            if (this.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedIps))
            {
                forwaredForIP = forwardedIps.First();
            }

            return forwaredForIP ?? this.Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}