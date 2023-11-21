// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportWorkerService.LittleNavmapMSFS.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Workers
{
    using System;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Data import worker service - LittleNavmap MSFS code.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class DataImportWorkerService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Process uploaded LittleNavmap MSFS sqlite database in a background worker.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// <param name="db">
        /// The OpenSky database context.
        /// </param>
        /// <param name="dataImport">
        /// The data import.
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task ImportLittleNavmapMSFS(OpenSkyDbContext db, DataImport dataImport, CancellationToken token)
        {
            this.logger.LogInformation($"Processing LittleNavmap MSFS data import {dataImport.ID} for file {dataImport.ImportDataSource}");
            var totalRecordsProcessed = 0;
            try
            {
                if (!File.Exists(dataImport.ImportDataSource))
                {
                    throw new Exception($"SQlite database {dataImport.ImportDataSource} does not exist!");
                }

                var connection = new SQLiteConnection($"URI=file:{dataImport.ImportDataSource};Read Only=true");
                connection.Open();
                try
                {
                    var dataImportStatus = new DataImportStatus();
                    Status.Add(dataImport.ID, dataImportStatus);
                    await this.ImportLittleNavmapPopulateImportStatus(connection, dataImportStatus, token);

                    var rowsAffected = await db.ResetAirportsMSFS();
                    this.logger.LogInformation($"Successfully reset the MSFS flag on {rowsAffected} airports.");
                    totalRecordsProcessed += await this.ImportLittleNavmapAirports(db, dataImport, connection, Simulator.MSFS, token);

                    this.logger.LogInformation("Clearing down existing MSFS runways and runway ends before importing new ones...");
                    await db.BulkDeleteAsync(db.RunwayEnds.Where(re => re.ID < 1000000), token);
                    await db.BulkDeleteAsync(db.Runways.Where(r => r.ID < 1000000), token);
                    totalRecordsProcessed += await this.ImportLittleNavmapRunways(db, dataImport, connection, Simulator.MSFS, token);
                    totalRecordsProcessed += await this.ImportLittleNavmapRunwayEnds(db, dataImport, connection, Simulator.MSFS, token);

                    this.logger.LogInformation("Clearing down existing MSFS approaches before importing new ones...");
                    await db.BulkDeleteAsync(db.Approaches.Where(a => a.ID < 1000000), token);
                    totalRecordsProcessed += await this.ImportLittleNavmapApproaches(db, dataImport, connection, Simulator.MSFS, token);

                    // Call the general airport size calculator as part of our import
                    totalRecordsProcessed += await this.CalculateAirportSizes(db, dataImport, token);
                }
                finally
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error processing LittleNavmap MSFS data import {dataImport.ID} for file {dataImport.ImportDataSource}");
                dataImport.ImportStatusJson = $"ERROR processing: {ex.Message}";
            }
            finally
            {
                // Save the outcome of the data import to the database (we are not cancelling this if the server wants to shut down)
                dataImport.Finished = DateTime.UtcNow;
                dataImport.TotalRecordsProcessed = totalRecordsProcessed;
                dataImport.ImportStatusJson = JsonSerializer.Serialize(Status[dataImport.ID]);
                await db.SaveDatabaseChangesAsync(this.logger, $"Error saving data import result to database for {dataImport.ID}");

                this.logger.LogInformation($"Finished processing LittleNavmap MSFS data import {dataImport.ID}, {totalRecordsProcessed} total records processed in {(DateTime.UtcNow - dataImport.Started).TotalMinutes:F1} minutes.");

                // Delete the temporary file
                if (File.Exists(dataImport.ImportDataSource))
                {
                    File.Delete(dataImport.ImportDataSource);
                }
            }
        }
    }
}