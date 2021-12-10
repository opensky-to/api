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
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;

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
        /// -------------------------------------------------------------------------------------------------
        public JobController(ILogger<JobController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
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

                var jobs = await this.db.Jobs.Where(j => j.OriginICAO == icao && j.OperatorID == null && j.OperatorAirlineID == null).ToListAsync();

                return new ApiResponse<IEnumerable<Job>>(jobs);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Job/atAirport/{icao}");
                return new ApiResponse<IEnumerable<Job>>(ex) { Data = new List<Job>() };
            }
        }
    }
}