// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftController.cs" company="OpenSky">
// OpenSky project 2021-2022
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
    using OpenSky.API.Helpers;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Aircraft;
    using OpenSky.API.Model.Authentication;
    using OpenSky.API.Services;
    using OpenSky.API.Services.Models;

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
        /// The aircraft populator service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly AircraftPopulatorService aircraftPopulator;

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
        /// The statistics service.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly StatisticsService statisticsService;

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
        /// <param name="aircraftPopulator">
        /// The aircraft populator service.
        /// </param>
        /// <param name="statisticsService">
        /// The statistics service.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftController(ILogger<AirportController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager, IcaoRegistrationsService icaoRegistrations, AircraftPopulatorService aircraftPopulator, StatisticsService statisticsService)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.icaoRegistrations = icaoRegistrations;
            this.aircraftPopulator = aircraftPopulator;
            this.statisticsService = statisticsService;
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

                if (aircraft.OwnerID != user.Id && aircraft.AirlineOwnerID == user.AirlineICAO)
                {
                    // User/Airline doesn't own this aircraft, zero out the financials
                    aircraft.LifeTimeExpense = 0;
                    aircraft.LifeTimeIncome = 0;
                }

                var missing = new AircraftManufacturer
                {
                    ID = "miss",
                    Name = "Missing"
                };
                aircraft.Type.Manufacturer ??= missing;
                foreach (var variant in aircraft.Type.Variants.Where(v => v.Manufacturer == null))
                {
                    variant.Manufacturer = missing;
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

                var missing = new AircraftManufacturer
                {
                    ID = "miss",
                    Name = "Missing"
                };
                if (this.User.IsInRole(UserRoles.Moderator) || this.User.IsInRole(UserRoles.Admin))
                {
                    // Return all aircraft
                    var aircraft = await this.db.Aircraft.Where(a => a.AirportICAO.Equals(icao) && !a.Flights.Any(f => f.Started.HasValue && !f.Completed.HasValue)).ToListAsync();

                    foreach (var craft in aircraft)
                    {
                        if (craft.OwnerID != user.Id && craft.AirlineOwnerID == user.AirlineICAO)
                        {
                            // User/Airline doesn't own this aircraft, zero out the financials
                            craft.LifeTimeExpense = 0;
                            craft.LifeTimeIncome = 0;
                        }

                        craft.Type.Manufacturer ??= missing;
                        foreach (var variant in craft.Type.Variants.Where(v => v.Manufacturer == null))
                        {
                            variant.Manufacturer = missing;
                        }
                    }

                    return new ApiResponse<IEnumerable<Aircraft>>(aircraft);
                }
                else
                {
                    // Only return aircraft that are available for purchase or rent, or owned by the player
                    var aircraft = await this.db.Aircraft.Where(
                                                 a => a.AirportICAO.Equals(icao) && !a.Flights.Any(f => f.Started.HasValue && !f.Completed.HasValue) &&
                                                      (a.OwnerID == user.Id || a.AirlineOwnerID == user.AirlineICAO || a.PurchasePrice.HasValue || a.RentPrice.HasValue))
                                             .ToListAsync();

                    foreach (var craft in aircraft)
                    {
                        if (craft.OwnerID != user.Id && craft.AirlineOwnerID == user.AirlineICAO)
                        {
                            // User/Airline doesn't own this aircraft, zero out the financials
                            craft.LifeTimeExpense = 0;
                            craft.LifeTimeIncome = 0;
                        }

                        craft.Type.Manufacturer ??= missing;
                        foreach (var variant in craft.Type.Variants.Where(v => v.Manufacturer == null))
                        {
                            variant.Manufacturer = missing;
                        }
                    }

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
                var missing = new AircraftManufacturer
                {
                    ID = "miss",
                    Name = "Missing"
                };
                foreach (var craft in aircraft)
                {
                    craft.Type.Manufacturer ??= missing;
                    foreach (var variant in craft.Type.Variants.Where(v => v.Manufacturer == null))
                    {
                        variant.Manufacturer = missing;
                    }
                }
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
        /// Perform ground operations for the specified aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 20/12/2021.
        /// </remarks>
        /// <param name="operations">
        /// The operations to be performed (fuel and payload loading).
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPut("groundOperations", Name = "PerformGroundOperations")]
        public async Task<ActionResult<ApiResponse<string>>> PerformGroundOperations([FromBody] GroundOperations operations)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT Aircraft/groundOperations/{operations.Registry}");

                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string> { Message = "Unable to find user record!", IsError = true };
                }

                var aircraft = await this.db.Aircraft.SingleOrDefaultAsync(a => a.Registry.Equals(operations.Registry));
                if (aircraft == null)
                {
                    return new ApiResponse<string>("Aircraft not found!") { IsError = true };
                }

                // Can start flight only works if idle or ground operations, we have the same requirements (aka no active flights or maintenance)
                if (!aircraft.CanStartFlight)
                {
                    return new ApiResponse<string>("Aircraft not available for ground operations!") { IsError = true };
                }

                if (!string.IsNullOrEmpty(aircraft.AirlineOwnerID))
                {
                    if (aircraft.AirlineOwnerID != user.AirlineICAO)
                    {
                        return new ApiResponse<string>("You airline doesn't own this aircraft!") { IsError = true };
                    }

                    // todo also check for assigned airline pilot
                    if (!AirlineController.UserHasPermission(user, AirlinePermission.PerformGroundOperations))
                    {
                        return new ApiResponse<string>("You don't have the permission to perform ground operations for your airline!") { IsError = true };
                    }
                }

                if (aircraft.OwnerID != user.Id)
                {
                    return new ApiResponse<string>("You don't own this aircraft!") { IsError = true };
                }

                // Invalid fuel value?
                if (operations.Fuel < 0 || operations.Fuel > aircraft.Type.FuelTotalCapacity)
                {
                    return new ApiResponse<string>("Invalid fuel amount!") { IsError = true };
                }

                // Any changes to these figures need to be mirrored in the FlightController.StartFlight method
                var gallonsPerMinute = aircraft.Type.Category switch
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
                var gallonsToTransfer = Math.Abs(aircraft.Fuel - operations.Fuel);
                if (gallonsPerMinute > 0 && gallonsToTransfer > 0)
                {
                    if (operations.Fuel > aircraft.Fuel)
                    {
                        var fuelRecord = new FinancialRecord
                        {
                            ID = Guid.NewGuid(),
                            Timestamp = DateTime.UtcNow,
                            AircraftRegistry = aircraft.Registry,
                            Category = FinancialCategory.Fuel
                        };

                        if (aircraft.Type.FuelType == FuelType.AvGas)
                        {
                            if (!aircraft.Airport.HasAvGas)
                            {
                                // todo check for fbo in the future
                                return new ApiResponse<string>($"Airport {aircraft.AirportICAO} does not sell AV gas!") { IsError = true };
                            }

                            var fuelPrice = (int)(gallonsToTransfer * aircraft.Airport.AvGasPrice);
                            aircraft.LifeTimeExpense += fuelPrice;
                            if (!string.IsNullOrEmpty(aircraft.AirlineOwnerID))
                            {
                                if (fuelPrice > aircraft.AirlineOwner.AccountBalance)
                                {
                                    return new ApiResponse<string>("Your airline can't afford this fuel purchase!") { IsError = true };
                                }

                                fuelRecord.AirlineID = aircraft.AirlineOwnerID;
                                fuelRecord.Expense = fuelPrice;
                                fuelRecord.Description = $"Fuel purchase {aircraft.Registry.RemoveSimPrefix()}: {gallonsToTransfer:F1} gallons AV gas at {aircraft.AirportICAO} for $B {aircraft.Airport.AvGasPrice:F2} / gallon";
                            }
                            else
                            {
                                if (fuelPrice > aircraft.Owner.PersonalAccountBalance)
                                {
                                    return new ApiResponse<string>("You can't afford this fuel purchase!") { IsError = true };
                                }

                                fuelRecord.UserID = aircraft.OwnerID;
                                fuelRecord.Expense = fuelPrice;
                                fuelRecord.Description = $"Fuel purchase {aircraft.Registry.RemoveSimPrefix()}: {gallonsToTransfer:F1} gallons AV gas at {aircraft.AirportICAO} for $B {aircraft.Airport.AvGasPrice:F2} / gallon";
                            }
                        }
                        else if (aircraft.Type.FuelType == FuelType.JetFuel)
                        {
                            if (!aircraft.Airport.HasJetFuel)
                            {
                                // todo check for fbo in the future
                                return new ApiResponse<string>($"Airport {aircraft.AirportICAO} does not sell jet fuel!") { IsError = true };
                            }

                            var fuelPrice = (int)(gallonsToTransfer * aircraft.Airport.JetFuelPrice);
                            aircraft.LifeTimeExpense += fuelPrice;
                            if (!string.IsNullOrEmpty(aircraft.AirlineOwnerID))
                            {
                                if (fuelPrice > aircraft.AirlineOwner.AccountBalance)
                                {
                                    return new ApiResponse<string>("Your airline can't afford this fuel purchase!") { IsError = true };
                                }

                                fuelRecord.AirlineID = aircraft.AirlineOwnerID;
                                fuelRecord.Expense = fuelPrice;
                                fuelRecord.Description = $"Fuel purchase {aircraft.Registry.RemoveSimPrefix()}: {gallonsToTransfer:F1} gallons jet fuel at {aircraft.AirportICAO} for $B {aircraft.Airport.AvGasPrice:F2} / gallon";
                            }
                            else
                            {
                                if (fuelPrice > aircraft.Owner.PersonalAccountBalance)
                                {
                                    return new ApiResponse<string>("You can't afford this fuel purchase!") { IsError = true };
                                }

                                fuelRecord.UserID = aircraft.OwnerID;
                                fuelRecord.Expense = fuelPrice;
                                fuelRecord.Description = $"Fuel purchase {aircraft.Registry.RemoveSimPrefix()}: {gallonsToTransfer:F1} gallons jet fuel at {aircraft.AirportICAO} for $B {aircraft.Airport.AvGasPrice:F2} / gallon";
                            }
                        }
                        else
                        {
                            return new ApiResponse<string>("This aircraft doesn't use fuel and can therefore not be fueled!") { IsError = true };
                        }

                        await this.db.FinancialRecords.AddAsync(fuelRecord);
                    }

                    aircraft.Fuel = operations.Fuel;
                    if (aircraft.FuellingUntil.HasValue && aircraft.FuellingUntil.Value > DateTime.UtcNow)
                    {
                        aircraft.FuellingUntil += TimeSpan.FromMinutes(gallonsToTransfer / gallonsPerMinute);
                    }
                    else
                    {
                        // New fuelling operation, add 3 minutes for arrival of fuel truck
                        aircraft.FuellingUntil = DateTime.UtcNow.AddMinutes(3 + (gallonsToTransfer / gallonsPerMinute));
                    }
                }

                // Any changes to these figures need to be mirrored in the FlightController.StartFlight and FlightController.CompleteFlight method
                var lbsPerMinute = aircraft.Type.Category switch
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

                // First check for payloads to be removed
                foreach (var aircraftPayload in aircraft.Payloads.ToList())
                {
                    if (!operations.Payloads.Contains(aircraftPayload.ID))
                    {
                        lbsToTransfer += aircraftPayload.Weight;
                        aircraftPayload.AirportICAO = aircraft.AirportICAO;
                        aircraftPayload.AircraftRegistry = null;
                        jobIDsToCheck.Add(aircraftPayload.JobID);
                        payloadsArrived.Add(aircraftPayload.ID);
                    }
                }

                // Check for payloads to be loaded
                foreach (var payloadID in operations.Payloads)
                {
                    var payload = await this.db.Payloads.SingleOrDefaultAsync(p => p.ID == payloadID);
                    if (payload == null)
                    {
                        return new ApiResponse<string>("Unknown payload ID specified!") { IsError = true };
                    }

                    if (!string.IsNullOrEmpty(payload.AircraftRegistry) && payload.AircraftRegistry != aircraft.Registry)
                    {
                        return new ApiResponse<string>("Can't transfer payload directly between two aircraft!") { IsError = true };
                    }

                    if (!string.IsNullOrEmpty(payload.AirportICAO))
                    {
                        if (payload.AirportICAO != aircraft.AirportICAO)
                        {
                            return new ApiResponse<string>("Specified payload is not at the same airport as the aircraft!") { IsError = true };
                        }

                        lbsToTransfer += payload.Weight;
                        payload.AircraftRegistry = aircraft.Registry;
                        payload.AirportICAO = null;
                    }
                }

                if (lbsPerMinute > 0 && lbsToTransfer > 0)
                {
                    if (aircraft.LoadingUntil.HasValue && aircraft.LoadingUntil.Value > DateTime.UtcNow)
                    {
                        aircraft.LoadingUntil += TimeSpan.FromMinutes(lbsToTransfer / lbsPerMinute);
                    }
                    else
                    {
                        // New loading operation, add 1 minute for arrival of cargo/pax
                        aircraft.LoadingUntil = DateTime.UtcNow.AddMinutes(1 + (lbsToTransfer / lbsPerMinute));
                    }
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
                                UserID = job.OperatorID,
                                AirlineID = job.OperatorAirlineID,
                                AircraftRegistry = aircraft.Registry,
                                Income = job.Value,
                                Category = job.Type switch
                                {
                                    JobType.Cargo_L => FinancialCategory.Cargo,
                                    JobType.Cargo_S => FinancialCategory.Cargo,
                                    _ => FinancialCategory.None
                                },
                                Timestamp = DateTime.UtcNow,
                                Description = $"{job.Type} job{(string.IsNullOrEmpty(job.UserIdentifier) ? string.Empty : $" ({job.UserIdentifier})")} of category {job.Category} from {job.OriginICAO} completed."
                            };
                            aircraft.LifeTimeIncome += job.Value;

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
                                    UserID = job.OperatorID,
                                    AirlineID = job.OperatorAirlineID,
                                    AircraftRegistry = aircraft.Registry,
                                    Category = FinancialCategory.Fines,
                                    Expense = (int)(job.Value * (1.0 - latePenaltyMultiplier)),
                                    Timestamp = DateTime.UtcNow,
                                    Description = $"Late delivery penalty ({(int)((1.0 - latePenaltyMultiplier) * 100)} %) for {job.Type} job{(string.IsNullOrEmpty(job.UserIdentifier) ? string.Empty : $" ({job.UserIdentifier})")} of category {job.Category} from {job.OriginICAO}"
                                };
                                await this.db.FinancialRecords.AddAsync(penaltyRecord);
                                aircraft.LifeTimeExpense += (int)(job.Value * (1.0 - latePenaltyMultiplier));
                            }

                            this.statisticsService.RecordCompletedJob(job.OperatorAirline != null ? FlightOperator.Airline : FlightOperator.Player, aircraft.Type.Category, job.Type, aircraft.Type.Simulator);
                            await this.db.FinancialRecords.AddAsync(jobRecord);
                            this.db.Payloads.RemoveRange(job.Payloads);
                            this.db.Jobs.Remove(job);
                        }
                    }
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error starting ground operations.");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully started ground operations for aircraft {operations.Registry.RemoveSimPrefix()}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | PUT Aircraft/groundOperations/{operations.Registry}");
                return new ApiResponse<string>(ex);
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
                    return new ApiResponse<string>($"Aircraft with registry {purchase.Registry.RemoveSimPrefix()} is not for sale!") { IsError = true };
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

                // If the owner isn't the system, credit the owner pilot/airline for the purchase
                if (aircraft.Owner != null)
                {
                    aircraft.Owner.PersonalAccountBalance += aircraft.PurchasePrice.Value;
                    var saleRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        UserID = aircraft.OwnerID,
                        Income = aircraft.PurchasePrice.Value,
                        Category = FinancialCategory.Aircraft,
                        Description = $"Sale of aircraft {aircraft.Registry.RemoveSimPrefix()}, type {aircraft.Type.Name} at airport {aircraft.AirportICAO}"
                    };
                    await this.db.FinancialRecords.AddAsync(saleRecord);
                }

                if (aircraft.AirlineOwner != null)
                {
                    aircraft.AirlineOwner.AccountBalance += aircraft.PurchasePrice.Value;
                    var airlineSaleRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        AirlineID = aircraft.AirlineOwnerID,
                        Income = aircraft.PurchasePrice.Value,
                        Category = FinancialCategory.Aircraft,
                        Description = $"Sale of aircraft {aircraft.Registry.RemoveSimPrefix()}, type {aircraft.Type.Name} at airport {aircraft.AirportICAO}"
                    };
                    await this.db.FinancialRecords.AddAsync(airlineSaleRecord);
                }

                if (!purchase.ForAirline)
                {
                    if (aircraft.PurchasePrice.Value > user.PersonalAccountBalance)
                    {
                        return new ApiResponse<string>("You can't afford this aircraft!") { IsError = true };
                    }

                    aircraft.OwnerID = user.Id;
                    aircraft.AirlineOwnerID = null;
                    user.PersonalAccountBalance -= aircraft.PurchasePrice.Value;
                    var purchaseRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        UserID = user.Id,
                        Expense = aircraft.PurchasePrice.Value,
                        Category = FinancialCategory.Aircraft,
                        Description = $"Purchase of aircraft {aircraft.Registry.RemoveSimPrefix()}, type {aircraft.Type.Name} at airport {aircraft.AirportICAO}",
                        AircraftRegistry = aircraft.Registry
                    };
                    await this.db.FinancialRecords.AddAsync(purchaseRecord);
                }
                else
                {
                    if (aircraft.PurchasePrice.Value > user.Airline.AccountBalance)
                    {
                        return new ApiResponse<string>("Your airline can't afford this aircraft!") { IsError = true };
                    }

                    aircraft.AirlineOwnerID = user.AirlineICAO;
                    aircraft.OwnerID = null;
                    user.Airline.AccountBalance -= aircraft.PurchasePrice.Value;
                    var airlinePurchaseRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        AirlineID = user.AirlineICAO,
                        Expense = aircraft.PurchasePrice.Value,
                        Category = FinancialCategory.Aircraft,
                        Description = $"Purchase of aircraft {aircraft.Registry.RemoveSimPrefix()}, type {aircraft.Type.Name} at airport {aircraft.AirportICAO} by user {user.UserName}",
                        AircraftRegistry = aircraft.Registry
                    };
                    await this.db.FinancialRecords.AddAsync(airlinePurchaseRecord);
                }

                aircraft.LifeTimeExpense = aircraft.PurchasePrice ?? 0;
                aircraft.PurchasePrice = null;
                aircraft.RentPrice = null;
                aircraft.LifeTimeIncome = 0;

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error purchasing aircraft");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                // Ask aircraft populator to "restock" the airport by adding a new plane in place of this one
                await this.aircraftPopulator.CheckAndGenerateAircraftForAirport(aircraft.Airport, aircraft.Type.Simulator);

                return new ApiResponse<string>($"Successfully purchased aircraft {purchase.Registry.RemoveSimPrefix()}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Aircraft/purchase: {purchase.Registry}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Purchase new aircraft.
        /// </summary>
        /// <remarks>
        /// sushi.at, 14/02/2022.
        /// </remarks>
        /// <param name="purchase">
        /// The purchase new aircraft model.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("purchaseNew", Name = "PurchaseNewAircraft")]
        public async Task<ActionResult<ApiResponse<string>>> PurchaseNewAircraft([FromBody] PurchaseNewAircraft purchase)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Aircraft/purchaseNew: {purchase.TypeID}@{purchase.DeliveryAirportICAO}");
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

                var aircraftType = await this.db.AircraftTypes.SingleOrDefaultAsync(at => at.ID == purchase.TypeID);
                if (aircraftType == null)
                {
                    return new ApiResponse<string>("Aircraft type with specified ID not found!") { IsError = true };
                }

                var deliveryAirport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == purchase.DeliveryAirportICAO);
                if (deliveryAirport == null)
                {
                    return new ApiResponse<string>("Delivery airport with specified ICAO code not found!") { IsError = true };
                }

                if (deliveryAirport.IsClosed)
                {
                    return new ApiResponse<string>("Can't purchase aircraft at closed airports!") { IsError = true };
                }

                if (aircraftType.Simulator == Simulator.MSFS && !deliveryAirport.MSFS)
                {
                    return new ApiResponse<string>("Can't purchase MSFS aircraft at airports not available for this simulator!") { IsError = true };
                }

                if (aircraftType.Simulator == Simulator.XPlane11 && !deliveryAirport.XP11)
                {
                    return new ApiResponse<string>("Can't purchase X-Plane11 aircraft at airports not available for this simulator!") { IsError = true };
                }

                if (aircraftType.DeliveryLocations.All(dl => dl.AirportICAO != purchase.DeliveryAirportICAO))
                {
                    return new ApiResponse<string>("The manufacturer does not deliver this aircraft type from the specified airport!") { IsError = true };
                }

                var deliveryCostPerAircraft = 0;
                var aircraftDestinationICAO = purchase.DeliveryAirportICAO;
                if (purchase.DeliveryOption == NewAircraftDeliveryOption.ManufacturerFerry)
                {
                    var ferryAirport = await this.db.Airports.SingleOrDefaultAsync(a => a.ICAO == purchase.FerryAirportICAO);
                    if (ferryAirport == null)
                    {
                        return new ApiResponse<string>("Ferry airport with specified ICAO code not found!") { IsError = true };
                    }

                    if (ferryAirport.IsClosed)
                    {
                        return new ApiResponse<string>("Can't ferry aircraft to closed airports!") { IsError = true };
                    }

                    if (aircraftType.Simulator == Simulator.MSFS && !ferryAirport.MSFS)
                    {
                        return new ApiResponse<string>("Can't ferry MSFS aircraft to airports not available for this simulator!") { IsError = true };
                    }

                    if (aircraftType.Simulator == Simulator.XPlane11 && !ferryAirport.XP11)
                    {
                        return new ApiResponse<string>("Can't ferry X-Plane11 aircraft to airports not available for this simulator!") { IsError = true };
                    }

                    aircraftDestinationICAO = purchase.FerryAirportICAO;
                    var ferryDistance = deliveryAirport.GeoCoordinate.GetDistanceTo(ferryAirport.GeoCoordinate) / 1852.0;
                    var manufacturerFerryCostPerNm = aircraftType.Category switch
                    {
                        AircraftTypeCategory.SEP => 15,
                        AircraftTypeCategory.MEP => 25,
                        AircraftTypeCategory.SET => 30,
                        AircraftTypeCategory.MET => 50,
                        AircraftTypeCategory.JET => 100,
                        AircraftTypeCategory.HEL => 100,
                        AircraftTypeCategory.REG => 150,
                        AircraftTypeCategory.NBA => 250,
                        AircraftTypeCategory.WBA => 400,
                        _ => 0
                    };
                    deliveryCostPerAircraft = (int)(ferryDistance * manufacturerFerryCostPerNm);
                }

                if (purchase.DeliveryOption == NewAircraftDeliveryOption.OutsourceFerry)
                {
                    // todo implement outsourced ferry flights
                    return new ApiResponse<string>("Sorry not yet implemented!") { IsError = true };
                }

                var volumeDiscount = purchase.NumberOfAircraft switch
                {
                    >= 50 => 0.25,
                    >= 10 => 0.1,
                    >= 3 => 0.05,
                    _ => 0
                };

                var grandTotalPrice = (int)((aircraftType.MaxPrice - volumeDiscount + deliveryCostPerAircraft) * purchase.NumberOfAircraft);

                var registries = string.Empty;
                for (var i = 0; i < purchase.NumberOfAircraft; i++)
                {
                    var registry = this.aircraftPopulator.GenerateNewAircraftRegistration(purchase.Country, aircraftType.Simulator);
                    registries += $"{registry.RemoveSimPrefix()},";
                    var aircraft = new Aircraft
                    {
                        Registry = registry,
                        TypeID = purchase.TypeID,
                        AirportICAO = aircraftDestinationICAO,
                        Fuel = aircraftType.FuelTotalCapacity * 0.25,
                        LifeTimeExpense = (int)(aircraftType.MaxPrice - volumeDiscount + deliveryCostPerAircraft),
                    };

                    if (!purchase.ForAirline)
                    {
                        aircraft.OwnerID = user.Id;
                    }
                    else
                    {
                        aircraft.AirlineOwnerID = user.AirlineICAO;
                    }

                    await this.db.Aircraft.AddAsync(aircraft);
                }

                registries = registries.TrimEnd(',');

                if (!purchase.ForAirline)
                {
                    if (grandTotalPrice > user.PersonalAccountBalance)
                    {
                        return new ApiResponse<string>("You can't afford this aircraft purchase!") { IsError = true };
                    }

                    user.PersonalAccountBalance -= aircraftType.MaxPrice;
                    var purchaseRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        UserID = user.Id,
                        Expense = (int)((aircraftType.MaxPrice - volumeDiscount) * purchase.NumberOfAircraft),
                        Category = FinancialCategory.Aircraft,
                        Description = $"Purchase of {purchase.NumberOfAircraft} new aircraft of type {aircraftType.Name} at airport {deliveryAirport.ICAO}"
                    };
                    await this.db.FinancialRecords.AddAsync(purchaseRecord);
                    if (deliveryCostPerAircraft > 0)
                    {
                        var ferryRecord = new FinancialRecord
                        {
                            ID = Guid.NewGuid(),
                            Timestamp = DateTime.UtcNow,
                            UserID = user.Id,
                            Expense = (int)(deliveryCostPerAircraft * purchase.NumberOfAircraft),
                            Category = FinancialCategory.Ferry,
                            Description = $"Ferry cost for {purchase.NumberOfAircraft} aircraft of type {aircraftType.Name} from {deliveryAirport.ICAO} to {aircraftDestinationICAO}."
                        };
                        await this.db.FinancialRecords.AddAsync(ferryRecord);
                    }
                }
                else
                {
                    if (grandTotalPrice > user.Airline.AccountBalance)
                    {
                        return new ApiResponse<string>("Your airline can't afford this aircraft purchase!") { IsError = true };
                    }

                    user.Airline.AccountBalance -= aircraftType.MaxPrice;
                    var airlinePurchaseRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        AirlineID = user.AirlineICAO,
                        Expense = (int)((aircraftType.MaxPrice - volumeDiscount) * purchase.NumberOfAircraft),
                        Category = FinancialCategory.Aircraft,
                        Description = $"Purchase of {purchase.NumberOfAircraft} new aircraft of type {aircraftType.Name} at airport {deliveryAirport.ICAO} by user {user.UserName}"
                    };
                    await this.db.FinancialRecords.AddAsync(airlinePurchaseRecord);
                    if (deliveryCostPerAircraft > 0)
                    {
                        var ferryRecord = new FinancialRecord
                        {
                            ID = Guid.NewGuid(),
                            Timestamp = DateTime.UtcNow,
                            AirlineID = user.AirlineICAO,
                            Expense = (int)(deliveryCostPerAircraft * purchase.NumberOfAircraft),
                            Category = FinancialCategory.Ferry,
                            Description = $"Ferry cost for {purchase.NumberOfAircraft} aircraft of type {aircraftType.Name} from {deliveryAirport.ICAO} to {aircraftDestinationICAO}."
                        };
                        await this.db.FinancialRecords.AddAsync(ferryRecord);
                    }
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error purchasing new aircraft");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully purchased {purchase.NumberOfAircraft} new aircraft of type \"{aircraftType.Name}\", and registered as {registries}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Aircraft/purchase/purchaseNew: {purchase.TypeID}@{purchase.DeliveryAirportICAO}");
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
                                                          (string.IsNullOrEmpty(search.Manufacturer) || a.Type.Manufacturer.Name.Contains(search.Manufacturer)) &&
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
                                                          (string.IsNullOrEmpty(search.Manufacturer) || a.Type.Manufacturer.Name.Contains(search.Manufacturer)) &&
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

                var missing = new AircraftManufacturer
                {
                    ID = "miss",
                    Name = "Missing"
                };
                foreach (var craft in searchResults)
                {
                    if (craft.OwnerID != user.Id && craft.AirlineOwnerID == user.AirlineICAO)
                    {
                        // User/Airline doesn't own this aircraft, zero out the financials
                        craft.LifeTimeExpense = 0;
                        craft.LifeTimeIncome = 0;
                    }

                    craft.Type.Manufacturer ??= missing;
                    foreach (var variant in craft.Type.Variants.Where(v => v.Manufacturer == null))
                    {
                        variant.Manufacturer = missing;
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
        /// Sell aircraft back to system.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2021.
        /// </remarks>
        /// <param name="registry">
        /// The registry of the aircraft to sell.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("sellToSystem/{registry}", Name = "SellAircraftToSystem")]
        public async Task<ActionResult<ApiResponse<string>>> SellAircraftToSystem(string registry)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Aircraft/sellToSystem/{registry}");
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

                // todo @todo update when economics/aircraft age/wear/tear are implemented
                var type = aircraft.Type;
                if (aircraft.Type.IsVariantOf.HasValue)
                {
                    // System only owns base variants
                    aircraft.TypeID = aircraft.Type.IsVariantOf.Value;
                    type = this.db.AircraftTypes.Single(t => t.ID == aircraft.Type.IsVariantOf.Value);
                }

                var random = new Random();
                var purchasePrice = (int)Math.Round((type.MaxPrice + type.MinPrice) / 2.0 * (random.Next(80, 121) / 100.0), 0);
                purchasePrice = Math.Max(Math.Min(type.MaxPrice, purchasePrice), type.MinPrice); // Make sure we stay within the min/max limit no matter what
                var rentPrice = purchasePrice / 200;

                if (aircraft.Owner != null)
                {
                    if (aircraft.OwnerID != user.Id)
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (aircraft.Status != "Idle")
                    {
                        return new ApiResponse<string>("Aircraft not idle!") { IsError = true };
                    }

                    if (this.db.Payloads.Any(p => p.AircraftRegistry == aircraft.Registry))
                    {
                        return new ApiResponse<string>("Aircraft not empty, please unload payload first!") { IsError = true };
                    }

                    var saleRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        UserID = aircraft.OwnerID,
                        Income = (int)(purchasePrice * 0.7),
                        Category = FinancialCategory.Aircraft,
                        Description = $"Sale of aircraft {aircraft.Registry.RemoveSimPrefix()}, type {aircraft.Type.Name} at airport {aircraft.AirportICAO}"
                    };

                    aircraft.OwnerID = null;
                    aircraft.Owner.PersonalAccountBalance += (int)(purchasePrice * 0.7);

                    await this.db.FinancialRecords.AddAsync(saleRecord);
                }

                if (aircraft.AirlineOwner != null)
                {
                    if (aircraft.AirlineOwnerID != user.AirlineICAO)
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (!AirlineController.UserHasPermission(user, AirlinePermission.SellAircraft))
                    {
                        return new ApiResponse<string> { Message = "You don't have the permission to sell aircraft for your airline!", IsError = true };
                    }

                    if (aircraft.Status != "Idle")
                    {
                        return new ApiResponse<string>("Aircraft not idle!") { IsError = true };
                    }

                    if (this.db.Payloads.Any(p => p.AircraftRegistry == aircraft.Registry))
                    {
                        return new ApiResponse<string>("Aircraft not empty, please unload payload first!") { IsError = true };
                    }

                    aircraft.AirlineOwnerID = null;
                    aircraft.AirlineOwner.AccountBalance += (int)(purchasePrice * 0.7);

                    var airlineSaleRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        AirlineID = aircraft.AirlineOwnerID,
                        Income = (int)(purchasePrice * 0.7),
                        Category = FinancialCategory.Aircraft,
                        Description = $"Sale of aircraft {aircraft.Registry.RemoveSimPrefix()}, type {aircraft.Type.Name} at airport {aircraft.AirportICAO}"
                    };
                    await this.db.FinancialRecords.AddAsync(airlineSaleRecord);
                }

                // User/Airline specific code finished, do the general changes
                aircraft.Name = null;
                aircraft.PurchasePrice = purchasePrice;
                aircraft.RentPrice = rentPrice;
                aircraft.LifeTimeExpense = 0;
                aircraft.LifeTimeIncome = 0;

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error selling aircraft.");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>($"Successfully sold aircraft {registry.RemoveSimPrefix()} for $B {purchasePrice:N0}.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Aircraft/sellToSystem/{registry}");
                return new ApiResponse<string>(ex);
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
                this.logger.LogInformation($"{this.User.Identity?.Name} | PUT Aircraft/update/{updateAircraft.Registry}");
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

                if (!aircraft.CanStartFlight)
                {
                    return new ApiResponse<string>("You currently can't edit this aircraft!") { IsError = true };
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

                return new ApiResponse<string>($"Successfully updated aircraft {updateAircraft.Registry.RemoveSimPrefix()}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | PUT Aircraft/update/{updateAircraft.Registry}");
                return new ApiResponse<string>(ex);
            }
        }
    }
}