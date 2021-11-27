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
    using OpenSky.API.DbModel.Enums;
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
        /// Abort flight, return to planning stage (with potential penalties depending on flight phase
        /// and location)
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/11/2021.
        /// </remarks>
        /// <param name="flightID">
        /// Identifier for the flight (plan).
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("abort/{flightID:guid}", Name = "AbortFlight")]
        public async Task<ActionResult<ApiResponse<string>>> AbortFlight(Guid flightID)
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | POST Flight/abort/{flightID}");
            try
            {
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                var flight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == flightID);
                if (flight == null)
                {
                    return new ApiResponse<string>("No flight with that ID was found!") { IsError = true };
                }

                // User operated flight, but not the current user?
                if (!string.IsNullOrEmpty(flight.OperatorID) && !flight.OperatorID.Equals(user.Id))
                {
                    return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                }

                // Airline flight, but not the assigned pilot?
                if (!string.IsNullOrEmpty(flight.OperatorAirlineID))
                {
                    if (!flight.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (!flight.AssignedAirlinePilotID.Equals(user.Id))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }
                }

                if (!flight.Started.HasValue)
                {
                    return new ApiResponse<string>("Can't abort flights that haven't started!") { IsError = true };
                }

                if (flight.Completed.HasValue)
                {
                    return new ApiResponse<string>("Can't abort flights that have been completed!") { IsError = true };
                }

                // Check if/what penalties to apply
                if (flight.OnGround)
                {
                    // What's the closes airport to the current location?
                    if (flight.Latitude.HasValue && flight.Longitude.HasValue)
                    {
                        // todo what's the closes airport, move the aircraft there
                        // todo also apply any time-warp to the aircraft

                        flight.Started = null;
                        flight.PayloadLoadingComplete = null;
                        flight.FuelLoadingComplete = null;
                        flight.AutoSaveLog = null;
                        flight.LastAutoSave = null;

                        flight.FlightPhase = FlightPhase.Briefing;
                        flight.Latitude = null; // We can leave the rest of the properties as this is now invalid for resume
                        flight.Longitude = null;
                        flight.LastPositionReport = null;
                        flight.OnGround = true;
                        flight.Altitude = null;
                        flight.GroundSpeed = null;
                        flight.Heading = null;
                    }
                    else
                    {
                        // Flight never report a position, so never started moving, easiest abort, no punishment, simply revert back to plan
                        flight.Started = null;
                        flight.PayloadLoadingComplete = null;
                        flight.FuelLoadingComplete = null;
                        flight.AutoSaveLog = null;
                        flight.LastAutoSave = null;
                        flight.LastPositionReport = null;
                        flight.OnGround = true;
                        flight.Altitude = null;
                        flight.GroundSpeed = null;
                        flight.Heading = null;
                    }
                }
                else
                {
                    // Now it should get expensive, return flight, etc.
                    // todo penalties (lock plane in time-warp, money cost, reputation impact)

                    // Return aircraft back to Origin airport
                    flight.Started = null;
                    flight.PayloadLoadingComplete = null;
                    flight.FuelLoadingComplete = null;
                    flight.AutoSaveLog = null;
                    flight.LastAutoSave = null;

                    flight.FlightPhase = FlightPhase.Briefing;
                    flight.Latitude = null; // We can leave the rest of the properties as this is now invalid for resume
                    flight.Longitude = null;
                    flight.LastPositionReport = null;
                    flight.OnGround = true;
                    flight.Altitude = null;
                    flight.GroundSpeed = null;
                    flight.Heading = null;
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error aborting flight");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully aborted flight {flightID}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Flight/abort/{flightID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Upload final save and complete flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/11/2021.
        /// </remarks>
        /// <param name="finalReport">
        /// The final report.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("complete", Name = "CompleteFlight")]
        public async Task<ActionResult<ApiResponse<string>>> CompleteFlight([FromBody] FinalReport finalReport)
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | POST Flight/complete/{finalReport.FinalPositionReport.ID}");
            try
            {
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                var flight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == finalReport.FinalPositionReport.ID);
                if (flight == null)
                {
                    return new ApiResponse<string>("No flight with that ID was found!") { IsError = true };
                }

                // User operated flight, but not the current user?
                if (!string.IsNullOrEmpty(flight.OperatorID) && !flight.OperatorID.Equals(user.Id))
                {
                    return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                }

                // Airline flight, but not the assigned pilot?
                if (!string.IsNullOrEmpty(flight.OperatorAirlineID))
                {
                    if (!flight.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (!flight.AssignedAirlinePilotID.Equals(user.Id))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }
                }

                if (!flight.Started.HasValue)
                {
                    return new ApiResponse<string>("Can't complete flights that haven't started!") { IsError = true };
                }

                if (flight.Paused.HasValue)
                {
                    return new ApiResponse<string>("Can't complete flights that have been paused!") { IsError = true };
                }

                if (flight.Completed.HasValue)
                {
                    return new ApiResponse<string>("This flight is already completed!") { IsError = true };
                }

                flight.FlightLog = finalReport.FlightLog;
                flight.AutoSaveLog = null;
                flight.Completed = DateTime.UtcNow;

                flight.AirspeedTrue = finalReport.FinalPositionReport.AirspeedTrue;
                flight.Altitude = finalReport.FinalPositionReport.Altitude;
                flight.BankAngle = finalReport.FinalPositionReport.BankAngle;
                flight.FlightPhase = finalReport.FinalPositionReport.FlightPhase;
                flight.GroundSpeed = finalReport.FinalPositionReport.GroundSpeed;
                flight.Heading = finalReport.FinalPositionReport.Heading;
                flight.Latitude = finalReport.FinalPositionReport.Latitude;
                flight.Longitude = finalReport.FinalPositionReport.Longitude;
                flight.OnGround = finalReport.FinalPositionReport.OnGround;
                flight.PitchAngle = finalReport.FinalPositionReport.PitchAngle;
                flight.RadioHeight = finalReport.FinalPositionReport.RadioHeight;
                flight.VerticalSpeedSeconds = finalReport.FinalPositionReport.VerticalSpeedSeconds;
                flight.TimeWarpTimeSavedSeconds = finalReport.FinalPositionReport.TimeWarpTimeSavedSeconds;
                if (flight.TimeWarpTimeSavedSeconds > 0)
                {
                    flight.Aircraft.WarpingUntil = DateTime.UtcNow.AddSeconds(flight.TimeWarpTimeSavedSeconds);
                }

                flight.FuelTankCenterQuantity = finalReport.FinalPositionReport.FuelTankCenterQuantity;
                flight.FuelTankCenter2Quantity = finalReport.FinalPositionReport.FuelTankCenter2Quantity;
                flight.FuelTankCenter3Quantity = finalReport.FinalPositionReport.FuelTankCenter3Quantity;
                flight.FuelTankLeftMainQuantity = finalReport.FinalPositionReport.FuelTankLeftMainQuantity;
                flight.FuelTankLeftAuxQuantity = finalReport.FinalPositionReport.FuelTankLeftAuxQuantity;
                flight.FuelTankLeftTipQuantity = finalReport.FinalPositionReport.FuelTankLeftTipQuantity;
                flight.FuelTankRightMainQuantity = finalReport.FinalPositionReport.FuelTankRightMainQuantity;
                flight.FuelTankRightAuxQuantity = finalReport.FinalPositionReport.FuelTankRightAuxQuantity;
                flight.FuelTankRightTipQuantity = finalReport.FinalPositionReport.FuelTankRightTipQuantity;
                flight.FuelTankExternal1Quantity = finalReport.FinalPositionReport.FuelTankExternal1Quantity;
                flight.FuelTankExternal2Quantity = finalReport.FinalPositionReport.FuelTankExternal2Quantity;

                flight.LastPositionReport = DateTime.UtcNow;

                // We can also delete the navlog fixes now, they aren't being used anymore
                this.db.FlightNavlogFixes.RemoveRange(flight.NavlogFixes);

                // Move aircraft to new location
                if (flight.FlightPhase != FlightPhase.Crashed)
                {
                    if (finalReport.FinalPositionReport.GeoCoordinate.GetDistanceTo(flight.Destination.GeoCoordinate) < 5000)
                    {
                        flight.Aircraft.AirportICAO = flight.DestinationICAO;
                        flight.LandedAtICAO = flight.DestinationICAO;
                    }
                    else if (finalReport.FinalPositionReport.GeoCoordinate.GetDistanceTo(flight.Alternate.GeoCoordinate) < 5000)
                    {
                        flight.Aircraft.AirportICAO = flight.AlternateICAO;
                        flight.LandedAtICAO = flight.AlternateICAO;
                    }
                    else if (finalReport.FinalPositionReport.GeoCoordinate.GetDistanceTo(flight.Origin.GeoCoordinate) < 5000)
                    {
                        flight.Aircraft.AirportICAO = flight.OriginICAO;
                        flight.LandedAtICAO = flight.OriginICAO;
                    }
                    else
                    {
                        // todo check where we are (closest airport)

                    }

                    flight.Aircraft.AirportICAO = flight.DestinationICAO; // todo only for now while testing!
                    flight.LandedAtICAO = flight.DestinationICAO;
                }
                else
                {
                    // Crash, set it back to origin
                    flight.LandedAtICAO = flight.OriginICAO;
                }

                flight.Aircraft.Fuel = finalReport.FinalPositionReport.FuelTankCenterQuantity + finalReport.FinalPositionReport.FuelTankCenter2Quantity + finalReport.FinalPositionReport.FuelTankCenter3Quantity +
                                       finalReport.FinalPositionReport.FuelTankLeftMainQuantity + finalReport.FinalPositionReport.FuelTankLeftAuxQuantity + finalReport.FinalPositionReport.FuelTankLeftTipQuantity +
                                       finalReport.FinalPositionReport.FuelTankRightMainQuantity + finalReport.FinalPositionReport.FuelTankRightAuxQuantity + finalReport.FinalPositionReport.FuelTankRightTipQuantity +
                                       finalReport.FinalPositionReport.FuelTankExternal1Quantity + finalReport.FinalPositionReport.FuelTankExternal2Quantity;

                // todo complete jobs and pay out (if aircraft landed at correct airport)
                // todo calculate wear and tear on the aircraft
                // todo check final log for signs of cheating?
                // todo calculate final reputation/xp/whatever based on flight

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error completing flight");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully completed flight {finalReport.FinalPositionReport.ID}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Flight/complete/{finalReport.FinalPositionReport.ID}");
                return new ApiResponse<string>(ex);
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
        [HttpDelete("flightPlan/{flightID}", Name = "DeleteFlightPlan")]
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

                if (!string.IsNullOrEmpty(existingFlight.OperatorID) && existingFlight.OperatorID != user.Id)
                {
                    return new ApiResponse<string>("You have no permission to delete this flight plan!") { IsError = true };
                }

                if (!string.IsNullOrEmpty(existingFlight.OperatorAirlineID) && (existingFlight.OperatorAirlineID != user.AirlineICAO || !AirlineController.UserHasPermission(user, AirlinePermission.Dispatch)))
                {
                    return new ApiResponse<string>("You have no permission to delete this flight plan!") { IsError = true };
                }

                if (existingFlight.Started.HasValue)
                {
                    return new ApiResponse<string>("You can't delete a flight plan once the flight has started!") { IsError = true };
                }

                var fullFlightNumber = existingFlight.FullFlightNumber;
                this.db.Flights.Remove(existingFlight);
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error deleting flight plan");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully deleted flight plan {fullFlightNumber}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | DELETE Flight/deleteFlightPlan/{flightID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Download flight auto-save.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/11/2021.
        /// </remarks>
        /// <param name="flightID">
        /// Identifier for the flight (plan).
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("autoSave/{flightID:guid}", Name = "DownloadFlightAutoSave")]
        public async Task<ActionResult<ApiResponse<string>>> DownloadFlightAutoSave(Guid flightID)
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | GET Flight/autoSave/{flightID}");
            try
            {
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                var flight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == flightID);
                if (flight == null)
                {
                    return new ApiResponse<string>("No flight with that ID was found!") { IsError = true };
                }

                // User operated flight, but not the current user?
                if (!string.IsNullOrEmpty(flight.OperatorID) && !flight.OperatorID.Equals(user.Id))
                {
                    return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                }

                // Airline flight, but not the assigned pilot?
                if (!string.IsNullOrEmpty(flight.OperatorAirlineID))
                {
                    if (!flight.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (!flight.AssignedAirlinePilotID.Equals(user.Id))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }
                }

                return new ApiResponse<string>(string.Empty) { Data = flight.AutoSaveLog };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Flight/autoSave/{flightID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get currently active flight for tracking.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/11/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the flight or NULL if there is no active flight.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet(Name = "GetFlight")]
        public async Task<ActionResult<ApiResponse<Flight>>> GetFlight()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Flight");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<Flight>("Unable to find user record!") { IsError = true, Data = Flight.ValidEmptyModel };
                }

                var flight = await this.db.Flights.SingleOrDefaultAsync(f => f.OperatorID == user.Id && f.Started.HasValue && !f.Paused.HasValue && !f.Completed.HasValue);
                if (flight != null)
                {
                    return new ApiResponse<Flight>(flight);
                }

                if (!string.IsNullOrEmpty(user.AirlineICAO))
                {
                    flight = await this.db.Flights.SingleOrDefaultAsync(f => f.OperatorAirlineID == user.AirlineICAO && f.AssignedAirlinePilotID == user.Id && f.Started.HasValue && !f.Paused.HasValue && !f.Completed.HasValue);
                }

                return new ApiResponse<Flight>(flight ?? Flight.ValidEmptyModel);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Flight");
                return new ApiResponse<Flight>(ex) { Data = Flight.ValidEmptyModel };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight log details (final log xml file and ofp)
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/11/2021.
        /// </remarks>
        /// <param name="flightID">
        /// Identifier for the flight.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the flight log details.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("flightLogDetails/{flightID:guid}", Name = "GetFlightLogDetails")]
        public async Task<ActionResult<ApiResponse<FlightLogDetails>>> GetFlightLogDetails(Guid flightID)
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | GET Flight/flightLogDetails/{flightID}");
            try
            {
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<FlightLogDetails>("Unable to find user record!") { IsError = true };
                }

                var flight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == flightID);
                if (flight == null)
                {
                    return new ApiResponse<FlightLogDetails>("No flight with that ID was found!") { IsError = true };
                }

                // User operated flight, but not the current user?
                if (!string.IsNullOrEmpty(flight.OperatorID) && !flight.OperatorID.Equals(user.Id))
                {
                    return new ApiResponse<FlightLogDetails>("Unauthorized request!") { IsError = true };
                }

                // Airline flight, but not the assigned pilot?
                if (!string.IsNullOrEmpty(flight.OperatorAirlineID))
                {
                    if (!flight.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<FlightLogDetails>("Unauthorized request!") { IsError = true };
                    }

                    if (!flight.AssignedAirlinePilotID.Equals(user.Id))
                    {
                        return new ApiResponse<FlightLogDetails>("Unauthorized request!") { IsError = true };
                    }
                }

                if (!flight.Completed.HasValue)
                {
                    return new ApiResponse<FlightLogDetails>("Flight not completed!") { IsError = true };
                }

                return new ApiResponse<FlightLogDetails>(new FlightLogDetails { FlightLog = flight.FlightLog, OfpHtml = flight.OfpHtml });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Flight/flightLogDetails/{flightID}");
                return new ApiResponse<FlightLogDetails>(ex);
            }
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
        [HttpGet("flightPlans", Name = "GetFlightPlans")]
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

                var plans = await this.db.Flights.Where(f => f.OperatorID == user.Id && !f.Started.HasValue).ToListAsync();

                if (!string.IsNullOrEmpty(user.AirlineICAO))
                {
                    if (AirlineController.UserHasPermission(user, AirlinePermission.Dispatch))
                    {
                        plans.AddRange(await this.db.Flights.Where(f => f.OperatorAirlineID == user.AirlineICAO && !f.Started.HasValue).ToListAsync());
                    }
                    else
                    {
                        plans.AddRange(await this.db.Flights.Where(f => f.OperatorAirlineID == user.AirlineICAO && f.AssignedAirlinePilotID == user.Id && !f.Started.HasValue).ToListAsync());
                    }
                }

                return new ApiResponse<IEnumerable<FlightPlan>>(plans.Select(f => new FlightPlan(f)));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Flight/getFlightPlans");
                return new ApiResponse<IEnumerable<FlightPlan>>(ex) { Data = new List<FlightPlan>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get flight logs (completed flights)
        /// </summary>
        /// <remarks>
        /// sushi.at, 15/11/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields my flight logs.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("myFlightLogs", Name = "GetMyFlightLogs")]
        public async Task<ActionResult<ApiResponse<IEnumerable<FlightLog>>>> GetMyFlightLogs()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Flight/myFlightLogs");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<FlightLog>>("Unable to find user record!") { IsError = true, Data = new List<FlightLog>() };
                }

                var flights = await this.db.Flights.Where(f => f.OperatorID == user.Id && f.Completed.HasValue).ToListAsync();

                if (!string.IsNullOrEmpty(user.AirlineICAO))
                {
                    flights.AddRange(await this.db.Flights.Where(f => f.OperatorAirlineID == user.AirlineICAO && f.AssignedAirlinePilotID == user.Id && f.Completed.HasValue).ToListAsync());
                }

                return new ApiResponse<IEnumerable<FlightLog>>(flights.OrderByDescending(f => f.Completed).Take(50).Select(f => new FlightLog(f, this.userManager)));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Flight/myFlightLogs");
                return new ApiResponse<IEnumerable<FlightLog>>(ex) { Data = new List<FlightLog>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get active flights (up to one currently flying and possibly multiple paused).
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/11/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the flights.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("myFlights", Name = "GetMyFlights")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Flight>>>> GetMyFlights()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Flight/myFlights");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<Flight>>("Unable to find user record!") { IsError = true, Data = new List<Flight>() };
                }

                var flights = await this.db.Flights.Where(f => f.OperatorID == user.Id && f.Started.HasValue && !f.Completed.HasValue).ToListAsync();

                if (!string.IsNullOrEmpty(user.AirlineICAO))
                {
                    flights.AddRange(await this.db.Flights.Where(f => f.OperatorAirlineID == user.AirlineICAO && f.AssignedAirlinePilotID == user.Id && f.Started.HasValue && !f.Completed.HasValue).ToListAsync());
                }

                return new ApiResponse<IEnumerable<Flight>>(flights);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Flight/myFlights");
                return new ApiResponse<IEnumerable<Flight>>(ex) { Data = new List<Flight>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get flights for the world map.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/11/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the world map flights.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("worldMap", Name = "GetWorldMapFlights")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WorldMapFlight>>>> GetWorldMapFlights()
        {
            // todo Decide in the future if certain user roles are required to see all flights and filter the rest to airline/personal flights? Or should the world map always be busy?

            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Flight/worldMap");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<WorldMapFlight>>("Unable to find user record!") { IsError = true, Data = new List<WorldMapFlight>() };
                }

                var flights = await this.db.Flights.Where(f => f.Started.HasValue && !f.Completed.HasValue).ToListAsync();
                return new ApiResponse<IEnumerable<WorldMapFlight>>(flights.Select(f => new WorldMapFlight(f, this.userManager)));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Flight/worldMap");
                return new ApiResponse<IEnumerable<WorldMapFlight>>(ex) { Data = new List<WorldMapFlight>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Pause the flight with the specified ID, does not save position or save file - upload these
        /// before calling pause if they should be preserved.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/11/2021.
        /// </remarks>
        /// <param name="flightID">
        /// Identifier for the flight.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("pause/{flightID:guid}", Name = "PauseFlight")]
        public async Task<ActionResult<ApiResponse<string>>> PauseFlight(Guid flightID)
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | POST Flight/pause/{flightID}");
            try
            {
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                var flight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == flightID);
                if (flight == null)
                {
                    return new ApiResponse<string>("No flight with that ID was found!") { IsError = true };
                }

                // User operated flight, but not the current user?
                if (!string.IsNullOrEmpty(flight.OperatorID) && !flight.OperatorID.Equals(user.Id))
                {
                    return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                }

                // Airline flight, but not the assigned pilot?
                if (!string.IsNullOrEmpty(flight.OperatorAirlineID))
                {
                    if (!flight.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (!flight.AssignedAirlinePilotID.Equals(user.Id))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }
                }

                if (!flight.Started.HasValue)
                {
                    return new ApiResponse<string>("Can't pause flights that haven't started!") { IsError = true };
                }

                if (flight.Completed.HasValue)
                {
                    return new ApiResponse<string>("Can't pause flights that have been completed!") { IsError = true };
                }

                if (flight.Paused.HasValue)
                {
                    return new ApiResponse<string>("Flight is already paused!") { IsError = true };
                }

                flight.Paused = DateTime.UtcNow;

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error pausing flight");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully paused flight {flightID}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Flight/pause/{flightID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Flight position report.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/11/2021.
        /// </remarks>
        /// <param name="report">
        /// The position report.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("posReport", Name = "PositionReport")]
        public async Task<ActionResult<ApiResponse<string>>> PositionReport([FromBody] PositionReport report)
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | POST Flight/posReport:{report.ID}");
            try
            {
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                var flight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == report.ID);
                if (flight == null)
                {
                    return new ApiResponse<string>("No flight with that ID was found!") { IsError = true };
                }

                // User operated flight, but not the current user?
                if (!string.IsNullOrEmpty(flight.OperatorID) && !flight.OperatorID.Equals(user.Id))
                {
                    return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                }

                // Airline flight, but not the assigned pilot?
                if (!string.IsNullOrEmpty(flight.OperatorAirlineID))
                {
                    if (!flight.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (!flight.AssignedAirlinePilotID.Equals(user.Id))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }
                }

                if (!flight.Started.HasValue)
                {
                    return new ApiResponse<string>("Can't add position reports to flights that haven't started!") { IsError = true };
                }

                // Allow position report in the first minute of pausing, to make sure we get everything saved for the user
                if (flight.Paused.HasValue && (DateTime.UtcNow - flight.Paused.Value).TotalMinutes > 1)
                {
                    return new ApiResponse<string>("Can't add position reports to flights that have been paused!") { IsError = true };
                }

                if (flight.Completed.HasValue)
                {
                    return new ApiResponse<string>("Can't add position reports to flights that have been completed!") { IsError = true };
                }

                flight.AirspeedTrue = report.AirspeedTrue;
                flight.Altitude = report.Altitude;
                flight.BankAngle = report.BankAngle;
                flight.FlightPhase = report.FlightPhase;
                flight.GroundSpeed = report.GroundSpeed;
                flight.Heading = report.Heading;
                flight.Latitude = report.Latitude;
                flight.Longitude = report.Longitude;
                flight.OnGround = report.OnGround;
                flight.PitchAngle = report.PitchAngle;
                flight.RadioHeight = report.RadioHeight;
                flight.VerticalSpeedSeconds = report.VerticalSpeedSeconds;
                flight.TimeWarpTimeSavedSeconds = report.TimeWarpTimeSavedSeconds;

                flight.FuelTankCenterQuantity = report.FuelTankCenterQuantity;
                flight.FuelTankCenter2Quantity = report.FuelTankCenter2Quantity;
                flight.FuelTankCenter3Quantity = report.FuelTankCenter3Quantity;
                flight.FuelTankLeftMainQuantity = report.FuelTankLeftMainQuantity;
                flight.FuelTankLeftAuxQuantity = report.FuelTankLeftAuxQuantity;
                flight.FuelTankLeftTipQuantity = report.FuelTankLeftTipQuantity;
                flight.FuelTankRightMainQuantity = report.FuelTankRightMainQuantity;
                flight.FuelTankRightAuxQuantity = report.FuelTankRightAuxQuantity;
                flight.FuelTankRightTipQuantity = report.FuelTankRightTipQuantity;
                flight.FuelTankExternal1Quantity = report.FuelTankExternal1Quantity;
                flight.FuelTankExternal2Quantity = report.FuelTankExternal2Quantity;

                flight.LastPositionReport = DateTime.UtcNow;

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving flight position report");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully saved position report for {report.ID}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Flight/posReport:{report.ID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resume the flight with the specified ID, only works if there is no other active flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/11/2021.
        /// </remarks>
        /// <param name="flightID">
        /// Identifier for the flight.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("resume/{flightID:guid}", Name = "ResumeFlight")]
        public async Task<ActionResult<ApiResponse<string>>> ResumeFlight(Guid flightID)
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | POST Flight/resume/{flightID}");
            try
            {
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                var flight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == flightID);
                if (flight == null)
                {
                    return new ApiResponse<string>("No flight with that ID was found!") { IsError = true };
                }

                // User operated flight, but not the current user?
                if (!string.IsNullOrEmpty(flight.OperatorID) && !flight.OperatorID.Equals(user.Id))
                {
                    return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                }

                // Airline flight, but not the assigned pilot?
                if (!string.IsNullOrEmpty(flight.OperatorAirlineID))
                {
                    if (!flight.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (!flight.AssignedAirlinePilotID.Equals(user.Id))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }
                }

                if (!flight.Started.HasValue)
                {
                    return new ApiResponse<string>("Can't resume flights that haven't started!") { IsError = true };
                }

                if (flight.Completed.HasValue)
                {
                    return new ApiResponse<string>("Can't resume flights that have been completed!") { IsError = true };
                }

                if (!flight.Paused.HasValue)
                {
                    return new ApiResponse<string>("Flight is not paused!") { IsError = true };
                }

                // Another flight in progress?
                var otherFlightInProgress = await this.db.Flights.AnyAsync(f => (f.OperatorID == user.Id || f.AssignedAirlinePilotID == user.Id) && f.Started.HasValue && !(f.Paused.HasValue || f.Completed.HasValue));
                if (otherFlightInProgress)
                {
                    return new ApiResponse<string>("You already have another flight in progress! Please complete or pause the other flight before resuming this one.") { IsError = true };
                }

                flight.Paused = null;

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error resuming flight");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully resumed flight {flightID}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Flight/resume/{flightID}");
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
        [HttpPost("flightPlan", Name = "SaveFlightPlan")]
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

                if (flightPlan.Aircraft != null)
                {
                    var aircraft = await this.db.Aircraft.SingleOrDefaultAsync(a => a.Registry.Equals(flightPlan.Aircraft.Registry));
                    if (aircraft == null)
                    {
                        return new ApiResponse<string>("Aircraft not found!") { IsError = true };
                    }

                    if (aircraft.OwnerID != user.Id && aircraft.AirlineOwnerID != user.AirlineICAO) // todo check for rent!
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

                if (flightPlan.UtcOffset is < -12.0 or > 14.0)
                {
                    return new ApiResponse<string>("UTC offset has to be between -12 and +14 hours!") { IsError = true };
                }

                if (flightPlan.FlightNumber is < 1 or > 9999)
                {
                    return new ApiResponse<string>("Flight number is out of range (1-9999)!") { IsError = true };
                }

                var existingFlight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == flightPlan.ID);
                if (existingFlight == null)
                {
                    var newFlight = new Flight
                    {
                        ID = flightPlan.ID,
                        FlightNumber = flightPlan.FlightNumber,
                        AircraftRegistry = flightPlan.Aircraft?.Registry,
                        OriginICAO = flightPlan.OriginICAO,
                        DestinationICAO = flightPlan.DestinationICAO,
                        AlternateICAO = flightPlan.AlternateICAO,
                        FuelGallons = flightPlan.FuelGallons,
                        UtcOffset = flightPlan.UtcOffset,
                        DispatcherID = user.Id,
                        DispatcherRemarks = flightPlan.DispatcherRemarks,
                        PlannedDepartureTime = flightPlan.PlannedDepartureTime,
                        Route = flightPlan.Route,
                        AlternateRoute = flightPlan.AlternateRoute,
                        OfpHtml = flightPlan.OfpHtml,

                        OnGround = true,
                        Created = DateTime.UtcNow,
                    };

                    if (flightPlan.IsAirlineFlight)
                    {
                        if (string.IsNullOrEmpty(user.AirlineICAO))
                        {
                            return new ApiResponse<string>("Can't plan airline flights without being a member of one!") { IsError = true };
                        }

                        newFlight.OperatorAirlineID = user.AirlineICAO;
                    }
                    else
                    {
                        newFlight.OperatorID = user.Id;
                    }

                    await this.db.Flights.AddAsync(newFlight);
                    await this.db.FlightNavlogFixes.AddRangeAsync(flightPlan.NavlogFixes);
                    var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving new flight plan");
                    if (saveEx != null)
                    {
                        throw saveEx;
                    }
                }
                else
                {
                    if (!flightPlan.IsAirlineFlight && existingFlight.OperatorID != null && existingFlight.OperatorID != user.Id)
                    {
                        return new ApiResponse<string>("You have no permission to edit this flight!") { IsError = true };
                    }

                    if (flightPlan.IsAirlineFlight && existingFlight.OperatorAirlineID != null && (existingFlight.OperatorAirlineID != user.AirlineICAO || !AirlineController.UserHasPermission(user, AirlinePermission.Dispatch)))
                    {
                        return new ApiResponse<string>("You have no permission to edit this flight!") { IsError = true };
                    }

                    if (existingFlight.Started.HasValue)
                    {
                        return new ApiResponse<string>("You can't update the flight plan once a flight has started!") { IsError = true };
                    }

                    existingFlight.FlightNumber = flightPlan.FlightNumber;
                    existingFlight.AircraftRegistry = flightPlan.Aircraft?.Registry;
                    existingFlight.OriginICAO = flightPlan.OriginICAO;
                    existingFlight.DestinationICAO = flightPlan.DestinationICAO;
                    existingFlight.AlternateICAO = flightPlan.AlternateICAO;
                    existingFlight.FuelGallons = flightPlan.FuelGallons;
                    existingFlight.UtcOffset = flightPlan.UtcOffset;
                    existingFlight.DispatcherID = user.Id;
                    existingFlight.DispatcherRemarks = flightPlan.DispatcherRemarks;
                    existingFlight.PlannedDepartureTime = flightPlan.PlannedDepartureTime;
                    existingFlight.Route = flightPlan.Route;
                    existingFlight.AlternateRoute = flightPlan.AlternateRoute;
                    existingFlight.OfpHtml = flightPlan.OfpHtml;

                    this.db.FlightNavlogFixes.RemoveRange(existingFlight.NavlogFixes);
                    await this.db.FlightNavlogFixes.AddRangeAsync(flightPlan.NavlogFixes);

                    // Set these two anyway, in case flight was changed between private and airline flight
                    if (!flightPlan.IsAirlineFlight)
                    {
                        existingFlight.OperatorID = user.Id;
                        existingFlight.OperatorAirlineID = null;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(user.AirlineICAO))
                        {
                            return new ApiResponse<string>("Can't plan airline flights without being a member of one!") { IsError = true };
                        }

                        existingFlight.OperatorID = null;
                        existingFlight.OperatorAirlineID = user.AirlineICAO;
                    }

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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Start flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/11/2021.
        /// </remarks>
        /// <param name="flightID">
        /// Identifier for the flight plan to start.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("start/{flightID:guid}", Name = "StartFlight")]
        public async Task<ActionResult<ApiResponse<string>>> StartFlight(Guid flightID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Flight/startFlight/{flightID}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                var plan = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == flightID);
                if (plan == null)
                {
                    return new ApiResponse<string>("No flight plan with that ID was found!") { IsError = true };
                }

                // User operated flight, but not the current user?
                if (!string.IsNullOrEmpty(plan.OperatorID) && !plan.OperatorID.Equals(user.Id))
                {
                    return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                }

                // Airline flight, but not the assigned pilot?
                if (!string.IsNullOrEmpty(plan.OperatorAirlineID))
                {
                    if (!plan.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (string.IsNullOrEmpty(plan.AssignedAirlinePilotID))
                    {
                        return new ApiResponse<string>("This airline flight plan has no assigned pilot!") { IsError = true };
                    }

                    if (!plan.AssignedAirlinePilotID.Equals(user.Id))
                    {
                        return new ApiResponse<string>("This flight is assigned to another pilot in your airline!") { IsError = true };
                    }
                }

                // Origin or destination missing?
                if (string.IsNullOrEmpty(plan.OriginICAO))
                {
                    return new ApiResponse<string>("Flight plan has no origin airport!") { IsError = true };
                }

                if (string.IsNullOrEmpty(plan.DestinationICAO))
                {
                    return new ApiResponse<string>("Flight plan has no destination airport!") { IsError = true };
                }

                // Aircraft not selected?
                if (string.IsNullOrEmpty(plan.AircraftRegistry))
                {
                    return new ApiResponse<string>("Flight plan has no assigned aircraft!") { IsError = true };
                }

                // Invalid UTC offset
                if (plan.UtcOffset is < -12.0 or > 14.0)
                {
                    return new ApiResponse<string>("UTC offset has to be between -12 and +14 hours!") { IsError = true };
                }

                // Invalid flight number
                if (plan.FlightNumber is < 1 or > 9999)
                {
                    return new ApiResponse<string>("Flight number is out of range (1-9999)!") { IsError = true };
                }

                // No fuel
                if (!plan.FuelGallons.HasValue)
                {
                    return new ApiResponse<string>("Flight plan has no fuel value!") { IsError = true };
                }

                // Has the flight started already?
                if (plan.Started.HasValue)
                {
                    return new ApiResponse<string>("Can't start flight already in progress!") { IsError = true };
                }

                // Another flight in progress?
                var otherFlightInProgress = await this.db.Flights.AnyAsync(f => (f.OperatorID == user.Id || f.AssignedAirlinePilotID == user.Id) && f.Started.HasValue && !(f.Paused.HasValue || f.Completed.HasValue));
                if (otherFlightInProgress)
                {
                    return new ApiResponse<string>("You already have another flight in progress! Please complete or pause the other flight before starting a new one.") { IsError = true };
                }

                // Is the aircraft at the origin airport and idle?
                if (!plan.OriginICAO.Equals(plan.Aircraft?.AirportICAO))
                {
                    return new ApiResponse<string>("The selected aircraft is not at the departure airport!") { IsError = true, Data = "AircraftNotAtOrigin" };
                }

                if (plan.Aircraft?.Status != "Idle")
                {
                    return new ApiResponse<string>("The selected aircraft must be idle!") { IsError = true };
                }

                // All checks passed, start the flight and calculate the payload and fuel loading times
                plan.Started = DateTime.UtcNow;

                var gallonsPerMinute = plan.Aircraft.Type.FuelType switch
                {
                    FuelType.JetFuel => 500,
                    FuelType.AvGas => 8,
                    FuelType.None => 0,
                    _ => 0.0
                };
                var gallonsToTransfer = Math.Abs(plan.Aircraft.Fuel - plan.FuelGallons.Value);
                plan.FuelLoadingComplete = gallonsPerMinute > 0 && gallonsToTransfer > 0 ? DateTime.UtcNow.AddMinutes(3 + gallonsToTransfer / gallonsPerMinute) : DateTime.UtcNow;

                // todo add payload calculation once we have that
                plan.PayloadLoadingComplete = DateTime.UtcNow;

                // No alternate set?, set to origin (return to base)
                if (string.IsNullOrEmpty(plan.AlternateICAO))
                {
                    plan.AlternateICAO = plan.OriginICAO;
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, $"Error starting flight {flightID}.");
                if (saveEx != null)
                {
                    return new ApiResponse<string>("Error starting flight.", saveEx);
                }

                return new ApiResponse<string>($"Flight {plan.FullFlightNumber} started successfully, remember to keep the rubber on the runway and your troubles on the ground!");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Flight/startFlight/{flightID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Upload auto-save flight log for the specified flight.
        /// </summary>
        /// <remarks>
        /// sushi.at, 13/11/2021.
        /// </remarks>
        /// <param name="flightID">
        /// Identifier for the flight.
        /// </param>
        /// <param name="autoSave">
        /// The auto-save (base64 encoded).
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("autoSave/{flightID:guid}", Name = "UploadFlightAutoSave")]
        public async Task<ActionResult<ApiResponse<string>>> UploadFlightAutoSave(Guid flightID, [FromBody] string autoSave)
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | POST Flight/autoSave/{flightID}");
            try
            {
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                var flight = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == flightID);
                if (flight == null)
                {
                    return new ApiResponse<string>("No flight with that ID was found!") { IsError = true };
                }

                // User operated flight, but not the current user?
                if (!string.IsNullOrEmpty(flight.OperatorID) && !flight.OperatorID.Equals(user.Id))
                {
                    return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                }

                // Airline flight, but not the assigned pilot?
                if (!string.IsNullOrEmpty(flight.OperatorAirlineID))
                {
                    if (!flight.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (!flight.AssignedAirlinePilotID.Equals(user.Id))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }
                }

                if (!flight.Started.HasValue)
                {
                    return new ApiResponse<string>("Can't auto-save flights that haven't started!") { IsError = true };
                }

                // Allow auto-save in the first minute of pausing, to make sure we get everything saved for the user
                if (flight.Paused.HasValue && (DateTime.UtcNow - flight.Paused.Value).TotalMinutes > 1)
                {
                    return new ApiResponse<string>("Can't auto-save flights that have been paused!") { IsError = true };
                }

                if (flight.Completed.HasValue)
                {
                    return new ApiResponse<string>("Can't auto-save flights that have been completed!") { IsError = true };
                }

                flight.AutoSaveLog = autoSave;
                flight.LastAutoSave = DateTime.UtcNow;

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error saving flight auto-save");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully auto-saved flight {flightID}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Flight/autoSave/{flightID}");
                return new ApiResponse<string>(ex);
            }
        }
    }
}