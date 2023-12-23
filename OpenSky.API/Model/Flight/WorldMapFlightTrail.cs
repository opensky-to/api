// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldMapFlightTrail.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Flight
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    using Microsoft.Extensions.Logging;

    using OpenSky.API.Controllers;
    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// World map flight trail model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 23/12/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class WorldMapFlightTrail
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldMapFlightTrail"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 23/12/2023.
        /// </remarks>
        /// <param name="flight">
        /// The flight.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public WorldMapFlightTrail(Flight flight, ILogger<FlightController> logger)
        {
            try
            {
                if (!string.IsNullOrEmpty(flight.AutoSaveLog))
                {
                    var sourceStream = new MemoryStream(Convert.FromBase64String(flight.AutoSaveLog));
                    var xmlStream = new MemoryStream();
                    using (var gzip = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        gzip.CopyTo(xmlStream);
                    }

                    xmlStream.Seek(0, SeekOrigin.Begin);
                    var xmlText = Encoding.UTF8.GetString(xmlStream.ToArray());

                    var flightLogXml = new FlightLogXML.FlightLog();
                    flightLogXml.RestoreFlightLog(XElement.Parse(xmlText));

                    this.PositionReports = flightLogXml.PositionReports.Select(pr => new WorldMapFlightPositionReport
                    {
                        Latitude = pr.Latitude,
                        Longitude = pr.Longitude,
                        Altitude = pr.Altitude
                    }).ToList();
                }

                if (!string.IsNullOrEmpty(flight.FlightLog))
                {
                    var sourceStream = new MemoryStream(Convert.FromBase64String(flight.FlightLog));
                    var xmlStream = new MemoryStream();
                    using (var gzip = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        gzip.CopyTo(xmlStream);
                    }

                    xmlStream.Seek(0, SeekOrigin.Begin);
                    var xmlText = Encoding.UTF8.GetString(xmlStream.ToArray());

                    var flightLogXml = new FlightLogXML.FlightLog();
                    flightLogXml.RestoreFlightLog(XElement.Parse(xmlText));

                    this.PositionReports = flightLogXml.PositionReports.Select(pr => new WorldMapFlightPositionReport
                    {
                        Latitude = pr.Latitude,
                        Longitude = pr.Longitude,
                        Altitude = pr.Altitude
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error parsing flight XML position reports.");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the position reports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<WorldMapFlightPositionReport> PositionReports { get; set; } = new();
    }
}