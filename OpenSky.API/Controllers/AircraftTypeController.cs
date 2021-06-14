// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeController.cs" company="OpenSky">
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
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.Helpers;
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
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST AircraftType");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes the specified aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// <param name="typeID">
        /// Identifier for the type.
        /// </param>
        /// <returns>
        /// A basic API result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpDelete("{typeID:guid}", Name = "DeleteAircraftType")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> DeleteAircraftType(Guid typeID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | DELETE AircraftType/{typeID}");
                var type = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == typeID);
                if (type == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find existing aircraft type!", IsError = true };
                }

                // Make sure there are no aircraft using the type
                var inUse = await this.db.Aircraft.AnyAsync(a => a.TypeID == typeID);
                if (inUse)
                {
                    return new ApiResponse<string> { Message = "This aircraft type is in use and can't be deleted!", IsError = true };
                }

                this.db.AircraftTypes.Remove(type);
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error deleting aircraft type.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error deleting aircraft type", saveEx);
                }

                return new ApiResponse<string>("The aircraft type was deleted successfully.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | DELETE AircraftType/{typeID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Disables the specified aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// <param name="typeID">
        /// Identifier for the type.
        /// </param>
        /// <returns>
        /// A basic API result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut("disable/{typeID:guid}", Name = "DisableAircraftType")]
        [Roles(UserRoles.Moderator, UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> DisableAircraftType(Guid typeID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT AircraftType/disable/{typeID}");
                var type = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == typeID);
                if (type == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find existing aircraft type!", IsError = true };
                }

                type.Enabled = false;
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error disabling aircraft type.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error disabling aircraft type", saveEx);
                }

                return new ApiResponse<string>("Success.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | PUT AircraftType/disable/{typeID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Disables the specified aircraft type's detailed checks (only to be used on patch days until a new aircraft type version can be created!).
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// <param name="typeID">
        /// Identifier for the type.
        /// </param>
        /// <returns>
        /// A basic API result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut("disableDetailedChecks/{typeID:guid}", Name = "DisableAircraftTypeDetailedChecks")]
        [Roles(UserRoles.Moderator, UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> DisableAircraftTypeDetailedChecks(Guid typeID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT AircraftType/disableDetailedChecks/{typeID}");
                var type = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == typeID);
                if (type == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find existing aircraft type!", IsError = true };
                }

                type.DetailedChecksDisabled = true;
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error disabling aircraft type detailed checks.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error disabling aircraft type detailed checks", saveEx);
                }

                return new ApiResponse<string>("Success.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | PUT AircraftType/disableDetailedChecks/{typeID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Enables the specified aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// <param name="typeID">
        /// Identifier for the type.
        /// </param>
        /// <returns>
        /// A basic API result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut("enable/{typeID:guid}", Name = "EnableAircraftType")]
        [Roles(UserRoles.Moderator, UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> EnableAircraftType(Guid typeID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT AircraftType/enable/{typeID}");
                var type = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == typeID);
                if (type == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find existing aircraft type!", IsError = true };
                }

                type.Enabled = true;
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error enabling aircraft type.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error enabling aircraft type", saveEx);
                }

                return new ApiResponse<string>("Success.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | PUT AircraftType/enable/{typeID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Enables the specified aircraft type's detailed checks (only to be used on patch days until a new aircraft type version can be created!).
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// <param name="typeID">
        /// Identifier for the type.
        /// </param>
        /// <returns>
        /// A basic API result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut("enableDetailedChecks/{typeID:guid}", Name = "EnableAircraftTypeDetailedChecks")]
        [Roles(UserRoles.Moderator, UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> EnableAircraftTypeDetailedChecks(Guid typeID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT AircraftType/enableDetailedChecks/{typeID}");
                var type = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == typeID);
                if (type == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find existing aircraft type!", IsError = true };
                }

                type.DetailedChecksDisabled = false;
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error enabling aircraft type detailed checks.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error enabling aircraft type detailed checks", saveEx);
                }

                return new ApiResponse<string>("Success.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | PUT AircraftType/enableDetailedChecks/{typeID}");
                return new ApiResponse<string>(ex);
            }
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
        /// Updates an existing aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// <param name="type">
        /// The aircraft type to update.
        /// </param>
        /// <returns>
        /// A basic API result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut(Name = "UpdateAircraftType")]
        [Roles(UserRoles.Moderator, UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateAircraftType([FromBody] AircraftType type)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT AircraftType");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                // Make sure type exists and that certain values can't be overridden (like original uploader and last edited by)
                var existingType = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == type.ID);
                if (existingType == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find existing aircraft type!", IsError = true };
                }

                // Transfer the editable properties
                existingType.Name = type.Name;
                existingType.VersionNumber = type.VersionNumber;
                existingType.Category = type.Category;
                existingType.IsVanilla = type.IsVanilla;
                existingType.NeedsCoPilot = type.NeedsCoPilot;
                existingType.IsVariantOf = type.IsVariantOf;
                existingType.NextVersion = type.NextVersion;
                existingType.MinPrice = type.MinPrice;
                existingType.MaxPrice = type.MaxPrice;
                existingType.Comments = type.Comments;

                // Make sure to record who edited it
                existingType.LastEditedByID = user.Id;

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving changes to aircraft type.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error saving changes to aircraft type", saveEx);
                }

                return new ApiResponse<string>("Changes saved successfully.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | PUT AircraftType");
                return new ApiResponse<string>(ex);
            }
        }
    }
}