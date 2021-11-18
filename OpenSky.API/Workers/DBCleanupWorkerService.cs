// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbCleanupWorkerService.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Workers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.Model;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// DB cleanup background worker service.
    /// </summary>
    /// <remarks>
    /// Flusinerd, 13/06/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.Extensions.Hosting.BackgroundService"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class DbCleanupWorkerService : BackgroundService
    {
        /// -------------------------------------------------------------------------------------------------        
        /// <summary>
        /// The clean up interval in milliseconds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private const int CleanupInterval = 30 * 60 * 1000;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<DbCleanupWorkerService> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The services.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly IServiceProvider services;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes static members of the <see cref="DbCleanupWorkerService"/> class.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 13/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        static DbCleanupWorkerService()
        {
            Status = new Dictionary<Guid, DataImportStatus>();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DbCleanupWorkerService"/> class.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 13/06/2021.
        /// </remarks>
        /// <param name="services">
        /// The services.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public DbCleanupWorkerService(
            IServiceProvider services,
            ILogger<DbCleanupWorkerService> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the status dictionary.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static Dictionary<Guid, DataImportStatus> Status { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 13/06/2021.
        /// </remarks>
        /// <param name="stoppingToken">
        /// Indicates that the shutdown process should no longer be graceful.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// <seealso cref="M:Microsoft.Extensions.Hosting.BackgroundService.StopAsync(CancellationToken)"/>
        /// -------------------------------------------------------------------------------------------------
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("DB cleanup background service stopping...");
            await base.StopAsync(stoppingToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" />
        /// starts. The implementation should return a task that represents the lifetime of the long
        /// running operation(s) being performed.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 13/06/2021.
        /// </remarks>
        /// <param name="stoppingToken">
        /// Triggered when
        /// <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" />
        /// is called.
        /// </param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.
        /// </returns>
        /// <seealso cref="M:Microsoft.Extensions.Hosting.BackgroundService.ExecuteAsync(CancellationToken)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("DB cleanup background service starting...");
            await this.CleanupOpenSkyTokens(stoppingToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cleans up expired OpenSkyTokens from the Database. Cleanup interval can be configured via the
        /// workers cleanupInterval.
        /// </summary>
        /// <remarks>
        /// Flusinerd, 14/06/2021.
        /// </remarks>
        /// <param name="stoppingToken">
        /// Triggered when
        /// <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" />
        /// is called.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task CleanupOpenSkyTokens(CancellationToken stoppingToken)
        {
            using var scope = this.services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OpenSkyDbContext>();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Delete OpenSkyTokens that are expired
                    var tokens = db.OpenSkyTokens.Where(token => DateTime.UtcNow > token.Expiry);
                    db.OpenSkyTokens.RemoveRange(tokens);
                    await db.SaveChangesAsync(stoppingToken);

                    await Task.Delay(CleanupInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error cleaning up OpenSky tokens.");
                    await Task.Delay(30 * 1000, stoppingToken);
                }
            }
        }
    }
}