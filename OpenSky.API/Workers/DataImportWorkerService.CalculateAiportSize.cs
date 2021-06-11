namespace OpenSky.API.Workers
{
    using System;
    using System.Collections.Generic;
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
                    airport.Size = CalculateAirportSize(airport);
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
        /// <returns>
        /// The calculated airport size.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private static int CalculateAirportSize(Airport airport)
        {
            // First we have a look at the longest runway length, this forms our base value, but special rules can downgrade from this
            var size = airport.Runways.Where(r => !r.RunwayEnds.All(e => e.HasClosedMarkings)).Max(r => r.Length) switch
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

            // Size 5 airports must have a second 10000 feet runway
            if (size == 5 && airport.Runways.Count(r => !r.RunwayEnds.All(e => e.HasClosedMarkings) && r.Length >= 10000) < 2)
            {
                size = 4;
            }

            // Size 4 airports must have an instrument approach
            if (size == 4 && airport.Approaches.Count == 0)
            {
                size = 3;
            }

            // Size 4+5 airports need to have runway lights
            if (size >= 4 && !airport.Runways.Any(r => r.CenterLight != null || r.EdgeLight != null))
            {
                size = 3;
            }

            // Size 3+4+5 must have at least one hard runway
            if (size >= 3 && !airport.Runways.Any(r => r.Surface.ParseRunwaySurface().IsHardSurface()))
            {
                size = 2;
            }

            return size;
        }
    }


}
