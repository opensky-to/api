// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobController.cs" company="OpenSky">
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
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<string>("Unable to find user record!") { IsError = true };
                }

                var job = await this.db.Jobs.SingleOrDefaultAsync(j => j.ID == jobID);
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

                    // todo deduct cancellation fee from airline account balance
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
        /// Gets the available jobs at the specified airport.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/12/2021.
        /// </remarks>
        /// <param name="icao">
        /// The ICAO code of the airport.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the jobs at airport.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("atAirport/{icao}", Name = "GetJobsAtAirport")]
        public async Task<ActionResult<ApiResponse<IEnumerable<Job>>>> GetJobsAtAirport(string icao)
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Job/atAirport/{icao}");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<Job>>("Unable to find user record!") { IsError = true, Data = new List<Job>() };
                }

                // Make sure there are enough jobs at this airport
                var jobResult = await this.jobPopulator.CheckAndGenerateJobsForAirport(icao);
                this.logger.LogInformation(jobResult);

                var jobs = await this.db.Jobs.Where(j => j.OriginICAO == icao && j.OperatorID == null && j.OperatorAirlineID == null).ToListAsync();

                return new ApiResponse<IEnumerable<Job>>(jobs);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Job/atAirport/{icao}");
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