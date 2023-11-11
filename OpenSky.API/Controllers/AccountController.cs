// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#pragma warning disable CA1416
namespace OpenSky.API.Controllers
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Account;
    using OpenSky.API.Model.Authentication;

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
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<AccountController> logger;

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
        /// -------------------------------------------------------------------------------------------------
        public AccountController(ILogger<AccountController> logger, UserManager<OpenSkyUser> userManager)
        {
            this.logger = logger;
            this.userManager = userManager;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/07/2021.
        /// </remarks>
        /// <param name="image">
        /// The image to resize.
        /// </param>
        /// <param name="width">
        /// The width to resize to.
        /// </param>
        /// <param name="height">
        /// The height to resize to.
        /// </param>
        /// <returns>
        /// The resized image.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static Bitmap ResizeImage([NotNull] Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            if (image.HorizontalResolution > 0 && image.VerticalResolution > 0)
            {
                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            }

            using var graphics = Graphics.FromImage(destImage);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);

            return destImage;
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
                    SimbriefUsername = user.SimbriefUsername
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

                var image = Image.FromStream(memoryStream);
                if (image.Width > 300 || image.Height > 300)
                {
                    image = ResizeImage(image, 300, 300);
                    memoryStream = new MemoryStream();
                    image.Save(memoryStream, ImageFormat.Png);
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