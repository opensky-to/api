// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightController.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;

    using GeoCoordinatePortable;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;
    using OpenSky.API.Model.Flight;
    using OpenSky.API.Services;
    using OpenSky.API.Services.Models;
    using OpenSky.S2Geometry.Extensions;

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
        /// The statistics service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly StatisticsService statisticsService;

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
        /// <param name="statisticsService">
        /// The statistics service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public FlightController(ILogger<FlightController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager, StatisticsService statisticsService)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.statisticsService = statisticsService;
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

                // Are there any fuelling/loading times left on the flight that need to be transferred back to the aircraft?
                if (flight.FuelLoadingComplete.HasValue && flight.FuelLoadingComplete.Value > DateTime.UtcNow)
                {
                    if (flight.Aircraft.FuellingUntil.HasValue && flight.Aircraft.FuellingUntil.Value > DateTime.UtcNow)
                    {
                        flight.Aircraft.FuellingUntil += flight.FuelLoadingComplete.Value - DateTime.UtcNow;
                    }
                    else
                    {
                        flight.Aircraft.FuellingUntil = flight.FuelLoadingComplete.Value;
                    }
                }

                if (flight.PayloadLoadingComplete.HasValue && flight.PayloadLoadingComplete.Value > DateTime.UtcNow)
                {
                    if (flight.Aircraft.LoadingUntil.HasValue && flight.Aircraft.LoadingUntil.Value > DateTime.UtcNow)
                    {
                        flight.Aircraft.LoadingUntil += flight.PayloadLoadingComplete.Value - DateTime.UtcNow;
                    }
                    else
                    {
                        flight.Aircraft.LoadingUntil = flight.PayloadLoadingComplete.Value;
                    }
                }

                // Check if/what penalties to apply
                if (flight.OnGround)
                {
                    // What's the closest airport to the current location?
                    if (flight.Latitude.HasValue && flight.Longitude.HasValue)
                    {
                        // Check where we are (closest airport)
                        var closestAirportICAO = await this.GetClosestAirport(new GeoCoordinate(flight.Latitude.Value, flight.Longitude.Value), flight.Aircraft.Type.Simulator);
                        if (string.IsNullOrEmpty(closestAirportICAO))
                        {
                            // Landed at no airport, back to origin!
                            flight.Aircraft.AirportICAO = flight.OriginICAO;
                            flight.LandedAtICAO = flight.OriginICAO;
                        }
                        else
                        {
                            flight.Aircraft.AirportICAO = closestAirportICAO;
                            flight.LandedAtICAO = closestAirportICAO;
                        }

                        if (flight.TimeWarpTimeSavedSeconds > 0)
                        {
                            flight.Aircraft.WarpingUntil = DateTime.UtcNow.AddSeconds(flight.TimeWarpTimeSavedSeconds);
                        }

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
                    // todo penalties (money cost, fuel consumption?, reputation impact)

                    var distanceToOrigin = 0d;
                    if (flight.Latitude.HasValue && flight.Longitude.HasValue)
                    {
                        // Convert meters to nautical miles, before dividing by knots
                        distanceToOrigin = flight.Origin.GeoCoordinate.GetDistanceTo(new GeoCoordinate(flight.Latitude.Value, flight.Longitude.Value)) / 1852d;
                    }

                    var groundSpeed = flight.Aircraft.Type.EngineType switch
                    {
                        EngineType.HeloBellTurbine => 150,
                        EngineType.Jet => 400,
                        EngineType.Piston => 100,
                        EngineType.Turboprop => 250,
                        EngineType.None => 0,
                        EngineType.Unsupported => 0,
                        _ => 0
                    };
                    var returnFlightDuration = (distanceToOrigin > 0 && groundSpeed > 0) ? TimeSpan.FromHours(distanceToOrigin / groundSpeed) : DateTime.UtcNow - flight.Started.Value;
                    flight.Aircraft.WarpingUntil = flight.TimeWarpTimeSavedSeconds > 0 ? DateTime.UtcNow.AddSeconds(flight.TimeWarpTimeSavedSeconds).Add(returnFlightDuration) : DateTime.UtcNow.Add(returnFlightDuration);

                    // Increase airframe and engine hours
                    var hours = (DateTime.UtcNow - flight.Started.Value).TotalHours + returnFlightDuration.TotalHours;
                    flight.Aircraft.AirframeHours += hours;
                    switch (flight.Aircraft.Type.EngineCount)
                    {
                        case 1:
                            flight.Aircraft.Engine1Hours += hours;
                            break;
                        case 2:
                            flight.Aircraft.Engine1Hours += hours;
                            flight.Aircraft.Engine2Hours += hours;
                            break;
                        case 3:
                            flight.Aircraft.Engine1Hours += hours;
                            flight.Aircraft.Engine2Hours += hours;
                            flight.Aircraft.Engine3Hours += hours;
                            break;
                        case 4:
                            flight.Aircraft.Engine1Hours += hours;
                            flight.Aircraft.Engine2Hours += hours;
                            flight.Aircraft.Engine3Hours += hours;
                            flight.Aircraft.Engine4Hours += hours;
                            break;
                    }

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
                        // Check where we are (closest airport)
                        var closestAirportICAO = await this.GetClosestAirport(finalReport.FinalPositionReport.GeoCoordinate, flight.Aircraft.Type.Simulator);
                        if (string.IsNullOrEmpty(closestAirportICAO))
                        {
                            // Landed at no airport, back to origin!
                            flight.Aircraft.AirportICAO = flight.OriginICAO;
                            flight.LandedAtICAO = flight.OriginICAO;
                        }
                        else
                        {
                            flight.Aircraft.AirportICAO = closestAirportICAO;
                            flight.LandedAtICAO = closestAirportICAO;
                        }
                    }
                }
                else
                {
                    // Crash, set it back to origin
                    flight.LandedAtICAO = flight.OriginICAO;

                    // todo determine the severity and calculate the impact on the aircraft status
                }

                // Sum up final fuel values and update aircraft
                flight.Aircraft.Fuel = finalReport.FinalPositionReport.FuelTankCenterQuantity + finalReport.FinalPositionReport.FuelTankCenter2Quantity + finalReport.FinalPositionReport.FuelTankCenter3Quantity +
                                       finalReport.FinalPositionReport.FuelTankLeftMainQuantity + finalReport.FinalPositionReport.FuelTankLeftAuxQuantity + finalReport.FinalPositionReport.FuelTankLeftTipQuantity +
                                       finalReport.FinalPositionReport.FuelTankRightMainQuantity + finalReport.FinalPositionReport.FuelTankRightAuxQuantity + finalReport.FinalPositionReport.FuelTankRightTipQuantity +
                                       finalReport.FinalPositionReport.FuelTankExternal1Quantity + finalReport.FinalPositionReport.FuelTankExternal2Quantity;

                // Increase airframe and engine hours
                var hours = (DateTime.UtcNow - flight.Started.Value).TotalHours;
                flight.Aircraft.AirframeHours += hours;
                switch (flight.Aircraft.Type.EngineCount)
                {
                    case 1:
                        flight.Aircraft.Engine1Hours += hours;
                        break;
                    case 2:
                        flight.Aircraft.Engine1Hours += hours;
                        flight.Aircraft.Engine2Hours += hours;
                        break;
                    case 3:
                        flight.Aircraft.Engine1Hours += hours;
                        flight.Aircraft.Engine2Hours += hours;
                        flight.Aircraft.Engine3Hours += hours;
                        break;
                    case 4:
                        flight.Aircraft.Engine1Hours += hours;
                        flight.Aircraft.Engine2Hours += hours;
                        flight.Aircraft.Engine3Hours += hours;
                        flight.Aircraft.Engine4Hours += hours;
                        break;
                }

                // Create the top-level financial record for this flight that will sum up all income and expenses
                var flightFinancialRecord = new FinancialRecord
                {
                    ID = Guid.NewGuid(),
                    Description = $"Flight {flight.FullFlightNumber} with aircraft {flight.AircraftRegistry.RemoveSimPrefix()} from {flight.OriginICAO} to {flight.LandedAtICAO}",
                    Timestamp = DateTime.UtcNow,
                    AircraftRegistry = flight.AircraftRegistry,
                    Category = FinancialCategory.Flight
                };
                if (!string.IsNullOrEmpty(flight.OperatorID))
                {
                    flightFinancialRecord.UserID = flight.OperatorID;
                }
                else
                {
                    flightFinancialRecord.AirlineID = flight.OperatorAirlineID;
                }

                await this.db.FinancialRecords.AddAsync(flightFinancialRecord);

                // Did any payloads reach their destination?
                if (!string.IsNullOrEmpty(flight.LandedAtICAO))
                {
                    // Any changes to these figures need to be mirrored in the AircraftController.PerformGroundOperations and FlightController.StartFlight method
                    var lbsPerMinute = flight.Aircraft.Type.Category switch
                    {
                        AircraftTypeCategory.SEP => 225,
                        AircraftTypeCategory.MEP => 225,
                        AircraftTypeCategory.SET => 225,
                        AircraftTypeCategory.MET => 350,
                        AircraftTypeCategory.JET => 350,
                        AircraftTypeCategory.REG => 1500,
                        AircraftTypeCategory.NBA => 2000,
                        AircraftTypeCategory.WBA => 3300,
                        AircraftTypeCategory.HEL => 350,
                        _ => 0.0
                    };
                    var lbsToTransfer = 0.0;
                    var jobIDsToCheck = new HashSet<Guid>();
                    var payloadsArrived = new List<Guid>();

                    // Check for arriving flight payloads
                    foreach (var flightPayload in flight.FlightPayloads)
                    {
                        if (flightPayload.Payload.DestinationICAO == flight.LandedAtICAO && flightPayload.Payload.AircraftRegistry == flight.Aircraft.Registry)
                        {
                            lbsToTransfer += flightPayload.Payload.Weight;
                            flightPayload.Payload.AirportICAO = flight.LandedAtICAO;
                            flightPayload.Payload.AircraftRegistry = null;
                            jobIDsToCheck.Add(flightPayload.Payload.JobID);
                            payloadsArrived.Add(flightPayload.PayloadID);
                        }
                    }

                    // Check for arriving aircraft payloads, not regarding of flight planning
                    foreach (var aircraftPayload in flight.Aircraft.Payloads)
                    {
                        if (aircraftPayload.AirportICAO == flight.LandedAtICAO && aircraftPayload.AircraftRegistry == flight.Aircraft.Registry)
                        {
                            lbsToTransfer += aircraftPayload.Weight;
                            aircraftPayload.AirportICAO = flight.LandedAtICAO;
                            aircraftPayload.AircraftRegistry = null;
                            jobIDsToCheck.Add(aircraftPayload.JobID);
                            payloadsArrived.Add(aircraftPayload.ID);
                        }
                    }

                    if (lbsPerMinute > 0 && lbsToTransfer > 0)
                    {
                        // Start payload loading timer (starts after warp is complete)
                        flight.Aircraft.LoadingUntil = DateTime.UtcNow.AddSeconds(flight.TimeWarpTimeSavedSeconds).AddMinutes(1 + (lbsToTransfer / lbsPerMinute));
                    }

                    // Check for completed jobs
                    foreach (var jobID in jobIDsToCheck)
                    {
                        var job = await this.db.Jobs.SingleOrDefaultAsync(j => j.ID == jobID);
                        if (job != null)
                        {
                            var allPayloadsArrived = true;
                            foreach (var payload in job.Payloads)
                            {
                                if (payload.AirportICAO != payload.DestinationICAO && !payloadsArrived.Contains(payload.ID))
                                {
                                    allPayloadsArrived = false;
                                }
                            }

                            if (allPayloadsArrived)
                            {
                                // Job complete, pay out the money and then delete it
                                var latePenaltyMultiplier = 1.0;
                                if (DateTime.UtcNow > job.ExpiresAt)
                                {
                                    // 30% off as soon as it is late
                                    latePenaltyMultiplier = 0.7;

                                    // 5% off for each additional day late (after 14 days, payout is 0)
                                    var daysLate = (DateTime.UtcNow - job.ExpiresAt).TotalDays;
                                    if (daysLate is >= 1.0 and < 14.0)
                                    {
                                        latePenaltyMultiplier -= 0.05 * (int)daysLate;
                                    }
                                    else if (daysLate >= 14.0)
                                    {
                                        latePenaltyMultiplier = 0;
                                    }
                                }

                                var value = (int)(job.Value * latePenaltyMultiplier);
                                var jobRecord = new FinancialRecord
                                {
                                    ID = Guid.NewGuid(),
                                    ParentRecordID = flightFinancialRecord.ID,
                                    UserID = flightFinancialRecord.UserID,
                                    AirlineID = flightFinancialRecord.AirlineID,
                                    AircraftRegistry = flight.AircraftRegistry,
                                    Income = job.Value,
                                    Category = job.Type switch
                                    {
                                        JobType.Cargo_L => FinancialCategory.Cargo,
                                        JobType.Cargo_S => FinancialCategory.Cargo,
                                        _ => FinancialCategory.None
                                    },
                                    Timestamp = flightFinancialRecord.Timestamp,
                                    Description = $"{job.Type} job{(string.IsNullOrEmpty(job.UserIdentifier) ? string.Empty : $" ({job.UserIdentifier})")} of category {job.Category} from {job.OriginICAO} completed."
                                };
                                flight.Aircraft.LifeTimeIncome += job.Value;

                                if (job.Operator != null)
                                {
                                    job.Operator.PersonalAccountBalance += value;
                                }

                                if (job.OperatorAirline != null)
                                {
                                    job.OperatorAirline.AccountBalance += value;
                                }

                                if (job.ExpiresAt <= DateTime.UtcNow)
                                {
                                    var penaltyRecord = new FinancialRecord
                                    {
                                        ID = Guid.NewGuid(),
                                        ParentRecordID = flightFinancialRecord.ID,
                                        UserID = flightFinancialRecord.UserID,
                                        AirlineID = flightFinancialRecord.AirlineID,
                                        AircraftRegistry = flight.AircraftRegistry,
                                        Category = FinancialCategory.Fines,
                                        Expense = (int)(job.Value * (1.0 - latePenaltyMultiplier)),
                                        Timestamp = flightFinancialRecord.Timestamp,
                                        Description = $"Late delivery penalty ({(int)((1.0 - latePenaltyMultiplier) * 100)} %) for {job.Type} job{(string.IsNullOrEmpty(job.UserIdentifier) ? string.Empty : $" ({job.UserIdentifier})")} of category {job.Category} from {job.OriginICAO}"
                                    };
                                    await this.db.FinancialRecords.AddAsync(penaltyRecord);
                                    flight.Aircraft.LifeTimeExpense += (int)(job.Value * (1.0 - latePenaltyMultiplier));
                                }

                                this.statisticsService.RecordCompletedJob(job.OperatorAirline != null ? FlightOperator.Airline : FlightOperator.Player, flight.Aircraft.Type.Category, job.Type, flight.Aircraft.Type.Simulator);
                                await this.db.FinancialRecords.AddAsync(jobRecord);
                                this.db.Payloads.RemoveRange(job.Payloads);
                                this.db.Jobs.Remove(job);
                            }
                        }
                    }

                    var landedAt = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == flight.LandedAtICAO);
                    if (landedAt != null)
                    {
                        // Calculate landing fees
                        var landingFee = this.CalculateLandingFee(flight.Aircraft, landedAt);
                        var landingFeeRecord = new FinancialRecord
                        {
                            ID = Guid.NewGuid(),
                            ParentRecordID = flightFinancialRecord.ID,
                            UserID = flightFinancialRecord.UserID,
                            AirlineID = flightFinancialRecord.AirlineID,
                            AircraftRegistry = flight.AircraftRegistry,
                            Expense = landingFee,
                            Category = FinancialCategory.AirportFees,
                            Timestamp = flightFinancialRecord.Timestamp,
                            Description = $"Landing fee for aircraft {flight.AircraftRegistry.RemoveSimPrefix()} at airport {flight.LandedAtICAO}"
                        };
                        await this.db.FinancialRecords.AddAsync(landingFeeRecord);
                        flight.Aircraft.LifeTimeExpense += landingFee;

                        // Landed at closed airport?
                        if (landedAt.IsClosed)
                        {
                            // Fine is $B 1000 / tonne of aircraft max gross
                            var fine = (int)(1000 * (flight.Aircraft.Type.MaxGrossWeight / 2205));
                            var landingClosedFineRecord = new FinancialRecord
                            {
                                ID = Guid.NewGuid(),
                                ParentRecordID = flightFinancialRecord.ID,
                                UserID = flightFinancialRecord.UserID,
                                AirlineID = flightFinancialRecord.AirlineID,
                                AircraftRegistry = flight.AircraftRegistry,
                                Category = FinancialCategory.Fines,
                                Expense = fine,
                                Timestamp = flightFinancialRecord.Timestamp,
                                Description = $"Penalty for landing aircraft {flight.AircraftRegistry.RemoveSimPrefix()} at CLOSED airport {flight.LandedAtICAO}"
                            };
                            await this.db.FinancialRecords.AddAsync(landingClosedFineRecord);
                            flight.Aircraft.LifeTimeExpense += fine;

                            // todo reputation damage
                        }
                    }
                }

                // Are there fuelling financial records for this aircraft not yet assigned to a flight?
                if (!string.IsNullOrEmpty(flight.OperatorID))
                {
                    var fuellingRecords = this.db.FinancialRecords.Where(
                        f => f.AircraftRegistry == flight.AircraftRegistry && f.ParentRecordID == null && f.UserID == flight.OperatorID && f.Description.StartsWith("Fuel purchase") && f.Description.Contains(flight.OriginICAO));
                    foreach (var fuellingRecord in fuellingRecords)
                    {
                        fuellingRecord.ParentRecordID = flightFinancialRecord.ID;
                    }
                }
                else
                {
                    var fuellingRecords = this.db.FinancialRecords.Where(
                        f => f.AircraftRegistry == flight.AircraftRegistry && f.ParentRecordID == null && f.AirlineID == flight.OperatorAirlineID && f.Description.StartsWith("Fuel purchase") && f.Description.Contains(flight.OriginICAO));
                    foreach (var fuellingRecord in fuellingRecords)
                    {
                        fuellingRecord.ParentRecordID = flightFinancialRecord.ID;
                    }
                }

                // todo find any other ground handling fees financial records from the beginning of the flight and add here

                // todo calculate wear and tear on the aircraft
                // todo check final log for signs of cheating?
                // todo calculate final reputation/xp/whatever based on flight

                this.statisticsService.RecordCompletedFlight(flight.OperatorAirline != null ? FlightOperator.Airline : FlightOperator.Player, flight.Aircraft.Type.Category, flight.Aircraft.Type.Simulator);
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

                var missing = new AircraftManufacturer
                {
                    ID = "miss",
                    Name = "Missing"
                };

                var flight = await this.db.Flights.SingleOrDefaultAsync(f => f.OperatorID == user.Id && f.Started.HasValue && !f.Paused.HasValue && !f.Completed.HasValue);
                if (flight != null)
                {
                    flight.Aircraft.Type.Manufacturer ??= missing;
                    foreach (var variant in flight.Aircraft.Type.Variants.Where(v => v.Manufacturer == null))
                    {
                        variant.Manufacturer = missing;
                    }
                    return new ApiResponse<Flight>(flight);
                }

                if (!string.IsNullOrEmpty(user.AirlineICAO))
                {
                    flight = await this.db.Flights.SingleOrDefaultAsync(f => f.OperatorAirlineID == user.AirlineICAO && f.AssignedAirlinePilotID == user.Id && f.Started.HasValue && !f.Paused.HasValue && !f.Completed.HasValue);
                }

                if (flight != null)
                {
                    flight.Aircraft.Type.Manufacturer ??= missing;
                    foreach (var variant in flight.Aircraft.Type.Variants.Where(v => v.Manufacturer == null))
                    {
                        variant.Manufacturer = missing;
                    }
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

                var missing = new AircraftManufacturer
                {
                    ID = "miss",
                    Name = "Missing"
                };
                foreach (var plan in plans.Where(p => p.Aircraft != null))
                {
                    plan.Aircraft.Type.Manufacturer ??= missing;
                    foreach (var variant in plan.Aircraft.Type.Variants.Where(v => v.Manufacturer == null))
                    {
                        variant.Manufacturer = missing;
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
        /// Get "my" active flights (up to one currently flying and possibly multiple paused).
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

                var missing = new AircraftManufacturer
                {
                    ID = "miss",
                    Name = "Missing"
                };
                foreach (var flight in flights)
                {
                    flight.Aircraft.Type.Manufacturer ??= missing;
                    foreach (var variant in flight.Aircraft.Type.Variants.Where(v => v.Manufacturer == null))
                    {
                        variant.Manufacturer = missing;
                    }
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

                Aircraft aircraft = null;
                if (flightPlan.Aircraft != null)
                {
                    aircraft = await this.db.Aircraft.SingleOrDefaultAsync(a => a.Registry.Equals(flightPlan.Aircraft.Registry));
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

                    if (aircraft != null)
                    {
                        if (aircraft.Type.Simulator == Simulator.MSFS && !airport.MSFS)
                        {
                            return new ApiResponse<string>("Origin airport is not available for the simulator your selected aircraft uses!") { IsError = true };
                        }

                        if (aircraft.Type.Simulator == Simulator.XPlane11 && !airport.XP11)
                        {
                            return new ApiResponse<string>("Origin airport is not available for the simulator your selected aircraft uses!") { IsError = true };
                        }
                    }
                }

                if (!string.IsNullOrEmpty(flightPlan.DestinationICAO))
                {
                    var airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == flightPlan.DestinationICAO);
                    if (airport == null)
                    {
                        return new ApiResponse<string>("Destination airport not found!") { IsError = true };
                    }

                    if (aircraft != null)
                    {
                        if (aircraft.Type.Simulator == Simulator.MSFS && !airport.MSFS)
                        {
                            return new ApiResponse<string>("Destination airport is not available for the simulator your selected aircraft uses!") { IsError = true };
                        }

                        if (aircraft.Type.Simulator == Simulator.XPlane11 && !airport.XP11)
                        {
                            return new ApiResponse<string>("Destination airport is not available for the simulator your selected aircraft uses!") { IsError = true };
                        }
                    }
                }

                if (!string.IsNullOrEmpty(flightPlan.AlternateICAO))
                {
                    var airport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == flightPlan.AlternateICAO);
                    if (airport == null)
                    {
                        return new ApiResponse<string>("Alternate airport not found!") { IsError = true };
                    }

                    if (aircraft != null)
                    {
                        if (aircraft.Type.Simulator == Simulator.MSFS && !airport.MSFS)
                        {
                            return new ApiResponse<string>("Alternate airport is not available for the simulator your selected aircraft uses!") { IsError = true };
                        }

                        if (aircraft.Type.Simulator == Simulator.XPlane11 && !airport.XP11)
                        {
                            return new ApiResponse<string>("Alternate airport is not available for the simulator your selected aircraft uses!") { IsError = true };
                        }
                    }
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
                    await this.db.FlightPayloads.AddRangeAsync(flightPlan.Payloads);
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
                    existingFlight.DispatcherID = user.Id;
                    existingFlight.DispatcherRemarks = flightPlan.DispatcherRemarks;
                    existingFlight.PlannedDepartureTime = flightPlan.PlannedDepartureTime;
                    existingFlight.Route = flightPlan.Route;
                    existingFlight.AlternateRoute = flightPlan.AlternateRoute;
                    existingFlight.OfpHtml = flightPlan.OfpHtml;

                    this.db.FlightNavlogFixes.RemoveRange(existingFlight.NavlogFixes);
                    await this.db.FlightNavlogFixes.AddRangeAsync(flightPlan.NavlogFixes);

                    this.db.FlightPayloads.RemoveRange(existingFlight.FlightPayloads);
                    await this.db.FlightPayloads.AddRangeAsync(flightPlan.Payloads);

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
        /// <param name="startFlight">
        /// The start flight model.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("start", Name = "StartFlight")]
        public async Task<ActionResult<ApiResponse<StartFlightStatus>>> StartFlight([FromBody] StartFlight startFlight)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Flight/startFlight/{startFlight.FlightID}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<StartFlightStatus>("Unable to find user record!") { IsError = true, Data = StartFlightStatus.Error };
                }

                var plan = await this.db.Flights.SingleOrDefaultAsync(f => f.ID == startFlight.FlightID);
                if (plan == null)
                {
                    return new ApiResponse<StartFlightStatus>("No flight plan with that ID was found!") { IsError = true, Data = StartFlightStatus.Error };
                }

                // User operated flight, but not the current user?
                if (!string.IsNullOrEmpty(plan.OperatorID) && !plan.OperatorID.Equals(user.Id))
                {
                    return new ApiResponse<StartFlightStatus>("Unauthorized request!") { IsError = true, Data = StartFlightStatus.Error };
                }

                // Airline flight, but not the assigned pilot?
                if (!string.IsNullOrEmpty(plan.OperatorAirlineID))
                {
                    if (!plan.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<StartFlightStatus>("Unauthorized request!") { IsError = true, Data = StartFlightStatus.Error };
                    }

                    if (string.IsNullOrEmpty(plan.AssignedAirlinePilotID))
                    {
                        return new ApiResponse<StartFlightStatus>("This airline flight plan has no assigned pilot!") { IsError = true, Data = StartFlightStatus.Error };
                    }

                    if (!plan.AssignedAirlinePilotID.Equals(user.Id))
                    {
                        return new ApiResponse<StartFlightStatus>("This flight is assigned to another pilot in your airline!") { IsError = true, Data = StartFlightStatus.Error };
                    }
                }

                // Origin or destination missing?
                if (string.IsNullOrEmpty(plan.OriginICAO))
                {
                    return new ApiResponse<StartFlightStatus>("Flight plan has no origin airport!") { IsError = true, Data = StartFlightStatus.Error };
                }

                if (string.IsNullOrEmpty(plan.DestinationICAO))
                {
                    return new ApiResponse<StartFlightStatus>("Flight plan has no destination airport!") { IsError = true, Data = StartFlightStatus.Error };
                }

                // Aircraft not selected?
                if (string.IsNullOrEmpty(plan.AircraftRegistry) || plan.Aircraft == null)
                {
                    return new ApiResponse<StartFlightStatus>("Flight plan has no assigned aircraft!") { IsError = true, Data = StartFlightStatus.Error };
                }

                // Invalid flight number
                if (plan.FlightNumber is < 1 or > 9999)
                {
                    return new ApiResponse<StartFlightStatus>("Flight number is out of range (1-9999)!") { IsError = true, Data = StartFlightStatus.Error };
                }

                // No fuel
                if (!plan.FuelGallons.HasValue)
                {
                    return new ApiResponse<StartFlightStatus>("Flight plan has no fuel value!") { IsError = true, Data = StartFlightStatus.Error };
                }

                // Invalid fuel value?
                if (plan.FuelGallons.Value < 0 || plan.FuelGallons.Value > plan.Aircraft.Type.FuelTotalCapacity)
                {
                    return new ApiResponse<StartFlightStatus>("Invalid fuel amount!") { IsError = true, Data = StartFlightStatus.Error };
                }

                // Has the flight started already?
                if (plan.Started.HasValue)
                {
                    return new ApiResponse<StartFlightStatus>("Can't start flight already in progress!") { IsError = true, Data = StartFlightStatus.Error };
                }

                // Another flight in progress?
                var otherFlightInProgress = await this.db.Flights.AnyAsync(f => (f.OperatorID == user.Id || f.AssignedAirlinePilotID == user.Id) && f.Started.HasValue && !(f.Paused.HasValue || f.Completed.HasValue));
                if (otherFlightInProgress)
                {
                    return new ApiResponse<StartFlightStatus>("You already have another flight in progress! Please complete or pause the other flight before starting a new one.") { IsError = true, Data = StartFlightStatus.Error };
                }

                // Is the aircraft at the origin airport?
                if (!plan.OriginICAO.Equals(plan.Aircraft?.AirportICAO))
                {
                    return new ApiResponse<StartFlightStatus>(StartFlightStatus.AircraftNotAtOrigin);
                }

                // Can the aircraft start a new flight?
                if (plan.Aircraft?.CanStartFlight != true)
                {
                    return new ApiResponse<StartFlightStatus>("The selected aircraft isn't available right now!") { IsError = true, Data = StartFlightStatus.Error };
                }

                // Are all payloads either at the origin airport or already onboard the aircraft?
                foreach (var flightPayload in plan.FlightPayloads)
                {
                    if (!string.IsNullOrEmpty(flightPayload.Payload.AirportICAO) && flightPayload.Payload.AirportICAO != plan.OriginICAO)
                    {
                        return new ApiResponse<StartFlightStatus>("At least one payload hasn't reached the departure airport yet!") { IsError = true, Data = StartFlightStatus.Error };
                    }

                    if (!string.IsNullOrEmpty(flightPayload.Payload.AircraftRegistry) && flightPayload.Payload.AircraftRegistry != plan.AircraftRegistry)
                    {
                        return new ApiResponse<StartFlightStatus>("At least one payload is currently loaded on another aircraft!") { IsError = true, Data = StartFlightStatus.Error };
                    }
                }

                // Are there payloads onboard the aircraft that aren't on the flight plan?
                if (!startFlight.OverrideStates.Contains(StartFlightStatus.NonFlightPlanPayloadsFound))
                {
                    foreach (var aircraftPayload in plan.Aircraft.Payloads)
                    {
                        if (plan.FlightPayloads.All(p => p.PayloadID != aircraftPayload.ID))
                        {
                            return new ApiResponse<StartFlightStatus>(StartFlightStatus.NonFlightPlanPayloadsFound);
                        }
                    }
                }

                // All checks passed, start the flight and calculate the payload and fuel loading times
                plan.Started = DateTime.UtcNow;

                // Any changes to these figures need to be mirrored in the AircraftController.PerformGroundOperations method
                var gallonsPerMinute = plan.Aircraft.Type.Category switch
                {
                    AircraftTypeCategory.SEP => 25,
                    AircraftTypeCategory.MEP => 25,
                    AircraftTypeCategory.SET => 50,
                    AircraftTypeCategory.MET => 50,
                    AircraftTypeCategory.JET => 50,
                    AircraftTypeCategory.REG => 600,
                    AircraftTypeCategory.NBA => 600,
                    AircraftTypeCategory.WBA => 1200,
                    AircraftTypeCategory.HEL => 50,
                    _ => 0.0
                };
                var gallonsToTransfer = Math.Abs(plan.Aircraft.Fuel - plan.FuelGallons.Value);
                if (gallonsToTransfer > 0 && plan.FuelGallons.Value > plan.Aircraft.Fuel)
                {
                    var fuelRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        AircraftRegistry = plan.Aircraft.Registry,
                        Category = FinancialCategory.Fuel
                    };

                    if (plan.Aircraft.Type.FuelType == FuelType.AvGas)
                    {
                        if (!plan.Aircraft.Airport.HasAvGas)
                        {
                            // todo check for fbo in the future
                            if (startFlight.OverrideStates.Contains(StartFlightStatus.OriginDoesntSellAvGas))
                            {
                                // User chose to skip fuelling and start anyway
                                gallonsToTransfer = 0;
                                fuelRecord = null;
                            }
                            else
                            {
                                return new ApiResponse<StartFlightStatus>(StartFlightStatus.OriginDoesntSellAvGas);
                            }
                        }
                        else
                        {
                            var fuelPrice = (int)(gallonsToTransfer * plan.Aircraft.Airport.AvGasPrice);
                            plan.Aircraft.LifeTimeExpense += fuelPrice;
                            if (!string.IsNullOrEmpty(plan.Aircraft.AirlineOwnerID))
                            {
                                if (fuelPrice > plan.Aircraft.AirlineOwner.AccountBalance)
                                {
                                    return new ApiResponse<StartFlightStatus>("Your airline can't afford this fuel purchase!") { IsError = true, Data = StartFlightStatus.Error };
                                }

                                fuelRecord.AirlineID = plan.Aircraft.AirlineOwnerID;
                                fuelRecord.Expense = fuelPrice;
                                fuelRecord.Description = $"Fuel purchase {plan.Aircraft.Registry.RemoveSimPrefix()}: {gallonsToTransfer:F1} gallons AV gas at {plan.Aircraft.AirportICAO} for $B {plan.Aircraft.Airport.AvGasPrice:F2} / gallon";
                            }
                            else
                            {
                                if (fuelPrice > plan.Aircraft.Owner.PersonalAccountBalance)
                                {
                                    return new ApiResponse<StartFlightStatus>("You can't afford this fuel purchase!") { IsError = true, Data = StartFlightStatus.Error };
                                }

                                fuelRecord.UserID = plan.Aircraft.OwnerID;
                                fuelRecord.Expense = fuelPrice;
                                fuelRecord.Description = $"Fuel purchase {plan.Aircraft.Registry.RemoveSimPrefix()}: {gallonsToTransfer:F1} gallons AV gas at {plan.Aircraft.AirportICAO} for $B {plan.Aircraft.Airport.AvGasPrice:F2} / gallon";
                            }
                        }
                    }
                    else if (plan.Aircraft.Type.FuelType == FuelType.JetFuel)
                    {
                        if (!plan.Aircraft.Airport.HasJetFuel)
                        {
                            // todo check for fbo in the future
                            if (startFlight.OverrideStates.Contains(StartFlightStatus.OriginDoesntSellJetFuel))
                            {
                                // User chose to skip fuelling and start anyway
                                gallonsToTransfer = 0;
                                fuelRecord = null;
                            }
                            else
                            {
                                return new ApiResponse<StartFlightStatus>(StartFlightStatus.OriginDoesntSellJetFuel);
                            }
                        }
                        else
                        {
                            var fuelPrice = (int)(gallonsToTransfer * plan.Aircraft.Airport.JetFuelPrice);
                            plan.Aircraft.LifeTimeExpense += fuelPrice;
                            if (!string.IsNullOrEmpty(plan.Aircraft.AirlineOwnerID))
                            {
                                if (fuelPrice > plan.Aircraft.AirlineOwner.AccountBalance)
                                {
                                    return new ApiResponse<StartFlightStatus>("Your airline can't afford this fuel purchase!") { IsError = true, Data = StartFlightStatus.Error };
                                }

                                fuelRecord.AirlineID = plan.Aircraft.AirlineOwnerID;
                                fuelRecord.Expense = fuelPrice;
                                fuelRecord.Description = $"Fuel purchase {plan.Aircraft.Registry.RemoveSimPrefix()}: {gallonsToTransfer:F1} gallons jet fuel at {plan.Aircraft.AirportICAO} for $B {plan.Aircraft.Airport.JetFuelPrice:F2} / gallon";
                            }
                            else
                            {
                                if (fuelPrice > plan.Aircraft.Owner.PersonalAccountBalance)
                                {
                                    return new ApiResponse<StartFlightStatus>("You can't afford this fuel purchase!") { IsError = true, Data = StartFlightStatus.Error };
                                }

                                fuelRecord.UserID = plan.Aircraft.OwnerID;
                                fuelRecord.Expense = fuelPrice;
                                fuelRecord.Description = $"Fuel purchase {plan.Aircraft.Registry.RemoveSimPrefix()}: {gallonsToTransfer:F1} gallons jet fuel at {plan.Aircraft.AirportICAO} for $B {plan.Aircraft.Airport.JetFuelPrice:F2} / gallon";
                            }
                        }
                    }
                    else
                    {
                        return new ApiResponse<StartFlightStatus>("This aircraft doesn't use fuel and can therefore not be fueled!") { IsError = true, Data = StartFlightStatus.Error };
                    }

                    if (fuelRecord != null)
                    {
                        await this.db.FinancialRecords.AddAsync(fuelRecord);
                    }
                }

                plan.FuelLoadingComplete = gallonsPerMinute > 0 && gallonsToTransfer > 0 ? DateTime.UtcNow.AddMinutes(3 + (gallonsToTransfer / gallonsPerMinute)) : DateTime.UtcNow;
                if (plan.Aircraft.FuellingUntil.HasValue && plan.Aircraft.FuellingUntil.Value > DateTime.UtcNow)
                {
                    // Aircraft still has fuelling time left, add to the flight and remove from aircraft
                    plan.FuelLoadingComplete += plan.Aircraft.FuellingUntil.Value - DateTime.UtcNow;
                    plan.Aircraft.FuellingUntil = null;
                    plan.Aircraft.Fuel = plan.FuelGallons.Value;
                }

                // Any changes to these figures need to be mirrored in the AircraftController.PerformGroundOperations and FlightController.CompleteFlight method
                var lbsPerMinute = plan.Aircraft.Type.Category switch
                {
                    AircraftTypeCategory.SEP => 225,
                    AircraftTypeCategory.MEP => 225,
                    AircraftTypeCategory.SET => 225,
                    AircraftTypeCategory.MET => 350,
                    AircraftTypeCategory.JET => 350,
                    AircraftTypeCategory.REG => 1500,
                    AircraftTypeCategory.NBA => 2000,
                    AircraftTypeCategory.WBA => 3300,
                    AircraftTypeCategory.HEL => 350,
                    _ => 0.0
                };
                var lbsToTransfer = 0.0;
                foreach (var flightPayload in plan.FlightPayloads)
                {
                    if (!string.IsNullOrEmpty(flightPayload.Payload.AirportICAO))
                    {
                        lbsToTransfer += flightPayload.Payload.Weight;
                        flightPayload.Payload.AirportICAO = null;
                        flightPayload.Payload.AircraftRegistry = plan.AircraftRegistry;
                    }
                }

                plan.PayloadLoadingComplete = lbsPerMinute > 0 && lbsToTransfer > 0 ? DateTime.UtcNow.AddMinutes(1 + (lbsToTransfer / lbsPerMinute)) : DateTime.UtcNow;
                if (plan.Aircraft.LoadingUntil.HasValue && plan.Aircraft.LoadingUntil.Value > DateTime.UtcNow)
                {
                    // Aircraft still has loading time left, add to the flight and remove from aircraft
                    plan.PayloadLoadingComplete += plan.Aircraft.LoadingUntil.Value - DateTime.UtcNow;
                    plan.Aircraft.LoadingUntil = null;
                }

                // todo charge cargo handling fee? passenger fee? for the airport...

                // No alternate set?, set to origin (return to base)
                if (string.IsNullOrEmpty(plan.AlternateICAO))
                {
                    plan.AlternateICAO = plan.OriginICAO;
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, $"Error starting flight {startFlight.FlightID}.");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<StartFlightStatus>(StartFlightStatus.Started)
                {
                    Message = $"Flight {plan.FullFlightNumber} started successfully, remember to keep the rubber on the runway and your troubles on the ground!"
                };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Flight/startFlight/{startFlight.FlightID}");
                return new ApiResponse<StartFlightStatus>(ex) { Data = StartFlightStatus.Error };
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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculate the landing fee for the specified aircraft at the given airport.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// <param name="aircraft">
        /// The aircraft.
        /// </param>
        /// <param name="airport">
        /// The airport.
        /// </param>
        /// <returns>
        /// The calculated landing fee.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private int CalculateLandingFee(Aircraft aircraft, Airport airport)
        {
            var mtowTonnes = (int)Math.Round(aircraft.Type.MaxGrossWeight / 2205, 0, MidpointRounding.ToPositiveInfinity);
            var militarySurcharge = airport.IsMilitary ? 500 : 0;
            if (airport.Size >= 5)
            {
                mtowTonnes = mtowTonnes > 45 ? mtowTonnes - 45 : 0;
                return 220 + (mtowTonnes * 6) + militarySurcharge;
            }

            if (airport.Size >= 2)
            {
                return 15 * mtowTonnes + militarySurcharge;
            }

            return airport.Size == 1 ? 10 + militarySurcharge : militarySurcharge;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Find the closest airport to the specified geo coordinate.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/01/2022.
        /// </remarks>
        /// <param name="location">
        /// The location.
        /// </param>
        /// <param name="simulator">
        /// The simulator.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the closest airport ICAO code, or NULL if there is no
        /// airport within 10 nm.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<string> GetClosestAirport(GeoCoordinate location, Simulator simulator)
        {
            // Check where we are (closest airport)
            var coverage = location.CircularCoverage(10);
            var cells = coverage.Cells.Select(c => c.Id).ToList();
            var airports = await this.db.Airports.Where($"@0.Contains(S2Cell{coverage.Level})", cells).ToListAsync();
            if (simulator == Simulator.MSFS)
            {
                airports = airports.Where(a => a.MSFS).ToList();
            }

            if (simulator == Simulator.XPlane11)
            {
                airports = airports.Where(a => a.XP11).ToList();
            }

            if (airports.Count == 0)
            {
                // Landed at no airport, back to origin?
                return null;
            }

            Airport closest = null;
            double closestDistance = 9999;
            foreach (var airport in airports)
            {
                var distance = airport.GeoCoordinate.GetDistanceTo(location);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = airport;
                }
            }

            if (closest != null)
            {
                return closest.ICAO;
            }

            // Should not happen, take first airport from list I guess?
            return airports[0].ICAO;
        }
    }
}