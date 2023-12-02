// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftTypeController.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#pragma warning disable CA1416
namespace OpenSky.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;
    using OpenSky.API.Model;
    using OpenSky.API.Model.AircraftType;
    using OpenSky.API.Model.Authentication;
    using SkiaSharp;

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
        /// Get all valid variants of the specified aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/11/2021.
        /// </remarks>
        /// <param name="type">
        /// The new aircraft type to evaluate.
        /// </param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the variants in this collection.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static IEnumerable<AircraftType> GetVariants(AircraftType type)
        {
            var variants = new List<AircraftType>();
            var baseType = type.VariantType ?? type;
            variants.Add(baseType);

            if (baseType.Variants?.Count > 0)
            {
                variants.AddRange(baseType.Variants.Where(v => !v.NextVersion.HasValue));
            }

            while (baseType.NextVersion.HasValue)
            {
                baseType = baseType.NextVersionType;
                variants[0] = baseType; // Replace "old" base type with next version

                if (baseType.Variants?.Count > 0)
                {
                    variants.AddRange(baseType.Variants.Where(v => !v.NextVersion.HasValue));
                }
            }

            return variants;
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
        /// <param name="upgradeForType">
        /// The ID of the aircraft type this one is an update for (auto adjusts next version and variants).
        /// </param>
        /// <returns>
        /// A basic API result containing the GUID of the newly created aircraft type.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost(Name = "AddAircraftType")]
        public async Task<ActionResult<ApiResponse<Guid>>> AddAircraftType([FromBody] AircraftType type, Guid? upgradeForType)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST AircraftType");

                // ReSharper disable once AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<Guid> { Message = "Unable to find user record!", IsError = true, Data = Guid.Empty };
                }

                var userRoles = await this.userManager.GetRolesAsync(user);

                // Would the new type create a variant-chain?
                if (type.IsVariantOf.HasValue)
                {
                    var variantType = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == type.IsVariantOf.Value);
                    if (variantType == null)
                    {
                        return new ApiResponse<Guid> { Message = "Unable to find specified variant type!", IsError = true, Data = Guid.Empty };
                    }

                    if (variantType.IsVariantOf.HasValue)
                    {
                        return new ApiResponse<Guid> { Message = "Not allowed to create variant chains, please set variant to base type.", IsError = true, Data = Guid.Empty };
                    }
                }

                // Does the manufacturer exist?
                if (!string.IsNullOrEmpty(type.ManufacturerID))
                {
                    var manufacturer = await this.db.AircraftManufacturers.SingleOrDefaultAsync(m => m.ID == type.ManufacturerID);
                    if (manufacturer == null)
                    {
                        return new ApiResponse<Guid> { Message = "Manufacturer does not exist!", IsError = true, Data = Guid.Empty };
                    }
                }

                // Set a few defaults that the user should not be able to set differently
                type.ID = Guid.NewGuid();
                type.Enabled = userRoles.Contains(UserRoles.Moderator) || userRoles.Contains(UserRoles.Admin);
                type.DetailedChecksDisabled = false;
                type.UploaderID = user.Id;
                type.NextVersion = null;
                await this.db.AircraftTypes.AddAsync(type);

                // Are there any delivery locations added?
                if (type.DeliveryLocations?.Count > 0 && !string.IsNullOrEmpty(type.ManufacturerID))
                {
                    var newDeliveryLocations = new List<AircraftManufacturerDeliveryLocation>();
                    foreach (var deliveryLocation in type.DeliveryLocations)
                    {
                        if (string.IsNullOrEmpty(deliveryLocation.AirportICAO))
                        {
                            continue;
                        }

                        // Does the delivery location airport exist?
                        var airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == deliveryLocation.AirportICAO);
                        if (airport == null)
                        {
                            return new ApiResponse<Guid> { Message = $"Delivery location airport {deliveryLocation.AirportICAO} does not exist!", IsError = true, Data = Guid.Empty };
                        }

                        if (airport.IsClosed)
                        {
                            return new ApiResponse<Guid> { Message = $"Delivery location airport {deliveryLocation.AirportICAO} is closed!", IsError = true, Data = Guid.Empty };
                        }

                        if (type.Simulator == Simulator.MSFS && !airport.MSFS)
                        {
                            return new ApiResponse<Guid> { Message = $"Delivery location airport {deliveryLocation.AirportICAO} is not available for aircraft type simulator!", IsError = true, Data = Guid.Empty };
                        }

                        if (type.Simulator == Simulator.XPlane11 && !airport.XP11)
                        {
                            return new ApiResponse<Guid> { Message = $"Delivery location airport {deliveryLocation.AirportICAO} is not available for aircraft type simulator!", IsError = true, Data = Guid.Empty };
                        }

                        var newDeliveryLocation = new AircraftManufacturerDeliveryLocation
                        {
                            ManufacturerID = type.ManufacturerID,
                            AircraftTypeID = type.ID,
                            AirportICAO = deliveryLocation.AirportICAO
                        };
                        newDeliveryLocations.Add(newDeliveryLocation);
                    }

                    type.DeliveryLocations.Clear();
                    await this.db.AircraftManufacturerDeliveryLocations.AddRangeAsync(newDeliveryLocations);
                }

                if (string.IsNullOrEmpty(type.ManufacturerID))
                {
                    type.ManufacturerID = "miss";
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving new aircraft type.");
                if (saveEx != null)
                {
                    return new ApiResponse<Guid>("Error saving new aircraft type", saveEx) { Data = Guid.Empty };
                }

                // New type was added successfully, update the previous version and variantOf pointers (for mods and admins only, simply ignore for users)
                if (upgradeForType.HasValue && (userRoles.Contains(UserRoles.Moderator) || userRoles.Contains(UserRoles.Admin)))
                {
                    var previousType = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == upgradeForType.Value);
                    if (previousType != null)
                    {
                        previousType.NextVersion = type.ID;
                    }

                    var variantsOfPrevious = await this.db.AircraftTypes.Where(t => t.IsVariantOf == upgradeForType).ToListAsync();
                    foreach (var variantOfPrevious in variantsOfPrevious)
                    {
                        variantOfPrevious.IsVariantOf = type.ID;
                    }

                    var saveExUpdate = await this.db.SaveDatabaseChangesAsync(this.logger, "Error updating previous version or variants.");
                    if (saveExUpdate != null)
                    {
                        return new ApiResponse<Guid>("New aircraft type added successfully but it needs to be reviewed before it will be active in OpenSky.\r\nHowever there was an error trying to update previous versions or variants thereof, please review those manually!", saveExUpdate) { Data = type.ID };
                    }
                }

                return new ApiResponse<Guid>("New aircraft type added successfully but it needs to be reviewed before it will be active in OpenSky.") { Data = type.ID };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST AircraftType");
                return new ApiResponse<Guid>(ex) { Data = Guid.Empty };
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
        /// Get the list aircraft manufacturers.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/02/2022.
        /// </remarks>
        /// <returns>
        /// The aircraft manufacturers.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("manufacturers", Name = "GetAircraftManufacturers")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AircraftManufacturer>>>> GetAircraftManufacturers()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AircraftType/manufacturers");
                var manufacturers = await this.db.AircraftManufacturers.ToListAsync();
                return new ApiResponse<IEnumerable<AircraftManufacturer>>(manufacturers);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AircraftType/manufacturers");
                return new ApiResponse<IEnumerable<AircraftManufacturer>>(ex) { Data = new List<AircraftManufacturer>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get image for the specified aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/02/2022.
        /// </remarks>
        /// <param name="typeID">
        /// Identifier for the type.
        /// </param>
        /// <returns>
        /// The aircraft type image.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("image/{typeID:guid}", Name = "GetAircraftTypeImage")]
        public async Task<ActionResult<ApiResponse<byte[]>>> GetAircraftTypeImage(Guid typeID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AircraftType/image/{typeID}");
                var type = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == typeID);
                return new ApiResponse<byte[]>(type.AircraftImage);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AircraftType/image/{typeID}");
                return new ApiResponse<byte[]>(ex);
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
                var types = await this.db.AircraftTypes
                                      .Where(t => t.Enabled && !t.NextVersion.HasValue)
                                      .Include(aircraftType => aircraftType.Manufacturer)
                                      .Include(aircraftType => aircraftType.Variants)
                                      .ThenInclude(aircraftType => aircraftType.Manufacturer)
                                      .ToListAsync();

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
        /// Get available aircraft upgrades.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/11/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the aircraft type upgrades.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("upgrades", Name = "GetAircraftTypeUpgrades")]
        [Roles(UserRoles.Moderator, UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AircraftTypeUpgrade>>>> GetAircraftTypeUpgrades()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AircraftType/upgrades");
                var updates = new List<AircraftTypeUpgrade>();
                var types = await this.db.AircraftTypes
                                      .Where(t => t.NextVersion.HasValue)
                                      .Include(aircraftType => aircraftType.NextVersionType)
                                      .ToListAsync();
                foreach (var type in types)
                {
                    AircraftType to = null;
                    var aircraft = await this.db.Aircraft.CountAsync(a => a.TypeID == type.ID);
                    if (aircraft > 0)
                    {
                        var copyType = type;
                        while (copyType.NextVersionType != null)
                        {
                            if (type.NextVersionType.Enabled)
                            {
                                to = type.NextVersionType;
                            }

                            copyType = copyType.NextVersionType;
                        }

                        if (to != null)
                        {
                            // Found both aircraft to upgrade and a valid target
                            updates.Add(new AircraftTypeUpgrade { From = type, To = to, AircraftCount = aircraft });
                        }
                    }
                }

                return new ApiResponse<IEnumerable<AircraftTypeUpgrade>>(updates);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AircraftType/upgrades");
                return new ApiResponse<IEnumerable<AircraftTypeUpgrade>>(ex) { Data = new List<AircraftTypeUpgrade>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets all aircraft types (including disabled and previous versions).
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// <returns>
        /// All aircraft types (including disabled and previous versions).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("all", Name = "GetAllAircraftTypes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AircraftType>>>> GetAllAircraftTypes()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AircraftType/all");
                var types = await this.db.AircraftTypes
                                      .Include(aircraftType => aircraftType.Manufacturer)
                                      .Include(aircraftType => aircraftType.Variants)
                                      .ThenInclude(aircraftType => aircraftType.Manufacturer)
                                      .ToListAsync();

                return new ApiResponse<IEnumerable<AircraftType>>(types);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AircraftType/all");
                return new ApiResponse<IEnumerable<AircraftType>>(ex) { Data = new List<AircraftType>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets all aircraft types for the specified simulator (including disabled and previous).
        /// versions).
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/11/2021.
        /// </remarks>
        /// <param name="simulator">
        /// The simulator.
        /// </param>
        /// <returns>
        /// All aircraft types for the specified simulator (including disabled and previous).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("simulator/{simulator}", Name = "GetSimulatorAircraftTypes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AircraftType>>>> GetSimulatorAircraftTypes(Simulator simulator)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AircraftType/simulator/{simulator}");
                var types = await this.db.AircraftTypes
                                      .Where(at => at.Simulator == simulator)
                                      .Include(aircraftType => aircraftType.Manufacturer)
                                      .Include(aircraftType => aircraftType.Variants)
                                      .ThenInclude(aircraftType => aircraftType.Manufacturer)
                                      .ToListAsync();

                return new ApiResponse<IEnumerable<AircraftType>>(types);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AircraftType/simulator/{simulator}");
                return new ApiResponse<IEnumerable<AircraftType>>(ex) { Data = new List<AircraftType>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the variants of this type (can be called with base or one of the variant sub-types)
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/11/2021.
        /// </remarks>
        /// <param name="typeID">
        /// Identifier for the type.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the variants of type.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("variants/{typeID:guid}", Name = "GetVariantsOfType")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AircraftType>>>> GetVariantsOfType(Guid typeID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET AircraftType/variants/{typeID}");
                var type = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == typeID);
                if (type == null)
                {
                    return new ApiResponse<IEnumerable<AircraftType>>("No aircraft type exists with the specified ID!") { IsError = true, Data = new List<AircraftType>() };
                }

                var variants = GetVariants(type);
                return new ApiResponse<IEnumerable<AircraftType>>(variants);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET AircraftType/variants/{typeID}");
                return new ApiResponse<IEnumerable<AircraftType>>(ex) { Data = new List<AircraftType>() };
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

                // ReSharper disable once AssignNullToNotNullAttribute
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

                // Make sure not creating a variant loop by being a variant of itself
                if (type.IsVariantOf.HasValue && type.IsVariantOf.Value == type.ID)
                {
                    return new ApiResponse<string> { Message = "An aircraft type can't be variant of itself!", IsError = true };
                }

                // Would this update create a variant-chain?
                if (type.IsVariantOf.HasValue)
                {
                    var variantType = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == type.IsVariantOf.Value);
                    if (variantType == null)
                    {
                        return new ApiResponse<string> { Message = "Unable to find specified variant type!", IsError = true };
                    }

                    if (variantType.IsVariantOf.HasValue)
                    {
                        return new ApiResponse<string> { Message = "Not allowed to create variant chains, please set variant to base type.", IsError = true };
                    }
                }

                // Transfer the editable properties
                existingType.Name = type.Name;
                existingType.ManufacturerID = string.IsNullOrEmpty(type.ManufacturerID) ? "miss" : type.ManufacturerID;
                existingType.VersionNumber = type.VersionNumber;
                existingType.Category = type.Category;
                existingType.IsVanilla = type.IsVanilla;
                existingType.NeedsCoPilot = type.NeedsCoPilot;
                existingType.NeedsFlightEngineer = type.NeedsFlightEngineer;
                existingType.RequiresManualFuelling = type.RequiresManualFuelling;
                existingType.RequiresManualLoading = type.RequiresManualLoading;
                existingType.IsVariantOf = type.IsVariantOf;
                existingType.NextVersion = type.NextVersion;
                existingType.MinPrice = type.MinPrice;
                existingType.MaxPrice = type.MaxPrice;
                existingType.Comments = type.Comments;
                existingType.FuelWeightPerGallon = type.FuelWeightPerGallon;
                existingType.MinimumRunwayLength = type.MinimumRunwayLength;
                existingType.IncludeInWorldPopulation = type.IncludeInWorldPopulation;
                existingType.MaxPayloadDeltaAllowed = type.MaxPayloadDeltaAllowed;
                existingType.EngineModel = type.EngineModel;
                existingType.OverrideFuelType = type.OverrideFuelType;
                existingType.IsHistoric = type.IsHistoric;
                existingType.UsesStrobeForBeacon = type.UsesStrobeForBeacon;

                // Process changes to delivery locations
                if (type.DeliveryLocations?.Count > 0)
                {
                    var existingICAOs = existingType.DeliveryLocations.Select(dl => dl.AirportICAO).ToHashSet();

                    foreach (var deliveryLocation in type.DeliveryLocations)
                    {
                        if (string.IsNullOrEmpty(deliveryLocation.AirportICAO))
                        {
                            continue;
                        }

                        // Does the delivery location airport exist?
                        var airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == deliveryLocation.AirportICAO);
                        if (airport == null)
                        {
                            return new ApiResponse<string> { Message = $"Delivery location airport {deliveryLocation.AirportICAO} does not exist!", IsError = true };
                        }

                        if (airport.IsClosed)
                        {
                            return new ApiResponse<string> { Message = $"Delivery location airport {deliveryLocation.AirportICAO} is closed!", IsError = true };
                        }

                        if (type.Simulator == Simulator.MSFS && !airport.MSFS)
                        {
                            return new ApiResponse<string> { Message = $"Delivery location airport {deliveryLocation.AirportICAO} is not available for aircraft type simulator!", IsError = true };
                        }

                        if (type.Simulator == Simulator.XPlane11 && !airport.XP11)
                        {
                            return new ApiResponse<string> { Message = $"Delivery location airport {deliveryLocation.AirportICAO} is not available for aircraft type simulator!", IsError = true };
                        }

                        if (!existingICAOs.Contains(deliveryLocation.AirportICAO))
                        {
                            var newDeliveryLocation = new AircraftManufacturerDeliveryLocation
                            {
                                ManufacturerID = type.ManufacturerID,
                                AircraftTypeID = type.ID,
                                AirportICAO = deliveryLocation.AirportICAO
                            };
                            await this.db.AircraftManufacturerDeliveryLocations.AddAsync(newDeliveryLocation);
                        }
                        else
                        {
                            existingICAOs.Remove(deliveryLocation.AirportICAO);
                        }
                    }

                    // Remove the locations still left in the hashset
                    this.db.RemoveRange(this.db.AircraftManufacturerDeliveryLocations.Where(dl => existingICAOs.Contains(dl.AirportICAO) && dl.AircraftTypeID == existingType.ID));
                }
                else
                {
                    this.db.RemoveRange(this.db.AircraftManufacturerDeliveryLocations.Where(dl => dl.AircraftTypeID == existingType.ID));
                }

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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Update the image of the specified aircraft type.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/02/2022.
        /// </remarks>
        /// <param name="typeID">
        /// Identifier for the aircraft type.
        /// </param>
        /// <param name="fileUpload">
        /// The file upload containing the new image.
        /// </param>
        /// <returns>
        /// A status string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("image/{typeID:guid}", Name = "UpdateAircraftTypeImage")]
        [Roles(UserRoles.Moderator, UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> UpdateAircraftTypeImage(Guid typeID, IFormFile fileUpload)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST AircraftType/image/{typeID}");

                var type = await this.db.AircraftTypes.SingleOrDefaultAsync(t => t.ID == typeID);

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
                memoryStream.Seek(0, SeekOrigin.Begin);

                var image = SKBitmap.Decode(memoryStream);
                if (image.Width > 640 || image.Height > 360)
                {
                    image = image.Resize(new SKSizeI(300, 300), SKFilterQuality.High);
                    memoryStream = new MemoryStream();
                    image.Encode(memoryStream, SKEncodedImageFormat.Png, 100);
                }

                type.AircraftImage = memoryStream.ToArray();
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving changes to aircraft type.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error saving changes to aircraft type", saveEx);
                }

                return new ApiResponse<string>("Aircraft type image updated.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST AircraftType/image/{typeID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Performs the specified aircraft type upgrade.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/11/2021.
        /// </remarks>
        /// <param name="upgrade">
        /// The upgrade to perform (from/to type).
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("upgrade", Name = "UpgradeAircraftType")]
        [Roles(UserRoles.Moderator, UserRoles.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> UpgradeAircraftType([FromBody] AircraftTypeUpgrade upgrade)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST AircraftType/upgrade");
                var from = await this.db.AircraftTypes
                                     .Include(aircraftType => aircraftType.NextVersionType)
                                     .SingleOrDefaultAsync(t => t.ID == upgrade.From.ID);
                if (from == null)
                {
                    return new ApiResponse<string>("No such aircraft type!") { IsError = true };
                }

                var validUpdate = false;
                while (from.NextVersionType != null)
                {
                    if (from.NextVersion == upgrade.To.ID && from.NextVersionType.Enabled)
                    {
                        validUpdate = true;
                        break;
                    }

                    from = from.NextVersionType;
                }

                if (!validUpdate)
                {
                    return new ApiResponse<string>("Not a valid update path!") { IsError = true };
                }

                var aircrafts = await this.db.Aircraft.Where(a => a.TypeID == upgrade.From.ID).ToListAsync();
                var count = 0;
                foreach (var aircraft in aircrafts)
                {
                    aircraft.TypeID = upgrade.To.ID;
                    count++;
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error upgrading aircraft.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error saving aircraft upgrades", saveEx);
                }

                return new ApiResponse<string>($"Successfully upgraded {count} aircraft!");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST AircraftType/upgrade");
                return new ApiResponse<string>(ex);
            }
        }
    }
}