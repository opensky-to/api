// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirlinePermissionsController.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.Model.Authentication;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airline permissions controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Route("[controller]")]
    public class AirlinePermissionsController
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
        private readonly ILogger<AirlinePermissionsController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserManager<OpenSkyUser> userManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirlinePermissionsController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/10/2021.
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
        public AirlinePermissionsController(ILogger<AirlinePermissionsController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
        }
    }
}