// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountController.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
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
            this.logger.LogInformation($"Getting account overview for user {this.User.Identity?.Name}");

            var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
            if (user == null)
            {
                return new ApiResponse<AccountOverview> { Message = "Unable to find user record!", IsError = true, Data = new AccountOverview() };
            }

            var accountOverview = new AccountOverview
            {
                Name = user.UserName,
                Joined = user.RegisteredOn,
                ProfileImage = user.ProfileImage
            };

            return new ApiResponse<AccountOverview>(accountOverview);
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
            this.logger.LogInformation($"Getting linked accounts for user {this.User.Identity?.Name}");

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
        /// An asynchronous result that yields a string;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("linkedAccounts", Name = "UpdateLinkedAccounts")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateLinkedAccounts([FromBody] LinkedAccounts linkedAccounts)
        {
            this.logger.LogInformation($"Updating linked accounts for user {this.User.Identity?.Name}");

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
    }
}