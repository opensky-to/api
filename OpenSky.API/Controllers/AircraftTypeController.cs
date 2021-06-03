using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSky.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft type controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 01/06/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Route("[controller]")]
    public class AircraftTypeController : ControllerBase
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
        /// Initializes a new instance of the <see cref="AircraftTypeController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
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
        public AircraftTypeController(ILogger<AirportController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets all enabled and current aircraft types.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// <returns>
        /// All enabled and current aircraft types.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet(Name = "GetAircraftTypes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AircraftType>>>> GetAircraftTypes()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AircraftType");
                var types = await this.db.AircraftTypes.Where(t => t.Enabled && !t.NextVersion.HasValue).ToListAsync();
                return new ApiResponse<IEnumerable<AircraftType>>(types);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AircraftType");
                return new ApiResponse<IEnumerable<AircraftType>>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets all aircraft types (including disabled and previous).
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// <returns>
        /// All aircraft types (including disabled and previous).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("all", Name = "GetAllAircraftTypes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AircraftType>>>> GetAllAircraftTypes()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AircraftType/all");
                var types = await this.db.AircraftTypes.ToListAsync();
                return new ApiResponse<IEnumerable<AircraftType>>(types);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AircraftType/all");
                return new ApiResponse<IEnumerable<AircraftType>>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds a new aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// <param name="type">
        /// The new aircraft type to add.
        /// </param>
        /// <returns>
        /// A basic API result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost(Name = "AddAircraftType")]
        public async Task<ActionResult<ApiResponse<string>>> AddAircraftType([FromBody] AircraftType type)
        {
            try
            {
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return this.Ok(new ApiResponse<LoginResponse> { Message = "Unable to find user record!", IsError = true });
                }

                // Set a few defaults that the user should not be able to set differently
                type.ID = Guid.NewGuid();
                type.Enabled = false;
                type.DetailedChecksDisabled = false;
                type.UploaderID = user.Id;
                type.NextVersion = null;
                await this.db.AircraftTypes.AddAsync(type);
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving new aircraft type.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error saving new aircraft type", saveEx);
                }

                return new ApiResponse<string>("New aircraft type added successfully but it needs to be reviewed before it will be active in OpenSky.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST AircraftType");
                return new ApiResponse<string>(ex);
            }
        }
    }
}
