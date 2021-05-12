// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportWorker.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Workers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Data import background worker service.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/05/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.Extensions.Hosting.BackgroundService"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class DataImportWorkerService : BackgroundService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<DataImportWorkerService> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DataImportWorkerService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// <param name="services">
        /// The services.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public DataImportWorkerService(
            IServiceProvider services,
            ILogger<DataImportWorkerService> logger)
        {
            this.Services = services;
            this.logger = logger;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the services.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IServiceProvider Services { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
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
            this.logger.LogInformation("Data import background service stopping...");
            await base.StopAsync(stoppingToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" />
        /// starts. The implementation should return a task that represents the lifetime of the long
        /// running operation(s) being performed.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
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
            this.logger.LogInformation("Data import background service starting...");
            await this.ProcessDataImports(stoppingToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Process data import tasks.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
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
        private async Task ProcessDataImports(CancellationToken stoppingToken)
        {
            using var scope = this.Services.CreateScope();
            while (!stoppingToken.IsCancellationRequested)
            {
                var db = scope.ServiceProvider.GetRequiredService<OpenSkyDbContext>();
                var unfinishedImport = await db.DataImports.FirstOrDefaultAsync(i => !i.Finished.HasValue, stoppingToken);
                if (unfinishedImport != null)
                {
                    switch (unfinishedImport.Type)
                    {
                        case "LittleNavmapMSFS":
                            await this.ImportLittleNavmapMSFS(db, unfinishedImport, stoppingToken);
                            break;
                    }
                }
                else
                {
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }
    }
}