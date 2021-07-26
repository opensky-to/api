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
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;

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
        /// -------------------------------------------------------------------------------------------------
        public AircraftController(ILogger<AirportController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
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

                // Only return planes that are available for purchase or rent, or owned by the player
                if (!this.User.IsInRole(UserRoles.Moderator) && !this.User.IsInRole(UserRoles.Admin))
                {
                    if (aircraft.OwnerID != user.Id && !aircraft.PurchasePrice.HasValue && !aircraft.RentPrice.HasValue)
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

                // todo for both queries: filter out planes with an active flight

                if (this.User.IsInRole(UserRoles.Moderator) || this.User.IsInRole(UserRoles.Admin))
                {
                    // Return all planes
                    var aircraft = await this.db.Aircraft.Where(a => a.AirportICAO.Equals(icao)).ToListAsync();
                    return new ApiResponse<IEnumerable<Aircraft>>(aircraft);
                }
                else
                {
                    // Only return planes that are available for purchase or rent, or owned by the player
                    var aircraft = await this.db.Aircraft.Where(a => a.AirportICAO.Equals(icao) && (a.OwnerID == user.Id || a.PurchasePrice.HasValue || a.RentPrice.HasValue)).ToListAsync();
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
        /// <param name="registry">
        /// The registry of the aircraft to purchase.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("purchase/{registry}", Name = "PurchaseAircraft")]
        public async Task<ActionResult<ApiResponse<string>>> PurchaseAircraft(string registry)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Aircraft/purchase/{registry}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                var aircraft = await this.db.Aircraft.SingleOrDefaultAsync(a => a.Registry.Equals(registry));
                if (aircraft == null)
                {
                    return new ApiResponse<string>("Aircraft not found!") { IsError = true };
                }

                if (!aircraft.PurchasePrice.HasValue)
                {
                    return new ApiResponse<string>($"Aircraft with registry {registry} is not for sale!") { IsError = true };
                }

                // todo check if the plane is in flight - has to be on the ground and idle

                // todo check if player has enough money, plus deduct purchase price from account balance

                // todo create some "financial" record for this transaction

                aircraft.OwnerID = user.Id;
                aircraft.PurchasePrice = null;
                aircraft.RentPrice = null;
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error purchasing aircraft");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                // todo ask world populator to "restock" the airport by adding a new plane in place of this one

                return new ApiResponse<string>($"Successfully purchased aircraft {registry}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Aircraft/purchase/{registry}");
                return new ApiResponse<string>(ex);
            }
        }
    }
}