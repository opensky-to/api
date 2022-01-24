﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinancialController.cs" company="OpenSky">
// OpenSky project 2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;
    using OpenSky.API.Model.Financial;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Financial controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/01/2022.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.User)]
    [ApiController]
    [Microsoft.AspNetCore.Components.Route("[controller]")]
    public class FinancialController : ControllerBase
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
        private readonly ILogger<FinancialController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user manager.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly UserManager<OpenSkyUser> userManager;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/01/2022.
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
        public FinancialController(ILogger<FinancialController> logger, OpenSkyDbContext db, UserManager<OpenSkyUser> userManager)
        {
            this.logger = logger;
            this.db = db;
            this.userManager = userManager;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get financial overview (with transactions of the last 30 days)
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/01/2022.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the financial overview.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("overview", Name = "GetFinancialOverview")]
        public async Task<ActionResult<ApiResponse<FinancialOverview>>> GetFinancialOverview()
        {
            try
            {
                this.logger.LogInformation($"{this.User.Identity?.Name} | GET Financial/overview");
                var user = await this.userManager.FindByNameAsync(this.User.Identity?.Name);
                if (user == null)
                {
                    return new ApiResponse<FinancialOverview> { Message = "Unable to find user record!", IsError = true };
                }

                var overview = new FinancialOverview
                {
                    AccountBalance = user.PersonalAccountBalance,
                    RecentFinancialRecords = user.FinancialRecords.Where(f => f.ParentRecordID == null && f.Timestamp >= DateTime.UtcNow.AddDays(-30)).ToList()
                };

                if (!string.IsNullOrEmpty(user.AirlineICAO) && AirlineController.UserHasPermission(user, AirlinePermission.FinancialRecords))
                {
                    overview.AirlineAccountBalance = user.Airline.AccountBalance;
                    overview.RecentAirlineFinancialRecords = user.Airline.FinancialRecords.Where(f => f.ParentRecordID == null && f.Timestamp >= DateTime.UtcNow.AddDays(-30)).ToList();
                }

                return new ApiResponse<FinancialOverview>(overview);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{this.User.Identity?.Name} | GET Financial/overview");
                return new ApiResponse<FinancialOverview>(ex);
            }
        }
    }
}