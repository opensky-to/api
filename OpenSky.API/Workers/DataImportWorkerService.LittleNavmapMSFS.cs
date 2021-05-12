﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportWorkerService.LittleNavmapMSFS.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Workers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.Helpers;
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
        /// Import the airports from an uploaded LittleNavmap database for MSFS.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/05/2021.
        /// </remarks>
        /// <param name="db">
        /// The OpenSky database context.
        /// </param>
        /// <param name="dataImport">
        /// The data import.
        /// </param>
        /// <param name="connection">
        /// The sqlite database connection.
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (airports processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> ImportLittleNavmapAirportsMSFS(OpenSkyDbContext db, DataImport dataImport, SQLiteConnection connection, CancellationToken token)
        {
            var lastIdent = "???";

            db.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                var airportCommand = new SQLiteCommand(
                    "SELECT " +
                    "ident,name,city,has_avgas,has_jetfuel,tower_frequency,atis_frequency,unicom_frequency,is_closed,is_military," +
                    "num_parking_gate,num_parking_ga_ramp,num_runways,longest_runway_length,longest_runway_surface,laty,lonx " +
                    "FROM airport",
                    connection);

                var newAirports = new List<Airport>();
                var updatedAirports = new List<Airport>();
                var reader = await airportCommand.ExecuteReaderAsync(token);
                await using (reader)
                {
                    while (await reader.ReadAsync(token))
                    {
                        if (token.IsCancellationRequested)
                        {
                            this.logger.LogWarning($"Aborting data import of {dataImport.ID}, the server is shutting down...");
                            break;
                        }

                        var ident = reader.GetString("ident");
                        lastIdent = ident;
                        var existingAirportHash = await db.Airports.Select(a => new { a.ICAO, a.HashCode }).SingleOrDefaultAsync(a => a.ICAO == ident, token);
                        if (existingAirportHash == null)
                        {
                            Status[dataImport.ID].Elements["airport"].New++;
                            var newAirport = new Airport
                            {
                                ICAO = ident,
                                Name = !await reader.IsDBNullAsync("name", token) ? new string(reader.GetString("name").Take(50).ToArray()) : "???",
                                City = !await reader.IsDBNullAsync("city", token) ? new string(reader.GetString("city").Take(50).ToArray()) : null,
                                HasAvGas = reader.GetBoolean("has_avgas"),
                                HasJetFuel = reader.GetBoolean("has_jetfuel"),
                                TowerFrequency = !await reader.IsDBNullAsync("tower_frequency", token) ? (reader.GetInt32("tower_frequency") != 0 ? reader.GetInt32("tower_frequency") : null) : null,
                                AtisFrequency = !await reader.IsDBNullAsync("atis_frequency", token) ? (reader.GetInt32("atis_frequency") != 0 ? reader.GetInt32("atis_frequency") : null) : null,
                                UnicomFrequency = !await reader.IsDBNullAsync("unicom_frequency", token) ? (reader.GetInt32("unicom_frequency") != 0 ? reader.GetInt32("unicom_frequency") : null) : null,
                                IsClosed = reader.GetBoolean("is_closed"),
                                IsMilitary = reader.GetBoolean("is_military"),
                                Gates = reader.GetInt32("num_parking_gate"),
                                GaRamps = reader.GetInt32("num_parking_ga_ramp"),
                                RunwayCount = reader.GetInt32("num_runways"),
                                LongestRunwayLength = reader.GetInt32("longest_runway_length"),
                                LongestRunwaySurface = reader.GetString("longest_runway_surface"),
                                Latitude = reader.GetDouble("laty"),
                                Longitude = reader.GetDouble("lonx"),
                                HashCode = reader.CalculateHashCode()
                            };
                            newAirports.Add(newAirport);
                        }
                        else
                        {
                            if (existingAirportHash.HashCode != reader.CalculateHashCode())
                            {
                                Status[dataImport.ID].Elements["airport"].Updated++;
                                var existingAirport = await db.Airports.SingleAsync(a => a.ICAO == ident, token);
                                existingAirport.Name = !await reader.IsDBNullAsync("name", token) ? new string(reader.GetString("name").Take(50).ToArray()) : "???";
                                existingAirport.City = !await reader.IsDBNullAsync("city", token) ? new string(reader.GetString("city").Take(50).ToArray()) : null;
                                existingAirport.HasAvGas = reader.GetBoolean("has_avgas");
                                existingAirport.HasJetFuel = reader.GetBoolean("has_jetfuel");
                                existingAirport.TowerFrequency = !await reader.IsDBNullAsync("tower_frequency", token) ? (reader.GetInt32("tower_frequency") != 0 ? reader.GetInt32("tower_frequency") : null) : null;
                                existingAirport.AtisFrequency = !await reader.IsDBNullAsync("atis_frequency", token) ? (reader.GetInt32("atis_frequency") != 0 ? reader.GetInt32("atis_frequency") : null) : null;
                                existingAirport.UnicomFrequency = !await reader.IsDBNullAsync("unicom_frequency", token) ? (reader.GetInt32("unicom_frequency") != 0 ? reader.GetInt32("unicom_frequency") : null) : null;
                                existingAirport.IsClosed = reader.GetBoolean("is_closed");
                                existingAirport.IsMilitary = reader.GetBoolean("is_military");
                                existingAirport.Gates = reader.GetInt32("num_parking_gate");
                                existingAirport.GaRamps = reader.GetInt32("num_parking_ga_ramp");
                                existingAirport.RunwayCount = reader.GetInt32("num_runways");
                                existingAirport.LongestRunwayLength = reader.GetInt32("longest_runway_length");
                                existingAirport.LongestRunwaySurface = reader.GetString("longest_runway_surface");
                                existingAirport.Latitude = reader.GetDouble("laty");
                                existingAirport.Longitude = reader.GetDouble("lonx");
                                existingAirport.HashCode = reader.CalculateHashCode();
                                updatedAirports.Add(existingAirport);
                            }
                            else
                            {
                                Status[dataImport.ID].Elements["airport"].Skipped++;
                            }
                        }

                        Status[dataImport.ID].Processed++;
                        Status[dataImport.ID].Elements["airport"].Processed++;
                        if (Status[dataImport.ID].Processed % 1000 == 0)
                        {
                            this.logger.LogInformation($"Data import {dataImport.ID} has processed {Status[dataImport.ID].Processed} of {Status[dataImport.ID].Total} [{Status[dataImport.ID].PercentDone} %]");
                        }
                    }

                    this.logger.LogInformation("Done processing airports, performing bulk insert and update operations...");
                    await db.BulkInsertAsync(newAirports, token);
                    await db.BulkUpdateAsync(updatedAirports, token);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmapMSFS sqlite database airports table, last ident was {lastIdent}.");
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return Status[dataImport.ID].Elements["airport"].Processed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Import the approaches from an uploaded LittleNavmap database for MSFS.
        /// </summary>
        /// <remarks>
        /// sushi.at, 10/05/2021.
        /// </remarks>
        /// <param name="db">
        /// The OpenSky database context.
        /// </param>
        /// <param name="dataImport">
        /// The data import.
        /// </param>
        /// <param name="connection">
        /// The sqlite database connection.
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (approaches processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> ImportLittleNavmapApproachesMSFS(OpenSkyDbContext db, DataImport dataImport, SQLiteConnection connection, CancellationToken token)
        {
            var lastID = 0;

            db.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                var approachCommand = new SQLiteCommand(
                    "SELECT " +
                    "approach_id,airport_ident,runway_name,type,suffix " +
                    "FROM approach",
                    connection);

                var newApproaches = new List<Approach>();
                var updatedApproaches = new List<Approach>();
                var reader = await approachCommand.ExecuteReaderAsync(token);
                await using (reader)
                {
                    while (await reader.ReadAsync(token))
                    {
                        if (token.IsCancellationRequested)
                        {
                            this.logger.LogWarning($"Aborting data import of {dataImport.ID}, the server is shutting down...");
                            break;
                        }

                        var id = reader.GetInt32("approach_id");
                        lastID = id;
                        var existingApproachHash = await db.Approaches.Select(a => new { a.ID, a.HashCode }).SingleOrDefaultAsync(a => a.ID == id, token);
                        if (existingApproachHash == null)
                        {
                            // Make sure the airport exists
                            var airportICAO = reader.GetString("airport_ident");
                            if (!string.IsNullOrEmpty(airportICAO) && await db.Airports.SingleOrDefaultAsync(a => a.ICAO == airportICAO, token) != null)
                            {
                                Status[dataImport.ID].Elements["approach"].New++;
                                var newApproach = new Approach
                                {
                                    ID = id,
                                    AirportICAO = airportICAO,
                                    RunwayName = !await reader.IsDBNullAsync("runway_name", token) ? reader.GetString("runway_name") : null,
                                    Type = reader.GetString("type"),
                                    Suffix = !await reader.IsDBNullAsync("suffix", token) ? reader.GetString("suffix") : null,
                                    HashCode = reader.CalculateHashCode()
                                };
                                newApproaches.Add(newApproach);
                            }
                            else
                            {
                                Status[dataImport.ID].Elements["approach"].Skipped++;
                                this.logger.LogWarning($"Skipping approach with ID {id} due to missing airport ICAO {airportICAO}");
                            }
                        }
                        else
                        {
                            if (existingApproachHash.HashCode != reader.CalculateHashCode())
                            {
                                Status[dataImport.ID].Elements["approach"].Updated++;
                                var existingApproach = await db.Approaches.SingleAsync(a => a.ID == id, token);
                                existingApproach.RunwayName = !await reader.IsDBNullAsync("runway_name", token) ? reader.GetString("runway_name") : null;
                                existingApproach.Type = reader.GetString("type");
                                existingApproach.Suffix = !await reader.IsDBNullAsync("suffix", token) ? reader.GetString("suffix") : null;
                                existingApproach.HashCode = reader.CalculateHashCode();
                                updatedApproaches.Add(existingApproach);
                            }
                            else
                            {
                                Status[dataImport.ID].Elements["approach"].Skipped++;
                            }
                        }

                        Status[dataImport.ID].Processed++;
                        Status[dataImport.ID].Elements["approach"].Processed++;
                        if (Status[dataImport.ID].Processed % 1000 == 0)
                        {
                            this.logger.LogInformation($"Data import {dataImport.ID} has processed {Status[dataImport.ID].Processed} of {Status[dataImport.ID].Total} [{Status[dataImport.ID].PercentDone} %]");
                        }
                    }

                    this.logger.LogInformation("Done processing approaches, performing bulk insert and update operations...");
                    await db.BulkInsertAsync(newApproaches, token);
                    await db.BulkUpdateAsync(updatedApproaches, token);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmapMSFS sqlite database approaches table, last ID was {lastID}.");
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return Status[dataImport.ID].Elements["approach"].Processed;
        }

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
                    await this.ImportLittleNavmapMSFSPopulateImportStatus(connection, dataImportStatus, token);

                    // todo Since runways and approaches aren't part of the core game (only for displaying information to players),
                    // should these be "updated", especially since the integer IDs could change from one sqlite database export to another.
                    // So maybe just purge those tables, and display some notification to players that those details are currently being updated

                    totalRecordsProcessed += await this.ImportLittleNavmapAirportsMSFS(db, dataImport, connection, token);
                    totalRecordsProcessed += await this.ImportLittleNavmapRunwaysMSFS(db, dataImport, connection, token);
                    totalRecordsProcessed += await this.ImportLittleNavmapRunwayEndsMSFS(db, dataImport, connection, token);
                    totalRecordsProcessed += await this.ImportLittleNavmapApproachesMSFS(db, dataImport, connection, token);
                }
                finally
                {
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error processing LittleNavmap MSFS data import {dataImport.ID} for file {dataImport.ImportDataSource}");
                dataImport.LogText = $"ERROR processing: {ex.Message}";
            }
            finally
            {
                // Save the outcome of the data import to the database (we are not cancelling this if the server wants to shut down)
                dataImport.Finished = DateTime.UtcNow;
                dataImport.TotalRecordsProcessed = totalRecordsProcessed;
                dataImport.LogText = JsonSerializer.Serialize(Status[dataImport.ID]);
                await db.SaveDatabaseChangesAsync(this.logger, $"Error saving data import result to database for {dataImport.ID}");

                this.logger.LogInformation($"Finished processing LittleNavmap MSFS data import {dataImport.ID}, {totalRecordsProcessed} total records processed in {(DateTime.UtcNow - dataImport.Started).TotalMinutes:F1} minutes.");

                // Delete the temporary file
                if (File.Exists(dataImport.ImportDataSource))
                {
                    File.Delete(dataImport.ImportDataSource);
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Populates import status dictionary for LittleNavmap MSFS import job.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/05/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="connection">
        /// The sqlite database connection.
        /// </param>
        /// <param name="dataImportStatus">
        /// The data import status.
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task ImportLittleNavmapMSFSPopulateImportStatus(SQLiteConnection connection, DataImportStatus dataImportStatus, CancellationToken token)
        {
            // Airports
            var airportCountCommand = new SQLiteCommand("SELECT COUNT(ident) FROM airport", connection);
            if (await airportCountCommand.ExecuteScalarAsync(token) is not long airportCount)
            {
                this.logger.LogInformation("Uploaded sqlite database contains 0 airports or count failed, aborting...");
                throw new Exception("Uploaded sqlite database contains 0 airports or count failed");
            }

            this.logger.LogInformation($"Uploaded sqlite database contains {airportCount} airports");
            dataImportStatus.Elements.Add("airport", new DataImportElement { Total = (int)airportCount });
            dataImportStatus.Total += (int)airportCount;

            // Runways
            var runwayCountCommand = new SQLiteCommand("SELECT COUNT(runway_id) FROM runway", connection);
            if (await runwayCountCommand.ExecuteScalarAsync(token) is not long runwayCount)
            {
                this.logger.LogInformation("Uploaded sqlite database contains 0 runways or count failed, aborting...");
                throw new Exception("Uploaded sqlite database contains 0 runways or count failed");
            }

            this.logger.LogInformation($"Uploaded sqlite database contains {runwayCount} runways");
            dataImportStatus.Elements.Add("runway", new DataImportElement { Total = (int)runwayCount });
            dataImportStatus.Total += (int)runwayCount;

            // Runway ends
            var runwayEndCountCommand = new SQLiteCommand("SELECT COUNT(runway_end_id) FROM runway_end", connection);
            if (await runwayEndCountCommand.ExecuteScalarAsync(token) is not long runwayEndCount)
            {
                this.logger.LogInformation("Uploaded sqlite database contains 0 runway ends or count failed, aborting...");
                throw new Exception("Uploaded sqlite database contains 0 runway ends or count failed");
            }

            this.logger.LogInformation($"Uploaded sqlite database contains {runwayEndCount} runway ends");
            dataImportStatus.Elements.Add("runwayEnd", new DataImportElement { Total = (int)runwayEndCount });
            dataImportStatus.Total += (int)runwayEndCount;

            // Approaches
            var approachCountCommand = new SQLiteCommand("SELECT COUNT(approach_id) FROM approach", connection);
            if (await approachCountCommand.ExecuteScalarAsync(token) is not long approachCount)
            {
                this.logger.LogInformation("Uploaded sqlite database contains 0 approaches or count failed, aborting...");
                throw new Exception("Uploaded sqlite database contains 0 approaches or count failed");
            }

            this.logger.LogInformation($"Uploaded sqlite database contains {approachCount} approaches");
            dataImportStatus.Elements.Add("approach", new DataImportElement { Total = (int)approachCount });
            dataImportStatus.Total += (int)approachCount;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Import the runway ends from an uploaded LittleNavmap database for MSFS.
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
        /// <param name="connection">
        /// The sqlite database connection.
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (runway ends processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> ImportLittleNavmapRunwayEndsMSFS(OpenSkyDbContext db, DataImport dataImport, SQLiteConnection connection, CancellationToken token)
        {
            try
            {
                var primaryRunwayEndsCommand = new SQLiteCommand(
                    "SELECT " +
                    "runway_id,runway_end_id,name,offset_threshold,has_closed_markings,e.heading," +
                    "left_vasi_type,left_vasi_pitch,right_vasi_type,right_vasi_pitch,app_light_system_type," +
                    "e.lonx,e.laty " +
                    "FROM runway_end e " +
                    "JOIN runway r ON e.runway_end_id = r.primary_end_id " +
                    "WHERE end_type='P'",
                    connection);
                var primaryReader = await primaryRunwayEndsCommand.ExecuteReaderAsync(token);
                await using (primaryReader)
                {
                    await this.ImportLittleNavmapRunwayEndsMSFS(db, dataImport, token, primaryReader);
                }

                var secondaryRunwayEndsCommand = new SQLiteCommand(
                    "SELECT " +
                    "runway_id,runway_end_id,name,offset_threshold,has_closed_markings,e.heading," +
                    "left_vasi_type,left_vasi_pitch,right_vasi_type,right_vasi_pitch,app_light_system_type," +
                    "e.lonx,e.laty " +
                    "FROM runway_end e " +
                    "JOIN runway r ON e.runway_end_id = r.secondary_end_id " +
                    "WHERE end_type='S'",
                    connection);
                var secondaryReader = await secondaryRunwayEndsCommand.ExecuteReaderAsync(token);
                await using (secondaryReader)
                {
                    await this.ImportLittleNavmapRunwayEndsMSFS(db, dataImport, token, secondaryReader);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unhandled exception processing LittleNavmapMSFS sqlite database runway ends table.");
            }

            return Status[dataImport.ID].Elements["runwayEnd"].Processed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Import the runway ends from the specified data reader - source database has primary/secondary
        /// runway ends foreign key stored in reverse so this methods gets called twice.
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
        /// <param name="reader">
        /// The reader containing the records to process.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (runway ends processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task ImportLittleNavmapRunwayEndsMSFS(OpenSkyDbContext db, DataImport dataImport, CancellationToken token, DbDataReader reader)
        {
            var lastID = 0;

            db.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                var newRunwayEnds = new List<RunwayEnd>();
                var updatedRunwayEnds = new List<RunwayEnd>();
                while (await reader.ReadAsync(token))
                {
                    if (token.IsCancellationRequested)
                    {
                        this.logger.LogWarning($"Aborting data import of {dataImport.ID}, the server is shutting down...");
                        break;
                    }

                    var id = reader.GetInt32("runway_end_id");
                    lastID = id;
                    var existingRunwayEndHash = await db.RunwayEnds.Select(r => new { r.ID, r.HashCode }).SingleOrDefaultAsync(e => e.ID == id, token);
                    if (existingRunwayEndHash == null)
                    {
                        // Make sure the runway exists
                        var runwayID = reader.GetInt32("runway_id");
                        if (await db.Runways.SingleOrDefaultAsync(r => r.ID == runwayID, token) != null)
                        {
                            Status[dataImport.ID].Elements["runwayEnd"].New++;
                            var newRunwayEnd = new RunwayEnd
                            {
                                ID = id,
                                RunwayID = runwayID,
                                Name = reader.GetString("name"),
                                OffsetThreshold = reader.GetInt32("offset_threshold"),
                                HasClosedMarkings = reader.GetBoolean("has_closed_markings"),
                                Heading = reader.GetDouble("heading"),
                                LeftVasiType = !await reader.IsDBNullAsync("left_vasi_type", token) ? reader.GetString("left_vasi_type") : null,
                                LeftVasiPitch = !await reader.IsDBNullAsync("left_vasi_pitch", token) ? reader.GetDouble("left_vasi_pitch") : null,
                                RightVasiType = !await reader.IsDBNullAsync("right_vasi_type", token) ? reader.GetString("right_vasi_type") : null,
                                RightVasiPitch = !await reader.IsDBNullAsync("right_vasi_pitch", token) ? reader.GetDouble("right_vasi_pitch") : null,
                                ApproachLightSystem = !await reader.IsDBNullAsync("app_light_system_type", token) ? reader.GetString("app_light_system_type") : null,
                                Longitude = reader.GetDouble("lonx"),
                                Latitude = reader.GetDouble("laty"),
                                HashCode = reader.CalculateHashCode()
                            };
                            newRunwayEnds.Add(newRunwayEnd);
                        }
                        else
                        {
                            Status[dataImport.ID].Elements["runwayEnd"].Skipped++;
                            this.logger.LogWarning($"Skipping runway end with ID {id} due to missing runway with ID {runwayID}");
                        }
                    }
                    else
                    {
                        if (existingRunwayEndHash.HashCode != reader.CalculateHashCode())
                        {
                            Status[dataImport.ID].Elements["runwayEnd"].Updated++;
                            var existingRunwayEnd = await db.RunwayEnds.SingleAsync(r => r.ID == id, token);
                            existingRunwayEnd.Name = reader.GetString("name");
                            existingRunwayEnd.OffsetThreshold = reader.GetInt32("offset_threshold");
                            existingRunwayEnd.HasClosedMarkings = reader.GetBoolean("has_closed_markings");
                            existingRunwayEnd.Heading = reader.GetDouble("heading");
                            existingRunwayEnd.LeftVasiType = !await reader.IsDBNullAsync("left_vasi_type", token) ? reader.GetString("left_vasi_type") : null;
                            existingRunwayEnd.LeftVasiPitch = !await reader.IsDBNullAsync("left_vasi_pitch", token) ? reader.GetDouble("left_vasi_pitch") : null;
                            existingRunwayEnd.RightVasiType = !await reader.IsDBNullAsync("right_vasi_type", token) ? reader.GetString("right_vasi_type") : null;
                            existingRunwayEnd.RightVasiPitch = !await reader.IsDBNullAsync("right_vasi_pitch", token) ? reader.GetDouble("right_vasi_pitch") : null;
                            existingRunwayEnd.ApproachLightSystem = !await reader.IsDBNullAsync("app_light_system_type", token) ? reader.GetString("app_light_system_type") : null;
                            existingRunwayEnd.Longitude = reader.GetDouble("lonx");
                            existingRunwayEnd.Latitude = reader.GetDouble("laty");
                            existingRunwayEnd.HashCode = reader.CalculateHashCode();
                            updatedRunwayEnds.Add(existingRunwayEnd);
                        }
                        else
                        {
                            Status[dataImport.ID].Elements["runwayEnd"].Skipped++;
                        }
                    }

                    Status[dataImport.ID].Processed++;
                    Status[dataImport.ID].Elements["runwayEnd"].Processed++;
                    if (Status[dataImport.ID].Processed % 1000 == 0)
                    {
                        this.logger.LogInformation($"Data import {dataImport.ID} has processed {Status[dataImport.ID].Processed} of {Status[dataImport.ID].Total} [{Status[dataImport.ID].PercentDone} %]");
                    }
                }

                this.logger.LogInformation("Done processing runway ends, performing bulk insert and update operations...");
                await db.BulkInsertAsync(newRunwayEnds, token);
                await db.BulkUpdateAsync(updatedRunwayEnds, token);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmapMSFS sqlite database runway ends table, last ID was {lastID}.");
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Import the runways from an uploaded LittleNavmap database for MSFS.
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
        /// <param name="connection">
        /// The sqlite database connection.
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (runways processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> ImportLittleNavmapRunwaysMSFS(OpenSkyDbContext db, DataImport dataImport, SQLiteConnection connection, CancellationToken token)
        {
            var lastID = 0;

            db.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                var runwaysCommand = new SQLiteCommand(
                    "SELECT " +
                    "ident,runway_id,surface,length,width,r.altitude,edge_light,center_light " +
                    "FROM runway r " +
                    "JOIN airport a ON r.airport_id = a.airport_id",
                    connection);

                var newRunways = new List<Runway>();
                var updatedRunways = new List<Runway>();
                var reader = await runwaysCommand.ExecuteReaderAsync(token);
                await using (reader)
                {
                    while (await reader.ReadAsync(token))
                    {
                        if (token.IsCancellationRequested)
                        {
                            this.logger.LogWarning($"Aborting data import of {dataImport.ID}, the server is shutting down...");
                            break;
                        }

                        var id = reader.GetInt32("runway_id");
                        lastID = id;
                        var existingRunwayHash = await db.Runways.Select(r => new { r.ID, r.HashCode }).SingleOrDefaultAsync(r => r.ID == id, token);
                        if (existingRunwayHash == null)
                        {
                            // Make sure the airport exists
                            var airportICAO = reader.GetString("ident");
                            if (!string.IsNullOrEmpty(airportICAO) && await db.Airports.SingleOrDefaultAsync(a => a.ICAO == airportICAO, token) != null)
                            {
                                Status[dataImport.ID].Elements["runway"].New++;
                                var newRunway = new Runway
                                {
                                    ID = id,
                                    AirportICAO = airportICAO,
                                    Surface = reader.GetString("surface"),
                                    Length = reader.GetInt32("length"),
                                    Width = reader.GetInt32("width"),
                                    Altitude = reader.GetInt32("altitude"),
                                    EdgeLight = !await reader.IsDBNullAsync("edge_light", token) ? reader.GetString("edge_light") : null,
                                    CenterLight = !await reader.IsDBNullAsync("center_light", token) ? reader.GetString("center_light") : null,
                                    HashCode = reader.CalculateHashCode()
                                };
                                newRunways.Add(newRunway);
                            }
                            else
                            {
                                Status[dataImport.ID].Elements["runway"].Skipped++;
                                this.logger.LogWarning($"Skipping runway with ID {id} due to missing airport ICAO {airportICAO}");
                            }
                        }
                        else
                        {
                            if (existingRunwayHash.HashCode != reader.CalculateHashCode())
                            {
                                Status[dataImport.ID].Elements["runway"].Updated++;
                                var existingRunway = await db.Runways.SingleAsync(r => r.ID == id, token);
                                existingRunway.Surface = reader.GetString("surface");
                                existingRunway.Length = reader.GetInt32("length");
                                existingRunway.Width = reader.GetInt32("width");
                                existingRunway.Altitude = reader.GetInt32("altitude");
                                existingRunway.EdgeLight = !await reader.IsDBNullAsync("edge_light", token) ? reader.GetString("edge_light") : null;
                                existingRunway.CenterLight = !await reader.IsDBNullAsync("center_light", token) ? reader.GetString("center_light") : null;
                                existingRunway.HashCode = reader.CalculateHashCode();
                                updatedRunways.Add(existingRunway);
                            }
                            else
                            {
                                Status[dataImport.ID].Elements["runway"].Skipped++;
                            }
                        }

                        Status[dataImport.ID].Processed++;
                        Status[dataImport.ID].Elements["runway"].Processed++;
                        if (Status[dataImport.ID].Processed % 1000 == 0)
                        {
                            this.logger.LogInformation($"Data import {dataImport.ID} has processed {Status[dataImport.ID].Processed} of {Status[dataImport.ID].Total} [{Status[dataImport.ID].PercentDone} %]");
                        }
                    }

                    this.logger.LogInformation("Done processing runways, performing bulk insert and update operations...");
                    await db.BulkInsertAsync(newRunways, token);
                    await db.BulkUpdateAsync(updatedRunways, token);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmapMSFS sqlite database runways table, last ID was {lastID}.");
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return Status[dataImport.ID].Elements["runway"].Processed;
        }
    }
}