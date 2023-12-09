// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobController.cs" company="OpenSky">
// OpenSky project 2021-2023
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
    using OpenSky.API.Model.Job;
    using OpenSky.API.Services;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Job controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 10/12/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Route("[controller]")]
    public class JobController : ControllerBase
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky database context.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly OpenSkyDbContext db;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The job populator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly JobPopulatorService jobPopulator;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<JobController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserManager<OpenSkyUser> userManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="JobController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
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
        /// <param name="jobPopulator">
        /// The job populator.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public JobController(ILogger<JobController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager, JobPopulatorService jobPopulator)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
            this.jobPopulator = jobPopulator;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Aborts the specified job.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2021.
        /// </remarks>
        /// <param name="jobID">
        /// Identifier for the job.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpDelete("abort/{jobID:guid}", Name = "AbortJob")]
        public async Task<ActionResult<ApiResponse<string>>> AbortJob(Guid jobID)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Job/abort/{jobID}");

                // ReSharper disable AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                var job = await this.db.Jobs
                                    .Include(job => job.OperatorAirline)
                                    .Include(job => job.Payloads)
                                    .ThenInclude(payload => payload.FlightPayloads)
                                    .ThenInclude(flightPayload => flightPayload.Flight)
                                    .SingleOrDefaultAsync(j => j.ID == jobID);
                if (job == null)
                {
                    return new ApiResponse<string>("Job not found!") { IsError = true };
                }

                if (!string.IsNullOrEmpty(job.OperatorID))
                {
                    // Personal job
                    if (!job.OperatorID.Equals(user.Id))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    // Deduct 30% of job value as penalty
                    user.PersonalAccountBalance -= (int)(job.Value * 0.3);
                    var penaltyRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        UserID = job.OperatorID,
                        Category = FinancialCategory.Fines,
                        Expense = (int)(job.Value * 0.3),
                        Description = $"Cancellation penalty for job{(string.IsNullOrEmpty(job.UserIdentifier) ? string.Empty : $" ({job.UserIdentifier})")} of type {job.Category} from {job.OriginICAO}"
                    };

                    await this.db.FinancialRecords.AddAsync(penaltyRecord);
                }
                else if (!string.IsNullOrEmpty(job.OperatorAirlineID))
                {
                    // Airline job
                    if (!job.OperatorAirlineID.Equals(user.AirlineICAO))
                    {
                        return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                    }

                    if (!AirlineController.UserHasPermission(user, AirlinePermission.AbortJobs))
                    {
                        return new ApiResponse<string> { Message = "You don't have the permission to abort jobs for your airline!", IsError = true };
                    }

                    // Deduct 30% of job value as penalty
                    job.OperatorAirline.AccountBalance -= (int)(job.Value * 0.3);
                    var airlinePenaltyRecord = new FinancialRecord
                    {
                        ID = Guid.NewGuid(),
                        Timestamp = DateTime.UtcNow,
                        AirlineID = job.OperatorAirlineID,
                        Expense = (int)(job.Value * 0.3),
                        Category = FinancialCategory.Fines,
                        Description = $"Cancellation penalty for job{(string.IsNullOrEmpty(job.UserIdentifier) ? string.Empty : $" ({job.UserIdentifier})")} of type {job.Category} from {job.OriginICAO}"
                    };

                    await this.db.FinancialRecords.AddAsync(airlinePenaltyRecord);
                }
                else
                {
                    return new ApiResponse<string>("Unauthorized request!") { IsError = true };
                }

                // Are any payloads currently loaded onto any aircraft?
                if (job.Payloads.Any(p => !string.IsNullOrEmpty(p.AircraftRegistry)))
                {
                    return new ApiResponse<string>("One or more the job's payloads are currently loaded onto an aircraft, you have to unload them before aborting the job!") { IsError = true };
                }

                // Are any payloads currently part of an active flight?
                if (job.Payloads.Any(p => p.FlightPayloads.Any(f => f.Flight.Started.HasValue && !f.Flight.Completed.HasValue)))
                {
                    return new ApiResponse<string>("One or more the job's payloads are currently part of an active flight, you have to abort the flight before aborting the job!") { IsError = true };
                }

                // Remove non-active flight payloads
                foreach (var jobPayload in job.Payloads)
                {
                    this.db.FlightPayloads.RemoveRange(jobPayload.FlightPayloads);
                }

                // Remove job and it's payloads
                this.db.Payloads.RemoveRange(job.Payloads);
                this.db.Jobs.Remove(job);

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error aborting job");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>("Successfully aborted job.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Job/abort/{jobID}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Accept the specified job.
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2021.
        /// </remarks>
        /// <param name="jobID">
        /// Identifier for the job.
        /// </param>
        /// <param name="forAirline">
        /// True to accept the job for the airline, false for private job.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an ActionResult&lt;ApiResponse&lt;string&gt;&gt;
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost("accept/{jobID:guid}/{forAirline:bool}", Name = "AcceptJob")]
        public async Task<ActionResult<ApiResponse<string>>> AcceptJob(Guid jobID, bool forAirline)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | POST Job/accept/{jobID}/{forAirline}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                if (string.IsNullOrEmpty(user.AirlineICAO) && forAirline)
                {
                    return new ApiResponse<string>("Not member of an airline!") { IsError = true };
                }

                if (!AirlineController.UserHasPermission(user, AirlinePermission.AcceptJobs) && forAirline)
                {
                    return new ApiResponse<string> { Message = "You don't have the permission to accept jobs for your airline!", IsError = true };
                }

                var job = await this.db.Jobs.SingleOrDefaultAsync(j => j.ID == jobID);
                if (job == null)
                {
                    return new ApiResponse<string>("Job not found!") { IsError = true };
                }

                if (DateTime.UtcNow > job.ExpiresAt)
                {
                    return new ApiResponse<string>("This job is no longer available!") { IsError = true };
                }

                if (!string.IsNullOrEmpty(job.OperatorID) || !string.IsNullOrEmpty(job.OperatorAirlineID))
                {
                    return new ApiResponse<string>("This job is no longer available!") { IsError = true };
                }

                if (!forAirline)
                {
                    job.OperatorID = user.Id;
                }
                else
                {
                    job.OperatorAirlineID = user.AirlineICAO;
                }

                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error accepting job");
                if (saveEx != null)
                {
                    throw saveEx;
                }

                return new ApiResponse<string>("Successfully accepted job.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | POST Job/accept/{jobID}/{forAirline}");
                return new ApiResponse<string>(ex);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available jobs at the specified airport and simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
        /// </remarks>
        /// <param name="icao">
        /// The ICAO code of the airport.
        /// </param>
        /// <param name="direction">
        /// The direction of the jobs to return.
        /// </param>
        /// <param name="simulator">
        /// The simulator (or NULL for all simulators).
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the jobs at airport.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("atAirportForSim/{icao}/{direction}/{simulator}", Name = "GetJobsAtAirportForSimulator")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Job>>>> GetJobsAtAirportForSimulator(string icao, JobDirection direction, Simulator simulator)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Job/atAirport/{icao}/{direction}/{simulator}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<Job>>("Unable to find user record!") { IsError = true, Data = new List<Job>() };
                }

                // Make sure there are enough jobs at this airport
                var jobResult = await this.jobPopulator.CheckAndGenerateJobsForAirport(icao, direction, null, simulator);
                this.logger.LogInformation(jobResult);

                if (direction == JobDirection.From)
                {
                    var jobs = await this.db.Jobs
                                         .Where(j => j.OriginICAO == icao && j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now)
                                         .Include(job => job.Origin)
                                         .Include(job => job.Payloads)
                                         .ThenInclude(payload => payload.Destination)
                                         .ToListAsync();

                    if (simulator == Simulator.MSFS)
                    {
                        jobs = jobs.Where(j => j.Origin.MSFS && j.Payloads.All(p => p.Destination.MSFS)).ToList();
                    }

                    if (simulator == Simulator.XPlane11)
                    {
                        jobs = jobs.Where(j => j.Origin.XP11 && j.Payloads.All(p => p.Destination.XP11)).ToList();
                    }

                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                if (direction == JobDirection.To)
                {
                    var jobs = await this.db.Jobs
                                         .Where(j => j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now && j.Payloads.Any(p => p.DestinationICAO == icao))
                                         .Include(job => job.Origin)
                                         .Include(job => job.Payloads)
                                         .ThenInclude(payload => payload.Destination)
                                         .ToListAsync();

                    if (simulator == Simulator.MSFS)
                    {
                        jobs = jobs.Where(j => j.Origin.MSFS && j.Payloads.All(p => p.Destination.MSFS)).ToList();
                    }

                    if (simulator == Simulator.XPlane11)
                    {
                        jobs = jobs.Where(j => j.Origin.XP11 && j.Payloads.All(p => p.Destination.XP11)).ToList();
                    }

                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                if (direction == JobDirection.RoundTrip)
                {
                    var jobs = await this.db.Jobs
                                         .Where(j => j.OriginICAO == icao && j.OperatorID == null && j.ExpiresAt > DateTime.Now && j.OperatorAirlineID == null && j.Payloads.Any(p => p.DestinationICAO == icao))
                                         .Include(job => job.Origin)
                                         .Include(job => job.Payloads)
                                         .ThenInclude(payload => payload.Destination)
                                         .ToListAsync();

                    if (simulator == Simulator.MSFS)
                    {
                        jobs = jobs.Where(j => j.Origin.MSFS && j.Payloads.All(p => p.Destination.MSFS)).ToList();
                    }

                    if (simulator == Simulator.XPlane11)
                    {
                        jobs = jobs.Where(j => j.Origin.XP11 && j.Payloads.All(p => p.Destination.XP11)).ToList();
                    }

                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                throw new Exception("Unsupported job direction.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Job/atAirport/{icao}/{direction}/{simulator}");
                return new ApiResponse<IEnumerable<Job>>(ex) { Data = new List<Job>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available jobs at the specified airport.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
        /// </remarks>
        /// <param name="icao">
        /// The ICAO code of the airport.
        /// </param>
        /// <param name="direction">
        /// The direction of the jobs to return.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the jobs at airport.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("atAirport/{icao}/{direction}", Name = "GetJobsAtAirport")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Job>>>> GetJobsAtAirport(string icao, JobDirection direction)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Job/atAirport/{icao}/{direction}");

                // ReSharper disable AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<Job>>("Unable to find user record!") { IsError = true, Data = new List<Job>() };
                }

                // Make sure there are enough jobs at this airport
                var jobResult = await this.jobPopulator.CheckAndGenerateJobsForAirport(icao, direction);
                this.logger.LogInformation(jobResult);

                if (direction == JobDirection.From)
                {
                    var jobs = await this.db.Jobs.Where(j => j.OriginICAO == icao && j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now).ToListAsync();
                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                if (direction == JobDirection.To)
                {
                    var jobs = await this.db.Jobs.Where(j => j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now && j.Payloads.Any(p => p.DestinationICAO == icao)).ToListAsync();
                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                if (direction == JobDirection.RoundTrip)
                {
                    var jobs = await this.db.Jobs.Where(j => j.OriginICAO == icao && j.OperatorID == null && j.ExpiresAt > DateTime.Now && j.OperatorAirlineID == null && j.Payloads.Any(p => p.DestinationICAO == icao)).ToListAsync();
                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                throw new Exception("Unsupported job direction.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Job/atAirport/{icao}/{direction}");
                return new ApiResponse<IEnumerable<Job>>(ex) { Data = new List<Job>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available jobs at the specified airport for the specified aircraft type category and simulator.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="icao">
        /// The ICAO code of the airport.
        /// </param>
        /// <param name="direction">
        /// The direction of the jobs to return.
        /// </param>
        /// <param name="category">
        /// The aircraft type category to return jobs for (recommended category).
        /// </param>
        /// <param name="simulator">
        /// The simulator (or NULL for all simulators).
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the jobs at airport.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("atAirportForCategoryAndSim/{icao}/{direction}/{category}/{simulator}", Name = "GetJobsAtAirportForCategoryAndSimulator")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Job>>>> GetJobsAtAirportForCategoryAndSimulator(string icao, JobDirection direction, AircraftTypeCategory category, Simulator simulator)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Job/atAirport/{icao}/{direction}/{category}/{simulator}");

                // ReSharper disable AssignNullToNotNullAttribute
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<Job>>("Unable to find user record!") { IsError = true, Data = new List<Job>() };
                }

                // Make sure there are enough jobs at this airport
                var jobResult = await this.jobPopulator.CheckAndGenerateJobsForAirport(icao, direction, category, simulator);
                this.logger.LogInformation(jobResult);

                if (direction == JobDirection.From)
                {
                    var jobs = await this.db.Jobs
                                         .Where(j => j.OriginICAO == icao && j.Category == category && j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now)
                                         .Include(job => job.Origin)
                                         .Include(job => job.Payloads)
                                         .ThenInclude(payload => payload.Destination)
                                         .ToListAsync();

                    if (simulator == Simulator.MSFS)
                    {
                        jobs = jobs.Where(j => j.Origin.MSFS && j.Payloads.All(p => p.Destination.MSFS)).ToList();
                    }

                    if (simulator == Simulator.XPlane11)
                    {
                        jobs = jobs.Where(j => j.Origin.XP11 && j.Payloads.All(p => p.Destination.XP11)).ToList();
                    }

                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                if (direction == JobDirection.To)
                {
                    var jobs = await this.db.Jobs
                                         .Where(j => j.Category == category && j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now && j.Payloads.Any(p => p.DestinationICAO == icao))
                                         .Include(job => job.Origin)
                                         .Include(job => job.Payloads)
                                         .ThenInclude(payload => payload.Destination)
                                         .ToListAsync();

                    if (simulator == Simulator.MSFS)
                    {
                        jobs = jobs.Where(j => j.Origin.MSFS && j.Payloads.All(p => p.Destination.MSFS)).ToList();
                    }

                    if (simulator == Simulator.XPlane11)
                    {
                        jobs = jobs.Where(j => j.Origin.XP11 && j.Payloads.All(p => p.Destination.XP11)).ToList();
                    }

                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                if (direction == JobDirection.RoundTrip)
                {
                    var jobs = await this.db.Jobs
                                         .Where(j => j.OriginICAO == icao && j.Category == category && j.OperatorID == null && j.ExpiresAt > DateTime.Now && j.OperatorAirlineID == null && j.Payloads.Any(p => p.DestinationICAO == icao))
                                         .Include(job => job.Origin)
                                         .Include(job => job.Payloads)
                                         .ThenInclude(payload => payload.Destination)
                                         .ToListAsync();

                    if (simulator == Simulator.MSFS)
                    {
                        jobs = jobs.Where(j => j.Origin.MSFS && j.Payloads.All(p => p.Destination.MSFS)).ToList();
                    }

                    if (simulator == Simulator.XPlane11)
                    {
                        jobs = jobs.Where(j => j.Origin.XP11 && j.Payloads.All(p => p.Destination.XP11)).ToList();
                    }

                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                throw new Exception("Unsupported job direction.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Job/atAirport/{icao}/{direction}/{category}/{simulator}");
                return new ApiResponse<IEnumerable<Job>>(ex) { Data = new List<Job>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available jobs at the specified airport for the specified aircraft type category.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="icao">
        /// The ICAO code of the airport.
        /// </param>
        /// <param name="direction">
        /// The direction of the jobs to return.
        /// </param>
        /// <param name="category">
        /// The aircraft type category to return jobs for (recommended category).
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the jobs at airport.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("atAirportForCategory/{icao}/{direction}/{category}", Name = "GetJobsAtAirportForCategory")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Job>>>> GetJobsAtAirportForCategory(string icao, JobDirection direction, AircraftTypeCategory category)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Job/atAirport/{icao}/{direction}/{category}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<Job>>("Unable to find user record!") { IsError = true, Data = new List<Job>() };
                }

                // Make sure there are enough jobs at this airport
                var jobResult = await this.jobPopulator.CheckAndGenerateJobsForAirport(icao, direction, category);
                this.logger.LogInformation(jobResult);

                if (direction == JobDirection.From)
                {
                    var jobs = await this.db.Jobs.Where(j => j.OriginICAO == icao && j.Category == category && j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now).ToListAsync();
                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                if (direction == JobDirection.To)
                {
                    var jobs = await this.db.Jobs.Where(j => j.Category == category && j.OperatorID == null && j.OperatorAirlineID == null && j.ExpiresAt > DateTime.Now && j.Payloads.Any(p => p.DestinationICAO == icao)).ToListAsync();
                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                if (direction == JobDirection.RoundTrip)
                {
                    var jobs = await this.db.Jobs.Where(j => j.OriginICAO == icao && j.Category == category && j.OperatorID == null && j.ExpiresAt > DateTime.Now && j.OperatorAirlineID == null && j.Payloads.Any(p => p.DestinationICAO == icao))
                                         .ToListAsync();
                    return new ApiResponse<IEnumerable<Job>>(jobs);
                }

                throw new Exception("Unsupported job direction.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Job/atAirport/{icao}/{direction}/{category}");
                return new ApiResponse<IEnumerable<Job>>(ex) { Data = new List<Job>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get "my" jobs, both personal and airline (active only)
        /// </summary>
        /// <remarks>
        /// sushi.at, 18/12/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields my jobs.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("myJobs", Name = "GetMyJobs")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Job>>>> GetMyJobs()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Job/myJobs");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<Job>>("Unable to find user record!") { IsError = true, Data = new List<Job>() };
                }

                var jobs = await this.db.Jobs.Where(j => j.OperatorID == user.Id).ToListAsync();

                if (!string.IsNullOrEmpty(user.AirlineICAO) && AirlineController.UserHasPermission(user, AirlinePermission.Dispatch))
                {
                    jobs.AddRange(await this.db.Jobs.Where(f => f.OperatorAirlineID == user.AirlineICAO && (f.AssignedAirlineDispatcherID == null || f.AssignedAirlineDispatcherID == user.Id)).ToListAsync());
                }

                return new ApiResponse<IEnumerable<Job>>(jobs);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Job/myJobs");
                return new ApiResponse<IEnumerable<Job>>(ex) { Data = new List<Job>() };
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get plannable payloads plus basic information about flights they are already planned for.
        /// </summary>
        /// <remarks>
        /// sushi.at, 19/12/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the plannable payloads.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("plannablePayloads", Name = "GetPlannablePayloads")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PlannablePayload>>>> GetPlannablePayloads()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Job/plannablePayloads");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<PlannablePayload>>("Unable to find user record!") { IsError = true, Data = new List<PlannablePayload>() };
                }

                var jobIDs = await this.db.Jobs.Where(j => j.OperatorID == user.Id).Select(j => j.ID).ToListAsync();
                if (!string.IsNullOrEmpty(user.AirlineICAO) && AirlineController.UserHasPermission(user, AirlinePermission.Dispatch))
                {
                    jobIDs.AddRange(await this.db.Jobs.Where(f => f.OperatorAirlineID == user.AirlineICAO && (f.AssignedAirlineDispatcherID == null || f.AssignedAirlineDispatcherID == user.Id)).Select(j => j.ID).ToListAsync());
                }

                var plannablePayloads = this.db.Payloads.Where(p => jobIDs.Contains(p.JobID)).ToList().Select(p => new PlannablePayload(p));

                return new ApiResponse<IEnumerable<PlannablePayload>>(plannablePayloads);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Job/plannablePayloads");
                return new ApiResponse<IEnumerable<PlannablePayload>>(ex) { Data = new List<PlannablePayload>() };
            }
        }
    }
}