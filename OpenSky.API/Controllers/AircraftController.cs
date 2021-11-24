// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftController.cs" company="OpenSky">
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
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Aircraft;
    using OpenSky.API.Model.Authentication;
    using OpenSky.API.Services;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 15/07/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Route("[controller]")]
    public class AircraftController : ControllerBase
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky database context.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly OpenSkyDbContext db;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The icao registration service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly IcaoRegistrationsService icaoRegistrations;

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
        /// The world populator service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly WorldPopulatorService worldPopulator;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/07/2021.
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
        /// <param name="icaoRegistrations">
        /// The icao registration service.
        /// </param>
        /// <param name="worldPopulator">
        /// The world populator service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftController(ILogger<AirportController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager, IcaoRegistrationsService icaoRegistrations, WorldPopulatorService worldPopulator)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.icaoRegistrations = icaoRegistrations;
            this.worldPopulator = worldPopulator;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get the aircraft record for the specified registry.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/07/2021.
        /// </remarks>
        /// <param name="registry">
        /// The registry.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the aircraft.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("{registry}", Name = "GetAircraft")]
        public async Task<ActionResult<ApiResponse<Aircraft>>> GetAircraft(string registry)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Aircraft/{registry}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<Aircraft> { Message = "Unable to find user record!", IsError = true };
                }

                var aircraft = await this.db.Aircraft.SingleOrDefaultAsync(a => a.Registry.Equals(registry));
                if (aircraft == null)
                {
                    return new ApiResponse<Aircraft>("Aircraft not found!") { IsError = true };
                }

                // Only return planes that are available for purchase or rent, or owned by the player (or the player airline), or are currently rented by the player or airline
                // todo include rented!
                if (!this.User.IsInRole(UserRoles.Moderator) && !this.User.IsInRole(UserRoles.Admin))
                {
                    if (aircraft.OwnerID != user.Id && aircraft.AirlineOwnerID == user.AirlineICAO && !aircraft.PurchasePrice.HasValue && !aircraft.RentPrice.HasValue)
                    {
                        return new ApiResponse<Aircraft>("Aircraft not found!") { IsError = true };
                    }
                }

                return new ApiResponse<Aircraft>(aircraft);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Aircraft/{registry}");
                return new ApiResponse<Aircraft>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get the aircraft at a given airport.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/07/2021.
        /// </remarks>
        /// <param name="icao">
        /// The icao of the airport.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the aircraft at airport.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("atAirport/{icao}", Name = "GetAircraftAtAirport")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Aircraft>>>> GetAircraftAtAirport(string icao)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Aircraft/atAirport/{icao}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<Aircraft>> { Message = "Unable to find user record!", IsError = true, Data = new List<Aircraft>() };
                }

                if (await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO.Equals(icao)) == null)
                {
                    return new ApiResponse<IEnumerable<Aircraft>> { Message = $"No airport with code {icao} exists!", IsError = true, Data = new List<Aircraft>() };
                }

                if (this.User.IsInRole(UserRoles.Moderator) || this.User.IsInRole(UserRoles.Admin))
                {
                    // Return all planes
                    var aircraft = await this.db.Aircraft.Where(a => a.AirportICAO.Equals(icao) && !a.Flights.Any(f => f.Started.HasValue && !f.Completed.HasValue)).ToListAsync();

                    return new ApiResponse<IEnumerable<Aircraft>>(aircraft);
                }
                else
                {
                    // Only return planes that are available for purchase or rent, or owned by the player
                    var aircraft = await this.db.Aircraft.Where(
                                                 a => a.AirportICAO.Equals(icao) && !a.Flights.Any(f => f.Started.HasValue && !f.Completed.HasValue) &&
                                                      (a.OwnerID == user.Id || a.AirlineOwnerID == user.AirlineICAO || a.PurchasePrice.HasValue || a.RentPrice.HasValue))
                                             .ToListAsync();
                    return new ApiResponse<IEnumerable<Aircraft>>(aircraft);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Aircraft/atAirport/{icao}");
                return new ApiResponse<IEnumerable<Aircraft>>(ex) { Data = new List<Aircraft>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get the player owned aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/07/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields my aircraft.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("myAircraft", Name = "GetMyAircraft")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Aircraft>>>> GetMyAircraft()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Aircraft/myAircraft");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<Aircraft>> { Message = "Unable to find user record!", IsError = true, Data = new List<Aircraft>() };
                }

                var aircraft = await this.db.Aircraft.Where(a => a.OwnerID == user.Id).ToListAsync();
                return new ApiResponse<IEnumerable<Aircraft>>(aircraft);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Aircraft/myAircraft");
                return new ApiResponse<IEnumerable<Aircraft>>(ex) { Data = new List<Aircraft>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Purchase specified aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/07/2021.
        /// </remarks>
        /// <param name="purchase">
        /// The purchase aircraft model.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("purchase", Name = "PurchaseAircraft")]
        public async Task<ActionResult<ApiResponse<string>>> PurchaseAircraft([FromBody] PurchaseAircraft purchase)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Aircraft/purchase: {purchase.Registry}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                if (string.IsNullOrEmpty(user.AirlineICAO) && purchase.ForAirline)
                {
                    return new ApiResponse<string> { Message = "Not member of an airline!", IsError = true };
                }

                if (!AirlineController.UserHasPermission(user, AirlinePermission.BuyAircraft) && purchase.ForAirline)
                {
                    return new ApiResponse<string> { Message = "You don't have the permission to buy aircraft for your airline!", IsError = true };
                }

                var aircraft = await this.db.Aircraft.SingleOrDefaultAsync(a => a.Registry.Equals(purchase.Registry));
                if (aircraft == null)
                {
                    return new ApiResponse<string>("Aircraft not found!") { IsError = true };
                }

                if (!aircraft.PurchasePrice.HasValue)
                {
                    return new ApiResponse<string>($"Aircraft with registry {purchase.Registry} is not for sale!") { IsError = true };
                }

                if (aircraft.OwnerID == user.Id && !purchase.ForAirline)
                {
                    return new ApiResponse<string>("You already own this aircraft!") { IsError = true };
                }

                if (aircraft.AirlineOwnerID == user.AirlineICAO && purchase.ForAirline)
                {
                    return new ApiResponse<string>("Your airline already owns this aircraft!") { IsError = true };
                }

                if (aircraft.Flights.Any(f => f.Started.HasValue && !f.Completed.HasValue))
                {
                    return new ApiResponse<string>("You can't buy aircraft with an active flight!") { IsError = true };
                }

                if (purchase.VariantID != Guid.Empty && aircraft.TypeID != purchase.VariantID)
                {
                    var variants = AircraftTypeController.GetVariants(aircraft.Type);
                    if (variants.All(v => v.ID != purchase.VariantID))
                    {
                        return new ApiResponse<string>("Invalid variant!") { IsError = true };
                    }

                    aircraft.TypeID = purchase.VariantID;
                }

                // todo check if player/airline has enough money, plus deduct purchase price from account balance

                // todo create some "financial" record for this transaction

                if (!purchase.ForAirline)
                {
                    aircraft.OwnerID = user.Id;
                }
                else
                {
                    aircraft.AirlineOwnerID = user.AirlineICAO;
                }

                aircraft.PurchasePrice = null;
                aircraft.RentPrice = null;

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error purchasing aircraft");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                // Ask world populator to "restock" the airport by adding a new plane in place of this one
                await this.worldPopulator.CheckAndGenerateAircraftForAirport(aircraft.Airport);

                return new ApiResponse<string>($"Successfully purchased aircraft {purchase.Registry}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Aircraft/purchase/purchase: {purchase.Registry}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Searches for aircraft matching the criteria in the specified country.
        /// </summary>
        /// <remarks>
        /// sushi.at, 27/07/2021.
        /// </remarks>
        /// <param name="search">
        /// The search.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the found aircraft in the country.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("searchInCountry", Name = "SearchAircraftInCountry")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Aircraft>>>> SearchAircraftInCountry([FromBody] AircraftSearchInCountry search)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Aircraft/searchInCountry");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<Aircraft>> { Message = "Unable to find user record!", IsError = true, Data = new List<Aircraft>() };
                }

                // Get the ICAO registration entries for the specified country to get the airport prefixes
                var airportPrefixes = this.icaoRegistrations.GetIcaoRegistrationsForCountry(search.Country).Select(r => r.AirportPrefix).Distinct().ToArray();

                var searchResults = new List<Aircraft>();
                foreach (var airportPrefix in airportPrefixes)
                {
                    if (this.User.IsInRole(UserRoles.Moderator) || this.User.IsInRole(UserRoles.Admin))
                    {
                        // Return all matching planes
                        var aircraft = await this.db.Aircraft.Where(
                                                     a => a.AirportICAO.StartsWith(airportPrefix) && !a.Flights.Any(f => f.Started.HasValue && !f.Completed.HasValue) &&
                                                          (!search.OnlyVanilla || a.Type.IsVanilla) &&
                                                          (!search.FilterByCategory || a.Type.Category == search.Category) &&
                                                          (string.IsNullOrEmpty(search.Manufacturer) || a.Type.Manufacturer.Contains(search.Manufacturer)) &&
                                                          (string.IsNullOrEmpty(search.Name) || a.Type.Name.Contains(search.Name)))
                                                 .Take(search.MaxResults).ToListAsync();
                        searchResults.AddRange(aircraft);
                    }
                    else
                    {
                        // Only return planes that are available for purchase or rent, or owned by the player/airline
                        var aircraft = await this.db.Aircraft.Where(
                                                     a => a.AirportICAO.StartsWith(airportPrefix) && !a.Flights.Any(f => f.Started.HasValue && !f.Completed.HasValue) &&
                                                          (!search.OnlyVanilla || a.Type.IsVanilla) &&
                                                          (!search.FilterByCategory || a.Type.Category == search.Category) &&
                                                          (string.IsNullOrEmpty(search.Manufacturer) || a.Type.Manufacturer.Contains(search.Manufacturer)) &&
                                                          (string.IsNullOrEmpty(search.Name) || a.Type.Name.Contains(search.Name)) &&
                                                          (a.OwnerID == user.Id || a.AirlineOwnerID == user.AirlineICAO || a.PurchasePrice.HasValue || a.RentPrice.HasValue))
                                                 .Take(search.MaxResults).ToListAsync();
                        searchResults.AddRange(aircraft);
                    }

                    if (searchResults.Count >= search.MaxResults)
                    {
                        break;
                    }
                }

                return new ApiResponse<IEnumerable<Aircraft>>(searchResults);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Aircraft/searchInCountry");
                return new ApiResponse<IEnumerable<Aircraft>>(ex) { Data = new List<Aircraft>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the specified aircraft (user editable properties only)
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/09/2021.
        /// </remarks>
        /// <param name="updateAircraft">
        /// The update aircraft.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut(Name = "UpdateAircraft")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateAircraft([FromBody] UpdateAircraft updateAircraft)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Aircraft/update/{updateAircraft.Registry}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                var aircraft = await this.db.Aircraft.SingleOrDefaultAsync(a => a.Registry.Equals(updateAircraft.Registry));
                if (aircraft == null)
                {
                    return new ApiResponse<string>("Aircraft not found!") { IsError = true };
                }

                if (!string.IsNullOrEmpty(aircraft.AirlineOwnerID))
                {
                    if (aircraft.AirlineOwnerID != user.AirlineICAO)
                    {
                        return new ApiResponse<string>("You airline doesn't own this aircraft!") { IsError = true };
                    }

                    if (!string.Equals(aircraft.Name, updateAircraft.Name) && !AirlineController.UserHasPermission(user, AirlinePermission.RenameAircraft))
                    {
                        return new ApiResponse<string>("You don't have the permission to rename airline aircraft!") { IsError = true };
                    }

                    if (aircraft.PurchasePrice != updateAircraft.PurchasePrice && !AirlineController.UserHasPermission(user, AirlinePermission.SellAircraft))
                    {
                        return new ApiResponse<string>("You don't have the permission to sell airline aircraft!") { IsError = true };
                    }

                    if (aircraft.RentPrice != updateAircraft.RentPrice && !AirlineController.UserHasPermission(user, AirlinePermission.RentOutAircraft))
                    {
                        return new ApiResponse<string>("You don't have the permission to rent out airline aircraft!") { IsError = true };
                    }
                }

                if (aircraft.OwnerID != user.Id)
                {
                    return new ApiResponse<string>("You don't own this aircraft!") { IsError = true };
                }

                // Check if the user wants to change the variant
                if (updateAircraft.VariantID != Guid.Empty && updateAircraft.VariantID != aircraft.TypeID)
                {
                    var variants = AircraftTypeController.GetVariants(aircraft.Type);
                    if (variants.All(v => v.ID != updateAircraft.VariantID))
                    {
                        return new ApiResponse<string>("Invalid variant!") { IsError = true };
                    }

                    aircraft.TypeID = updateAircraft.VariantID;
                }

                aircraft.Name = updateAircraft.Name;
                aircraft.PurchasePrice = updateAircraft.PurchasePrice;
                aircraft.RentPrice = updateAircraft.RentPrice;

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving changes to aircraft.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error saving changes to aircraft", saveEx);
                }

                return new ApiResponse<string>($"Successfully updated aircraft {updateAircraft.Registry}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Aircraft/update/{updateAircraft.Registry}");
                return new ApiResponse<string>(ex);
            }
        }
    }
}