namespace OpenSky.API.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using OpenSky.API.DbModel;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;
    using OpenSky.API.Model.Notification;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Notification controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/12/2023.
    /// </remarks>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
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
        private readonly ILogger<AirportController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserManager<OpenSkyUser> userManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/12/2023.
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
        /// -------------------------------------------------------------------------------------------------
        public NotificationController(ILogger<AirportController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
        }

        [HttpPost(Name = "AddNotification")]
        [Authorize(Roles = UserRoles.Moderator)]
        public async Task<ActionResult<ApiResponse<string>>> AddNotification([FromBody] AddNotification addNotification)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Notification");

                // ReSharper disable once AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Notification");
                return new ApiResponse<string>(ex);
            }

            return null;
        }
    }
}
