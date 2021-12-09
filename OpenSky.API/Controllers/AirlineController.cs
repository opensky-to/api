// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirlineController.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Airline;
    using OpenSky.API.Model.Authentication;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airline controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Route("[controller]")]
    public class AirlineController : ControllerBase
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky database context.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        // ReSharper disable once NotAccessedField.Local
        private readonly OpenSkyDbContext db;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<AirlineController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserManager<OpenSkyUser> userManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirlineController"/> class.
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
        public AirlineController(ILogger<AirlineController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
        }

        // todo assign/revoke permissions methods for airline managers

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get user airline.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/11/2021.
        /// </remarks>
        /// <returns>
        /// The user's airline.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet(Name = "GetAirline")]
        public async Task<ActionResult<ApiResponse<UserAirline>>> GetUserAirline()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Airline");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<UserAirline> { Message = "Unable to find user record!", IsError = true, Data = new UserAirline() };
                }

                if (string.IsNullOrEmpty(user.AirlineICAO))
                {
                    return new ApiResponse<UserAirline>(new UserAirline()) { Message = "Not part of an airline!" };
                }

                return new ApiResponse<UserAirline>(new UserAirline(user.Airline));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AirlinePermissions");
                return new ApiResponse<UserAirline>(ex) { Data = new UserAirline() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the airline permissions for the current user.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/10/2021.
        /// </remarks>
        /// <returns>
        /// The list of airline permissions.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("permissions", Name = "GetAirlinePermissions")]
        public async Task<ActionResult<ApiResponse<List<AirlinePermission>>>> GetAirlinePermissions()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AirlinePermissions");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<List<AirlinePermission>> { Message = "Unable to find user record!", IsError = true, Data = new List<AirlinePermission>() };
                }

                if (string.IsNullOrEmpty(user.AirlineICAO))
                {
                    return new ApiResponse<List<AirlinePermission>> { Message = "Not member of an airline!", IsError = true, Data = new List<AirlinePermission>() };
                }

                // Is this the founder?
                if (user.Airline.FounderID == user.Id)
                {
                    return new ApiResponse<List<AirlinePermission>>(new List<AirlinePermission> { AirlinePermission.AllPermissions });
                }

                var permissions = user.AirlinePermissions.Select(p => p.Permission).ToList();
                return new ApiResponse<List<AirlinePermission>>(permissions);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AirlinePermissions");
                return new ApiResponse<List<AirlinePermission>>(ex) { Data = new List<AirlinePermission>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets all airline permissions (for all members, requires ChangePermission)
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/10/2021.
        /// </remarks>
        /// <returns>
        /// Dictionary containing sets of all user permissions.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("permissions/all", Name = "GetAllAirlinePermissions")]
        public async Task<ActionResult<ApiResponse<Dictionary<AirlineMember, HashSet<AirlinePermission>>>>> GetAllAirlinePermissions()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AllAirlinePermissions");

                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<Dictionary<AirlineMember, HashSet<AirlinePermission>>> { Message = "Unable to find user record!", IsError = true, Data = new Dictionary<AirlineMember, HashSet<AirlinePermission>>() };
                }

                if (string.IsNullOrEmpty(user.AirlineICAO))
                {
                    return new ApiResponse<Dictionary<AirlineMember, HashSet<AirlinePermission>>> { Message = "Not member of an airline!", IsError = true, Data = new Dictionary<AirlineMember, HashSet<AirlinePermission>>() };
                }

                if (!UserHasPermission(user, AirlinePermission.ChangePermissions))
                {
                    return new ApiResponse<Dictionary<AirlineMember, HashSet<AirlinePermission>>> { Message = "Unauthorized request!", IsError = true, Data = new Dictionary<AirlineMember, HashSet<AirlinePermission>>() };
                }

                var permissions = new Dictionary<AirlineMember, HashSet<AirlinePermission>>();
                foreach (var airlineUserPermission in user.Airline.UserPermissions)
                {
                    var member = new AirlineMember(airlineUserPermission.User);
                    if (!permissions.ContainsKey(member))
                    {
                        permissions.Add(member, new HashSet<AirlinePermission>());
                    }

                    permissions[member].Add(airlineUserPermission.Permission);
                }

                return new ApiResponse<Dictionary<AirlineMember, HashSet<AirlinePermission>>>(permissions);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AllAirlinePermissions");
                return new ApiResponse<Dictionary<AirlineMember, HashSet<AirlinePermission>>>(ex)
                {
                    Data = new Dictionary<AirlineMember, HashSet<AirlinePermission>>()
                };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Checks if the specified user has the specified airline permission.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/10/2021.
        /// </remarks>
        /// <param name="user">
        /// The OpenSky user.
        /// </param>
        /// <param name="permission">
        /// The permission.
        /// </param>
        /// <returns>
        /// True if the user has the permissions (or all permissions), false otherwise.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        internal static bool UserHasPermission(OpenSkyUser user, AirlinePermission permission)
        {
            return user.AirlinePermissions.Any(p => p.Permission == permission || p.Permission == AirlinePermission.AllPermissions);
        }
    }
}