// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportWorkerService.CalculateAirportSize.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Workers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Data import worker service - Calculate airport size code.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class DataImportWorkerService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculates the airport size.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/06/2021.
        /// </remarks>
        /// <param name="airport">
        /// The airport to process.
        /// </param>
        /// <param name="top50Airports">
        /// The top 50 airports.
        /// </param>
        /// <returns>
        /// The calculated airport size.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private int CalculateAirportSize(Airport airport, string[] top50Airports)
        {
            var openRunways = airport.Runways.Where(r => !r.RunwayEnds.All(e => e.HasClosedMarkings)).ToList();
            if (openRunways.Count == 0)
            {
                // No open runway, size set to -1 .... airport closed
                return -1;
            }

            // First we have a look at the longest runway length, this forms our base value, but special rules can downgrade from this
            var size = openRunways.Max(r => r.Length) switch
            {
                >= 10000 => 5,
                >= 8000 => 4,
                >= 6000 => 3,
                >= 4500 => 2,
                >= 2300 => 1,
                _ => 0
            };

            // Size 5 airports must have an ILS approach
            if (size == 5 && airport.Approaches.All(a => a.Type != "ILS"))
            {
                size = 4;
            }

            // Size 5 airports must have a second at least 8000 feet hard surface runway
            if (size == 5 && openRunways.Count(r => r.Length >= 8000 && r.Surface.ParseRunwaySurface().IsHardSurface()) < 2)
            {
                size = 4;
            }

            // Size 5 airports need to have gates, unless it's a top 50 airport
            if (size == 5 && airport.Gates == 0 && !top50Airports.Contains(airport.ICAO))
            {
                size = 4;
            }

            // Size 4 airports must have an instrument approach
            if (size == 4 && airport.Approaches.Count == 0)
            {
                size = 3;
            }

            // Size 4+5 airports need to have runway lights
            if (size >= 4 && !openRunways.Any(r => r.CenterLight != null || r.EdgeLight != null))
            {
                size = 3;
            }

            // Size 3+4+5 must have at least one hard runway
            if (size >= 3 && !openRunways.Any(r => r.Surface.ParseRunwaySurface().IsHardSurface()))
            {
                size = 2;
            }

            // Is this a top 50 airport?
            if (top50Airports.Contains(airport.ICAO))
            {
                if (size == 5)
                {
                    size = 6;
                }
                else
                {
                    this.logger.LogWarning($"Top50 airport {airport.ICAO} was not classified size 5 so will not be upgraded to size 6, please double check classification code and source data.");
                }
            }

            return size;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Calculates the airport sizes for all airports with size NULL.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/06/2021.
        /// </remarks>
        /// <param name="db">
        /// The OpenSky database.
        /// </param>
        /// <param name="dataImport">
        /// The data import that triggered this (for reporting progress).
        /// </param>
        /// <param name="token">
        /// A token that allows processing to be cancelled.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the records processed count.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private async Task<int> CalculateAirportSizes(OpenSkyDbContext db, DataImport dataImport, CancellationToken token)
        {
            var lastIdent = "???";
            db.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                var airportsWithoutSize = db.Airports.Where(a => !a.Size.HasValue).Select(a => a.ICAO).ToList();
                this.logger.LogInformation($"OpenSky database now contains {airportsWithoutSize.Count} airports without size, processing...");

                // Check if our airportSize count estimate was off (or another import has left airports without size that we can process now)
                if (Status[dataImport.ID].Elements["airportSize"].Total != airportsWithoutSize.Count)
                {
                    var oldCount = Status[dataImport.ID].Elements["airportSize"].Total;
                    Status[dataImport.ID].Elements["airportSize"].Total = airportsWithoutSize.Count;
                    Status[dataImport.ID].Total -= oldCount;
                    Status[dataImport.ID].Total += airportsWithoutSize.Count;
                }

                // Load "static" dataset for top 50 airports ... the only size 6
                var top50Airports = await File.ReadAllLinesAsync("Datasets/top50airports.txt", token);

                var updatedAirports = new List<Airport>();
                foreach (var icao in airportsWithoutSize)
                {
                    if (token.IsCancellationRequested)
                    {
                        this.logger.LogWarning($"Aborting data import of {dataImport.ID}, the server is shutting down...");
                        break;
                    }

                    lastIdent = icao;
                    var airport = await db.Airports.SingleOrDefaultAsync(a => a.ICAO == icao, token);
                    airport.Size = this.CalculateAirportSize(airport, top50Airports);
                    updatedAirports.Add(airport);

                    Status[dataImport.ID].Processed++;
                    Status[dataImport.ID].Elements["airportSize"].Processed++;
                    if (Status[dataImport.ID].Processed % 1000 == 0)
                    {
                        this.logger.LogInformation($"Data import {dataImport.ID} has processed {Status[dataImport.ID].Processed} of {Status[dataImport.ID].Total} [{Status[dataImport.ID].PercentDone} %]");
                    }
                }

                this.logger.LogInformation("Done calculating airport sizes, performing bulk update operation...");
                await db.BulkUpdateAsync(updatedAirports, token);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception calculating airport sizes, last ident was {lastIdent}.");
            }
            finally
            {
                db.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return Status[dataImport.ID].Elements["airportSize"].Processed;
        }
    }
}