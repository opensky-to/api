// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportWorkerService.LittleNavmapMSFS.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Workers
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.SQLite;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;

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
            var airportsProcessed = 0;
            var lastIdent = "???";
            var origLogText = dataImport.LogText;

            try
            {
                var airportCountCommand = new SQLiteCommand("SELECT COUNT(ident) FROM airport", connection);
                if (await airportCountCommand.ExecuteScalarAsync(token) is not long count)
                {
                    this.logger.LogInformation("Uploaded sqlite database contains 0 airports or count failed, aborting...");
                    return 0;
                }

                this.logger.LogInformation($"Uploaded sqlite database contains {count} airports, processing...");

                var airportCommand = new SQLiteCommand(
                    "SELECT " +
                    "ident,name,city,has_avgas,has_jetfuel,tower_frequency,atis_frequency,unicom_frequency,is_closed,is_military," +
                    "num_parking_gate,num_parking_ga_ramp,num_runways,longest_runway_length,longest_runway_surface,laty,lonx " +
                    "FROM airport",
                    connection);
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

                        airportsProcessed++;
                        if (airportsProcessed % 50 == 0)
                        {
                            dataImport.LogText = origLogText + $"|Processed {airportsProcessed}/{count} airports [{(double)airportsProcessed / count * 100.0:F0} %]";
                            await db.SaveDatabaseChangesAsync(this.logger, $"Error saving changes for last batch of airports, last ident was {lastIdent}");
                        }

                        var ident = reader.GetString("ident");
                        lastIdent = ident;
                        var existingAirport = await db.Airports.SingleOrDefaultAsync(a => a.ICAO.Equals(ident), token);
                        if (existingAirport == null)
                        {
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
                                Longitude = reader.GetDouble("lonx")
                            };
                            await db.Airports.AddAsync(newAirport, token);
                        }
                        else
                        {
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
                        }
                    }

                    await db.SaveDatabaseChangesAsync(this.logger, $"Error saving changes for final batch of airports, last ident was {lastIdent}");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmapMSFS sqlite database airports table, last ident was {lastIdent}.");
            }

            return airportsProcessed;
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
            var approachesProcessed = 0;
            var lastID = 0;
            var origLogText = dataImport.LogText;

            try
            {
                var approachCountCommand = new SQLiteCommand("SELECT COUNT(approach_id) FROM approach", connection);
                if (await approachCountCommand.ExecuteScalarAsync(token) is not long count)
                {
                    this.logger.LogInformation("Uploaded sqlite database contains 0 approaches or count failed, aborting...");
                    return 0;
                }

                this.logger.LogInformation($"Uploaded sqlite database contains {count} approaches, processing...");

                var approachCommand = new SQLiteCommand(
                    "SELECT " +
                    "approach_id,airport_ident,runway_name,type,suffix " +
                    "FROM approach",
                    connection);
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

                        approachesProcessed++;
                        if (approachesProcessed % 50 == 0)
                        {
                            dataImport.LogText = origLogText + $"|Processed {approachesProcessed}/{count} approaches [{(double)approachesProcessed / count * 100.0:F0} %]";
                            await db.SaveDatabaseChangesAsync(this.logger, $"Error saving changes for last batch of approaches, last ID was {lastID}");
                        }

                        var id = reader.GetInt32("approach_id");
                        lastID = id;
                        var existingApproach = await db.Approaches.SingleOrDefaultAsync(a => a.ID == id, token);
                        if (existingApproach == null)
                        {
                            // Make sure the airport exists
                            var airportICAO = reader.GetString("airport_ident");
                            if (!string.IsNullOrEmpty(airportICAO) && await db.Airports.SingleOrDefaultAsync(a => a.ICAO == airportICAO, token) != null)
                            {
                                var newApproach = new Approach
                                {
                                    ID = id,
                                    AirportICAO = airportICAO,
                                    RunwayName = !await reader.IsDBNullAsync("runway_name", token) ? reader.GetString("runway_name") : null,
                                    Type = reader.GetString("type"),
                                    Suffix = !await reader.IsDBNullAsync("suffix", token) ? reader.GetString("suffix") : null
                                };
                                await db.Approaches.AddAsync(newApproach, token);
                            }
                            else
                            {
                                this.logger.LogWarning($"Skipping approach with ID {id} due to missing airport ICAO {airportICAO}");
                            }
                        }
                        else
                        {
                            existingApproach.RunwayName = !await reader.IsDBNullAsync("runway_name", token) ? reader.GetString("runway_name") : null;
                            existingApproach.Type = reader.GetString("type");
                            existingApproach.Suffix = !await reader.IsDBNullAsync("suffix", token) ? reader.GetString("suffix") : null;
                        }
                    }

                    await db.SaveDatabaseChangesAsync(this.logger, $"Error saving changes for final batch of approaches, last ID was {lastID}");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmapMSFS sqlite database approaches table, last ID was {lastID}.");
            }

            return approachesProcessed;
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
                var connection = new SQLiteConnection($"URI=file:{dataImport.ImportDataSource}");
                connection.Open();
                try
                {
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
                dataImport.LogText += $"|ERROR processing: {ex}";
            }
            finally
            {
                // Save the outcome of the data import to the database (we are not cancelling this if the server wants to shut down)
                dataImport.Finished = DateTime.UtcNow;
                dataImport.TotalRecordsProcessed = totalRecordsProcessed;
                await db.SaveDatabaseChangesAsync(this.logger, $"Error saving data import result to database for {dataImport.ID}");

                this.logger.LogInformation($"Finished processing LittleNavmap MSFS data import {dataImport.ID}, {totalRecordsProcessed} total records processed in {(DateTime.UtcNow - dataImport.Started).TotalMinutes:F1} minutes.");

                // Delete the temporary file
                if (System.IO.File.Exists(dataImport.ImportDataSource))
                {
                    System.IO.File.Delete(dataImport.ImportDataSource);
                }
            }
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
            var runwayEndsProcessed = 0;
            var origLogText = dataImport.LogText;

            try
            {
                var runwayCountCommand = new SQLiteCommand("SELECT COUNT(runway_end_id) FROM runway_end", connection);
                if (await runwayCountCommand.ExecuteScalarAsync(token) is not long count)
                {
                    this.logger.LogInformation("Uploaded sqlite database contains 0 runway ends or count failed, aborting...");
                    return 0;
                }

                this.logger.LogInformation($"Uploaded sqlite database contains {count} runway ends, processing...");
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
                    runwayEndsProcessed = await this.ImportLittleNavmapRunwayEndsMSFS(db, dataImport, token, primaryReader, origLogText, count, runwayEndsProcessed);
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
                    runwayEndsProcessed = await this.ImportLittleNavmapRunwayEndsMSFS(db, dataImport, token, secondaryReader, origLogText, count, runwayEndsProcessed);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unhandled exception processing LittleNavmapMSFS sqlite database runway ends table.");
            }

            return runwayEndsProcessed;
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
        /// <param name="origLogText">
        /// The original log text.
        /// </param>
        /// <param name="count">
        /// The total number of runway ends.
        /// </param>
        /// <param name="runwayEndsProcessed">
        /// The number runway ends already processed.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (runway ends processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> ImportLittleNavmapRunwayEndsMSFS(OpenSkyDbContext db, DataImport dataImport, CancellationToken token, DbDataReader reader, string origLogText, long count, int runwayEndsProcessed)
        {
            var lastID = 0;

            try
            {
                while (await reader.ReadAsync(token))
                {
                    if (token.IsCancellationRequested)
                    {
                        this.logger.LogWarning($"Aborting data import of {dataImport.ID}, the server is shutting down...");
                        break;
                    }

                    runwayEndsProcessed++;
                    if (runwayEndsProcessed % 50 == 0)
                    {
                        dataImport.LogText = origLogText + $"|Processed {runwayEndsProcessed}/{count} runway ends [{(double)runwayEndsProcessed / count * 100.0:F0} %]";
                        await db.SaveDatabaseChangesAsync(this.logger, $"Error saving changes for last batch of runway ends, last ID was {lastID}");
                    }

                    var id = reader.GetInt32("runway_end_id");
                    lastID = id;
                    var existingRunwayEnd = await db.RunwayEnds.SingleOrDefaultAsync(e => e.ID == id, token);
                    if (existingRunwayEnd == null)
                    {
                        // Make sure the runway exists
                        var runwayID = reader.GetInt32("runway_id");
                        if (await db.Runways.SingleOrDefaultAsync(r => r.ID == runwayID, token) != null)
                        {
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
                                Latitude = reader.GetDouble("laty")
                            };
                            await db.RunwayEnds.AddAsync(newRunwayEnd, token);
                        }
                        else
                        {
                            this.logger.LogWarning($"Skipping runway end with ID {id} due to missing runway with ID {runwayID}");
                        }
                    }
                    else
                    {
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
                    }
                }

                await db.SaveDatabaseChangesAsync(this.logger, $"Error saving changes for last batch of runway ends, last ID was {lastID}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmapMSFS sqlite database runway ends table, last ID was {lastID}.");
            }

            return runwayEndsProcessed;
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
            var runwaysProcessed = 0;
            var lastID = 0;
            var origLogText = dataImport.LogText;

            try
            {
                var runwayCountCommand = new SQLiteCommand("SELECT COUNT(runway_id) FROM runway", connection);
                if (await runwayCountCommand.ExecuteScalarAsync(token) is not long count)
                {
                    this.logger.LogInformation("Uploaded sqlite database contains 0 runways or count failed, aborting...");
                    return 0;
                }

                this.logger.LogInformation($"Uploaded sqlite database contains {count} runways, processing...");

                var runwaysCommand = new SQLiteCommand(
                    "SELECT " +
                    "ident,runway_id,surface,length,width,r.altitude,edge_light,center_light " +
                    "FROM runway r " +
                    "JOIN airport a ON r.airport_id = a.airport_id",
                    connection);
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

                        runwaysProcessed++;
                        if (runwaysProcessed % 50 == 0)
                        {
                            dataImport.LogText = origLogText + $"|Processed {runwaysProcessed}/{count} runways [{(double)runwaysProcessed / count * 100.0:F0} %]";
                            await db.SaveDatabaseChangesAsync(this.logger, $"Error saving changes for last batch of runways, last ID was {lastID}");
                        }

                        var id = reader.GetInt32("runway_id");
                        lastID = id;
                        var existingRunway = await db.Runways.SingleOrDefaultAsync(r => r.ID == id, token);
                        if (existingRunway == null)
                        {
                            // Make sure the airport exists
                            var airportICAO = reader.GetString("ident");
                            if (!string.IsNullOrEmpty(airportICAO) && await db.Airports.SingleOrDefaultAsync(a => a.ICAO == airportICAO, token) != null)
                            {
                                var newRunway = new Runway
                                {
                                    ID = id,
                                    AirportICAO = airportICAO,
                                    Surface = reader.GetString("surface"),
                                    Length = reader.GetInt32("length"),
                                    Width = reader.GetInt32("width"),
                                    Altitude = reader.GetInt32("altitude"),
                                    EdgeLight = !await reader.IsDBNullAsync("edge_light", token) ? reader.GetString("edge_light") : null,
                                    CenterLight = !await reader.IsDBNullAsync("center_light", token) ? reader.GetString("center_light") : null
                                };
                                await db.Runways.AddAsync(newRunway, token);
                            }
                            else
                            {
                                this.logger.LogWarning($"Skipping runway with ID {id} due to missing airport ICAO {airportICAO}");
                            }
                        }
                        else
                        {
                            existingRunway.Surface = reader.GetString("surface");
                            existingRunway.Length = reader.GetInt32("length");
                            existingRunway.Width = reader.GetInt32("width");
                            existingRunway.Altitude = reader.GetInt32("altitude");
                            existingRunway.EdgeLight = !await reader.IsDBNullAsync("edge_light", token) ? reader.GetString("edge_light") : null;
                            existingRunway.CenterLight = !await reader.IsDBNullAsync("center_light", token) ? reader.GetString("center_light") : null;
                        }
                    }

                    await db.SaveDatabaseChangesAsync(this.logger, $"Error saving changes for final batch of runways, last ID was {lastID}");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmapMSFS sqlite database runways table, last ID was {lastID}.");
            }

            return runwaysProcessed;
        }
    }
}