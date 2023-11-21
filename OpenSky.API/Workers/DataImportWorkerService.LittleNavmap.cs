// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportWorkerService.LittleNavmap.cs" company="OpenSky">
// OpenSky project 2021-2023
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
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model;
    using OpenSky.S2Geometry.Extensions;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Data import worker service - LittleNavmap code.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class DataImportWorkerService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Populates import status dictionary for LittleNavmap import job.
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
        private async Task ImportLittleNavmapPopulateImportStatus(SQLiteConnection connection, DataImportStatus dataImportStatus, CancellationToken token)
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
            dataImportStatus.Elements.Add("airportSize", new DataImportElement { Total = (int)airportCount });
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
        /// Import the airports from an uploaded LittleNavmap database.
        /// </summary>
        /// <remarks>
        /// sushi.at and Flusinerd, 19/06/2021.
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
        /// <param name="simulator">
        /// The simulator.
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (airports processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> ImportLittleNavmapAirports(OpenSkyDbContext db, DataImport dataImport, SQLiteConnection connection, Simulator simulator, CancellationToken token)
        {
            var lastIdent = "???";

            db.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                var airportCommand = new SQLiteCommand(
                    "SELECT " +
                    "ident,name,city,has_avgas,has_jetfuel,tower_frequency,atis_frequency,unicom_frequency,is_closed,is_military," +
                    "num_parking_gate,num_parking_ga_ramp,num_runways,longest_runway_length,longest_runway_surface,laty,lonx,altitude " +
                    "FROM airport",
                    connection);

                // Load "static" dataset for A380 airports
                var a380Airports = await File.ReadAllLinesAsync("Datasets/a380airports.txt", token);

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
                        if (ident.Length > 5)
                        {
                            this.logger.LogWarning($"Skipping airport {ident}, ICAO code too long...");
                            Status[dataImport.ID].Elements["airport"].Skipped++;
                            Status[dataImport.ID].Processed++;
                            Status[dataImport.ID].Elements["airport"].Processed++;
                            continue;
                        }

                        var existingAirport = await db.Airports.SingleOrDefaultAsync(a => a.ICAO == ident, token);
                        if (existingAirport == null)
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
                                LongestRunwayLength = (int)reader.GetDouble("longest_runway_length"),
                                LongestRunwaySurface = reader.GetString("longest_runway_surface"),
                                Latitude = reader.GetDouble("laty"),
                                Longitude = reader.GetDouble("lonx"),
                                Altitude = reader.GetInt32("altitude"),
                                SupportsSuper = a380Airports.Contains(ident),
                                Size = null, // This will be calculated later as this depends on runways and approaches that aren't imported yet
                            };
                            if (simulator == Simulator.MSFS)
                            {
                                newAirport.MSFS = true;
                                newAirport.HasBeenPopulatedMSFS = ProcessingStatus.NeedsHandling;
                            }

                            if (simulator == Simulator.XPlane11)
                            {
                                newAirport.XP11 = true;
                                newAirport.HasBeenPopulatedXP11 = ProcessingStatus.NeedsHandling;
                            }

                            // Calculate S2 cell IDs
                            newAirport.S2Cell3 = OpenSkyS2.CellIDForCoordinates(newAirport.Latitude, newAirport.Longitude, 3).Id;
                            newAirport.S2Cell4 = OpenSkyS2.CellIDForCoordinates(newAirport.Latitude, newAirport.Longitude, 4).Id;
                            newAirport.S2Cell5 = OpenSkyS2.CellIDForCoordinates(newAirport.Latitude, newAirport.Longitude, 5).Id;
                            newAirport.S2Cell6 = OpenSkyS2.CellIDForCoordinates(newAirport.Latitude, newAirport.Longitude, 6).Id;
                            newAirport.S2Cell7 = OpenSkyS2.CellIDForCoordinates(newAirport.Latitude, newAirport.Longitude, 7).Id;
                            newAirport.S2Cell8 = OpenSkyS2.CellIDForCoordinates(newAirport.Latitude, newAirport.Longitude, 8).Id;
                            newAirport.S2Cell9 = OpenSkyS2.CellIDForCoordinates(newAirport.Latitude, newAirport.Longitude, 9).Id;

                            newAirports.Add(newAirport);
                        }
                        else
                        {
                            Status[dataImport.ID].Elements["airport"].Updated++;
                            var triggerAircraftPopulator = false;
                            if (simulator == Simulator.MSFS)
                            {
                                triggerAircraftPopulator = existingAirport.Gates != reader.GetInt32("num_parking_gate") || existingAirport.GaRamps != reader.GetInt32("num_parking_ga_ramp") || existingAirport.LongestRunwayLength != reader.GetInt32("longest_runway_length");

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
                                existingAirport.Altitude = reader.GetInt32("altitude");
                                existingAirport.SupportsSuper = a380Airports.Contains(ident);
                                existingAirport.PreviousSize = existingAirport.Size; // Save that away to detect size changes
                                existingAirport.Size = null; // Re-calculate the size
                                existingAirport.MSFS = true;
                            }

                            if (simulator == Simulator.XPlane11)
                            {
                                // Only update airport values if it only exists in XPlane11
                                if (!existingAirport.MSFS)
                                {
                                    triggerAircraftPopulator = existingAirport.Gates != reader.GetInt32("num_parking_gate") || existingAirport.GaRamps != reader.GetInt32("num_parking_ga_ramp") || existingAirport.LongestRunwayLength != (int)reader.GetDouble("longest_runway_length");

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
                                    existingAirport.LongestRunwayLength = (int)reader.GetDouble("longest_runway_length");
                                    existingAirport.LongestRunwaySurface = reader.GetString("longest_runway_surface");
                                    existingAirport.Latitude = reader.GetDouble("laty");
                                    existingAirport.Longitude = reader.GetDouble("lonx");
                                    existingAirport.Altitude = reader.GetInt32("altitude");
                                    existingAirport.SupportsSuper = a380Airports.Contains(ident);
                                    existingAirport.PreviousSize = existingAirport.Size; // Save that away to detect size changes
                                    existingAirport.Size = null; // Re-calculate the size
                                }
                                else
                                {
                                    triggerAircraftPopulator = true;
                                }

                                existingAirport.XP11 = true;
                            }

                            // Re-Calculate S2 cell IDs
                            existingAirport.S2Cell3 = OpenSkyS2.CellIDForCoordinates(existingAirport.Latitude, existingAirport.Longitude, 3).Id;
                            existingAirport.S2Cell4 = OpenSkyS2.CellIDForCoordinates(existingAirport.Latitude, existingAirport.Longitude, 4).Id;
                            existingAirport.S2Cell5 = OpenSkyS2.CellIDForCoordinates(existingAirport.Latitude, existingAirport.Longitude, 5).Id;
                            existingAirport.S2Cell6 = OpenSkyS2.CellIDForCoordinates(existingAirport.Latitude, existingAirport.Longitude, 6).Id;
                            existingAirport.S2Cell7 = OpenSkyS2.CellIDForCoordinates(existingAirport.Latitude, existingAirport.Longitude, 7).Id;
                            existingAirport.S2Cell8 = OpenSkyS2.CellIDForCoordinates(existingAirport.Latitude, existingAirport.Longitude, 8).Id;
                            existingAirport.S2Cell9 = OpenSkyS2.CellIDForCoordinates(existingAirport.Latitude, existingAirport.Longitude, 9).Id;

                            if (triggerAircraftPopulator)
                            {
                                if (simulator == Simulator.MSFS)
                                {
                                    existingAirport.HasBeenPopulatedMSFS = ProcessingStatus.NeedsHandling;
                                }

                                if (simulator == Simulator.XPlane11)
                                {
                                    existingAirport.HasBeenPopulatedXP11 = ProcessingStatus.NeedsHandling;
                                }
                            }

                            updatedAirports.Add(existingAirport);
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
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmap sqlite database airports table, last ident was {lastIdent}.");
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return Status[dataImport.ID].Elements["airport"].Processed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Import the runways from an uploaded LittleNavmap database.
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
        /// <param name="simulator">
        /// The simulator.
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (runways processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> ImportLittleNavmapRunways(OpenSkyDbContext db, DataImport dataImport, SQLiteConnection connection, Simulator simulator, CancellationToken token)
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
                        id = simulator switch
                        {
                            Simulator.MSFS => id,
                            Simulator.XPlane11 => id + 1000000,
                            _ => -1
                        };
                        lastID = id;

                        // Make sure the airport exists
                        var airportICAO = reader.GetString("ident");
                        if (!string.IsNullOrEmpty(airportICAO))
                        {
                            var airport = await db.Airports.SingleOrDefaultAsync(a => a.ICAO == airportICAO, token);
                            if (airport != null)
                            {
                                if (simulator == Simulator.MSFS || !airport.MSFS)
                                {
                                    Status[dataImport.ID].Elements["runway"].New++;
                                    var newRunway = new Runway
                                    {
                                        ID = id,
                                        AirportICAO = airportICAO,
                                        Surface = reader.GetString("surface"),
                                        Length = (int)reader.GetDouble("length"),
                                        Width = (int)reader.GetDouble("width"),
                                        Altitude = reader.GetInt32("altitude"),
                                        EdgeLight = !await reader.IsDBNullAsync("edge_light", token) ? reader.GetString("edge_light") : null,
                                        CenterLight = !await reader.IsDBNullAsync("center_light", token) ? reader.GetString("center_light") : null
                                    };
                                    newRunways.Add(newRunway);
                                }
                                else
                                {
                                    Status[dataImport.ID].Elements["runway"].Skipped++;
                                    this.logger.LogInformation($"Skipping runway with ID {id} due to already being available in MSFS");
                                }
                            }
                            else
                            {
                                Status[dataImport.ID].Elements["runway"].Skipped++;
                                this.logger.LogWarning($"Skipping runway with ID {id} due to missing airport ICAO {airportICAO}");
                            }
                        }
                        else
                        {
                            Status[dataImport.ID].Elements["runway"].Skipped++;
                            this.logger.LogWarning($"Skipping runway with ID {id} due to missing airport ICAO {airportICAO}");
                        }

                        Status[dataImport.ID].Processed++;
                        Status[dataImport.ID].Elements["runway"].Processed++;
                        if (Status[dataImport.ID].Processed % 1000 == 0)
                        {
                            this.logger.LogInformation($"Data import {dataImport.ID} has processed {Status[dataImport.ID].Processed} of {Status[dataImport.ID].Total} [{Status[dataImport.ID].PercentDone} %]");
                        }
                    }

                    this.logger.LogInformation("Done processing runways, performing bulk insert operation...");
                    await db.BulkInsertAsync(newRunways, token);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmap sqlite database runways table, last ID was {lastID}.");
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return Status[dataImport.ID].Elements["runway"].Processed;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Import the runway ends from an uploaded LittleNavmap database.
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
        /// <param name="simulator">
        /// The simulator.
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (runway ends processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> ImportLittleNavmapRunwayEnds(OpenSkyDbContext db, DataImport dataImport, SQLiteConnection connection, Simulator simulator, CancellationToken token)
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
                    await this.ImportLittleNavmapRunwayEnds(db, dataImport, token, primaryReader, simulator);
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
                    await this.ImportLittleNavmapRunwayEnds(db, dataImport, token, secondaryReader, simulator);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unhandled exception processing LittleNavmap sqlite database runway ends table.");
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
        /// <param name="simulator">
        /// The simulator.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (runway ends processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task ImportLittleNavmapRunwayEnds(OpenSkyDbContext db, DataImport dataImport, CancellationToken token, DbDataReader reader, Simulator simulator)
        {
            var lastID = 0;

            db.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                var newRunwayEnds = new List<RunwayEnd>();
                while (await reader.ReadAsync(token))
                {
                    if (token.IsCancellationRequested)
                    {
                        this.logger.LogWarning($"Aborting data import of {dataImport.ID}, the server is shutting down...");
                        break;
                    }

                    var id = reader.GetInt32("runway_end_id");
                    id = simulator switch
                    {
                        Simulator.MSFS => id,
                        Simulator.XPlane11 => id + 1000000,
                        _ => -1
                    };
                    lastID = id;

                    // Make sure the runway exists
                    var runwayID = reader.GetInt32("runway_id");
                    runwayID = simulator switch
                    {
                        Simulator.MSFS => runwayID,
                        Simulator.XPlane11 => runwayID + 1000000,
                        _ => -1
                    };
                    if (await db.Runways.SingleOrDefaultAsync(r => r.ID == runwayID, token) != null)
                    {
                        Status[dataImport.ID].Elements["runwayEnd"].New++;
                        var newRunwayEnd = new RunwayEnd
                        {
                            ID = id,
                            RunwayID = runwayID,
                            Name = reader.GetString("name"),
                            OffsetThreshold = (int)reader.GetDouble("offset_threshold"),
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
                        newRunwayEnds.Add(newRunwayEnd);
                    }
                    else
                    {
                        Status[dataImport.ID].Elements["runwayEnd"].Skipped++;
                        this.logger.LogWarning($"Skipping runway end with ID {id} due to missing runway with ID {runwayID}");
                    }

                    Status[dataImport.ID].Processed++;
                    Status[dataImport.ID].Elements["runwayEnd"].Processed++;
                    if (Status[dataImport.ID].Processed % 1000 == 0)
                    {
                        this.logger.LogInformation($"Data import {dataImport.ID} has processed {Status[dataImport.ID].Processed} of {Status[dataImport.ID].Total} [{Status[dataImport.ID].PercentDone} %]");
                    }
                }

                this.logger.LogInformation("Done processing runway ends, performing bulk insert operation...");
                await db.BulkInsertAsync(newRunwayEnds, token);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmap sqlite database runway ends table, last ID was {lastID}.");
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Import the approaches from an uploaded LittleNavmap database.
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
        /// <param name="simulator">
        /// The simulator.
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields an int (approaches processed count).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> ImportLittleNavmapApproaches(OpenSkyDbContext db, DataImport dataImport, SQLiteConnection connection, Simulator simulator, CancellationToken token)
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
                        id = simulator switch
                        {
                            Simulator.MSFS => id,
                            Simulator.XPlane11 => id + 1000000,
                            _ => -1
                        };
                        lastID = id;

                        // Make sure the airport exists
                        var airportICAO = reader.GetString("airport_ident");
                        if (!string.IsNullOrEmpty(airportICAO))
                        {
                            var airport = await db.Airports.SingleOrDefaultAsync(a => a.ICAO == airportICAO, token);
                            if (airport != null)
                            {
                                if (simulator == Simulator.MSFS || !airport.MSFS)
                                {
                                    Status[dataImport.ID].Elements["approach"].New++;
                                    var newApproach = new Approach
                                    {
                                        ID = id,
                                        AirportICAO = airportICAO,
                                        RunwayName = !await reader.IsDBNullAsync("runway_name", token) ? reader.GetString("runway_name") : null,
                                        Type = reader.GetString("type"),
                                        Suffix = !await reader.IsDBNullAsync("suffix", token) ? reader.GetString("suffix") : null
                                    };
                                    newApproaches.Add(newApproach);
                                }
                                else
                                {
                                    Status[dataImport.ID].Elements["approach"].Skipped++;
                                    this.logger.LogInformation($"Skipping approach with ID {id} due to already being available in MSFS");
                                }
                            }
                            else
                            {
                                Status[dataImport.ID].Elements["approach"].Skipped++;
                                this.logger.LogWarning($"Skipping approach with ID {id} due to missing airport ICAO {airportICAO}");
                            }
                        }
                        else
                        {
                            Status[dataImport.ID].Elements["approach"].Skipped++;
                            this.logger.LogWarning($"Skipping approach with ID {id} due to missing airport ICAO {airportICAO}");
                        }

                        Status[dataImport.ID].Processed++;
                        Status[dataImport.ID].Elements["approach"].Processed++;
                        if (Status[dataImport.ID].Processed % 1000 == 0)
                        {
                            this.logger.LogInformation($"Data import {dataImport.ID} has processed {Status[dataImport.ID].Processed} of {Status[dataImport.ID].Total} [{Status[dataImport.ID].PercentDone} %]");
                        }
                    }

                    this.logger.LogInformation("Done processing approaches, performing bulk insert operation...");
                    await db.BulkInsertAsync(newApproaches, token);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmap sqlite database approaches table, last ID was {lastID}.");
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return Status[dataImport.ID].Elements["approach"].Processed;
        }
    }
}