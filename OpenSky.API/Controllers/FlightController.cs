// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightController.cs" company="OpenSky">
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
    using OpenSky.API.Model.Flight;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/10/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Route("[controller]")]
    public class FlightController : ControllerBase
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
        private readonly ILogger<FlightController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserManager<OpenSkyUser> userManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FlightController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/10/2021.
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
        public FlightController(ILogger<FlightController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get flight plans.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/10/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the flight plans for the current user.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("getFlightPlans")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FlightPlan>>>> GetFlightPlans()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Flight/getFlightPlans");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<FlightPlan>>("Unable to find user record!") { IsError = true, Data = new List<FlightPlan>() };
                }

                var plans = await this.db.Flights.Where(f => f.OperatorID == user.Id && !f.Started.HasValue).Select(f => new FlightPlan(f)).ToListAsync();
                return new ApiResponse<IEnumerable<FlightPlan>>(plans);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Flight/getFlightPlans");
                return new ApiResponse<IEnumerable<FlightPlan>>(ex) { Data = new List<FlightPlan>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Delete the specified flight plan.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/10/2021.
        /// </remarks>
        /// <param name="flightID">
        /// Identifier for the flight (plan).
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpDelete("deleteFlightPlan/{flightID}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteFlightPlan(Guid flightID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | DELETE Flight/deleteFlightPlan/{flightID}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                var existingFlight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == flightID);
                if (existingFlight == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find flight plan!", IsError = true };
                }

                if (existingFlight.OperatorID != user.Id)
                {
                    return new ApiResponse<string>("You have no permission to delete this flight plan!") { IsError = true };
                }

                if (existingFlight.Started.HasValue)
                {
                    return new ApiResponse<string>("You can't delete a flight plan once the flight has started!") { IsError = true };
                }

                this.db.Flights.Remove(existingFlight);
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error deleting flight plan");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully deleted flight plan {existingFlight.FlightNumber}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | DELETE Flight/deleteFlightPlan/{flightID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Save a new or existing flight plan.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/10/2021.
        /// </remarks>
        /// <param name="flightPlan">
        /// The flight plan.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("saveFlightPlan")]
        public async Task<ActionResult<ApiResponse<string>>> SaveFlightPlan([FromBody] FlightPlan flightPlan)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Flight/saveFlightPlan");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                if (!string.IsNullOrEmpty(flightPlan.AircraftRegistry))
                {
                    var aircraft = await this.db.Aircraft.SingleOrDefaultAsync(a => a.Registry.Equals(flightPlan.AircraftRegistry));
                    if (aircraft == null)
                    {
                        return new ApiResponse<string>("Aircraft not found!") { IsError = true };
                    }

                    if (aircraft.OwnerID != user.Id) // todo check for VA and rent!
                    {
                        return new ApiResponse<string>("You can only fly planes you or your airline own or rented!") { IsError = true };
                    }

                    if (flightPlan.FuelGallons.HasValue && flightPlan.FuelGallons.Value > aircraft.Type.FuelTotalCapacity)
                    {
                        return new ApiResponse<string>($"The selected aircraft has a maximum total fuel capacity of {aircraft.Type.FuelTotalCapacity} gallons!") { IsError = true };
                    }

                    if (flightPlan.FuelGallons is < 0)
                    {
                        return new ApiResponse<string>("Target fuel amount can't be a negative number!") { IsError = true };
                    }
                }

                if (!string.IsNullOrEmpty(flightPlan.OriginICAO))
                {
                    var airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == flightPlan.OriginICAO);
                    if (airport == null)
                    {
                        return new ApiResponse<string>("Origin airport not found!") { IsError = true };
                    }
                }

                if (!string.IsNullOrEmpty(flightPlan.DestinationICAO))
                {
                    var airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == flightPlan.DestinationICAO);
                    if (airport == null)
                    {
                        return new ApiResponse<string>("Destination airport not found!") { IsError = true };
                    }
                }

                if (!string.IsNullOrEmpty(flightPlan.AlternateICAO))
                {
                    var airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == flightPlan.AlternateICAO);
                    if (airport == null)
                    {
                        return new ApiResponse<string>("Alternate airport not found!") { IsError = true };
                    }
                }

                if (flightPlan.UtcOffset is < 12.0 or > 14.0)
                {
                    return new ApiResponse<string>("UTC offset has to be between -12 and +14 hours!") { IsError = true };
                }

                if (string.IsNullOrEmpty(flightPlan.FlightNumber) || flightPlan.FlightNumber.Length > 7)
                {
                    return new ApiResponse<string>("Flight number is missing or exceeding 7 characters!") { IsError = true };
                }

                var existingFlight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == flightPlan.ID);
                if (existingFlight == null)
                {
                    var newFlight = new Flight
                    {
                        ID = flightPlan.ID,
                        FlightNumber = flightPlan.FlightNumber,
                        AircraftRegistry = flightPlan.AircraftRegistry,
                        OriginICAO = flightPlan.OriginICAO,
                        DestinationICAO = flightPlan.DestinationICAO,
                        AlternateICAO = flightPlan.AlternateICAO,
                        FuelGallons = flightPlan.FuelGallons,
                        UtcOffset = flightPlan.UtcOffset,

                        Created = DateTime.Now,
                        OperatorID = user.Id
                    };
                    await this.db.Flights.AddAsync(newFlight);
                    var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving new flight plan");
                    if (saveEx != null)
                    {
                        throw saveEx;
                    }
                }
                else
                {
                    // todo check va operator
                    if (existingFlight.OperatorID != user.Id)
                    {
                        return new ApiResponse<string>("You have no permission to edit this flight!") { IsError = true };
                    }

                    if (existingFlight.Started.HasValue)
                    {
                        return new ApiResponse<string>("You can't update the flight plan once a flight has started!") { IsError = true };
                    }

                    existingFlight.FlightNumber = flightPlan.FlightNumber;
                    existingFlight.AircraftRegistry = flightPlan.AircraftRegistry;
                    existingFlight.OriginICAO = flightPlan.OriginICAO;
                    existingFlight.DestinationICAO = flightPlan.DestinationICAO;
                    existingFlight.AlternateICAO = flightPlan.AlternateICAO;
                    existingFlight.FuelGallons = flightPlan.FuelGallons;
                    existingFlight.UtcOffset = flightPlan.UtcOffset;
                    var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error updating existing flight plan");
                    if (saveEx != null)
                    {
                        throw saveEx;
                    }
                }

                return new ApiResponse<string>($"Successfully saved flight plan {flightPlan.FlightNumber}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Flight/saveFlightPlan");
                return new ApiResponse<string>(ex);
            }
        }
    }
}