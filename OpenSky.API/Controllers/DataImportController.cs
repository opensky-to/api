// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportController.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System;
    using System.Data;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Data import controller.
    /// </summary>
    /// <remarks>
    /// sushi.at, 04/05/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Mvc.ControllerBase"/>
    /// -------------------------------------------------------------------------------------------------
    [Authorize(Roles = UserRoles.Admin)]
    [ApiController]
    [Route("[controller]")]
    public class DataImportController : ControllerBase
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
        private readonly ILogger<DataImportController> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="DataImportController"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/05/2021.
        /// </remarks>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="db">
        /// The OpenSky database context.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public DataImportController(ILogger<DataImportController> logger, OpenSkyDbContext db)
        {
            this.logger = logger;
            this.db = db;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Post LittleNavmap MSFS sqlite database for import.
        /// </summary>
        /// <remarks>
        /// sushi.at, 04/05/2021.
        /// </remarks>
        /// <param name="fileUpload">
        /// The file upload containing the sqlite database.
        /// </param>
        /// <returns>
        /// A basic IActionResult containing status or error messages.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("littleNavmapMSFS")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<ApiResponse<string>>> PostLittleNavmapMSFS(IFormFile fileUpload)
        {
            var filePath = Path.GetTempFileName();
            var airportsProcessed = 0;
            var lastIdent = "???";
            try
            {
                this.logger.LogInformation($"PostLittleNavmapMSFS received file with length {fileUpload.Length} bytes, saving to temporary file {filePath}");
                await using (var stream = System.IO.File.Create(filePath))
                {
                    await fileUpload.CopyToAsync(stream);
                }

                var connection = new SQLiteConnection($"URI=file:{filePath}");
                connection.Open();
                try
                {
                    var airportCountCommand = new SQLiteCommand("SELECT COUNT(ident) FROM airport", connection);
                    var count = airportCountCommand.ExecuteScalar();
                    this.logger.LogInformation($"Uploaded sqlite database contains {count ?? 0} airports, processing...");

                    var airportCommand = new SQLiteCommand("SELECT " +
                                                           "ident,name,city,has_avgas,has_jetfuel,tower_frequency,atis_frequency,unicom_frequency,is_closed,is_military," +
                                                           "num_parking_gate,num_parking_ga_ramp,num_runways,longest_runway_length,longest_runway_surface,laty,lonx " +
                                                           "FROM airport", connection);
                    await using var reader = airportCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        airportsProcessed++;
                        if (airportsProcessed % 50 == 0)
                        {
                            try
                            {
                                await this.db.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                this.logger.LogError(ex, $"Error saving changes for last batch of airports, last ident was {lastIdent}");
                            }
                            
                            this.logger.LogInformation($"Processed {airportsProcessed} airports...");
                        }

                        var ident = reader.GetString("ident");
                        lastIdent = ident;
                        var existingAirport = this.db.Airports.SingleOrDefault(a => a.ICAO.Equals(ident));
                        if (existingAirport == null)
                        {
                            var newAirport = new Airport
                            {
                                ICAO = ident,
                                Name = !await reader.IsDBNullAsync("name") ? new string(reader.GetString("name").Take(50).ToArray()) : "???",
                                City = !await reader.IsDBNullAsync("city") ? new string(reader.GetString("city").Take(50).ToArray()) : null,
                                HasAvGas = reader.GetBoolean("has_avgas"),
                                HasJetFuel = reader.GetBoolean("has_jetfuel"),
                                TowerFrequency = !await reader.IsDBNullAsync("tower_frequency") ? (reader.GetInt32("tower_frequency") != 0 ? reader.GetInt32("tower_frequency") : null) : null,
                                AtisFrequency = !await reader.IsDBNullAsync("atis_frequency") ? (reader.GetInt32("atis_frequency") != 0 ? reader.GetInt32("atis_frequency") : null) : null,
                                UnicomFrequency = !await reader.IsDBNullAsync("unicom_frequency") ? (reader.GetInt32("unicom_frequency") != 0 ? reader.GetInt32("unicom_frequency") : null) : null,
                                IsClosed = reader.GetBoolean("is_closed"),
                                IsMilitary = reader.GetBoolean("is_military"),
                                Gates = reader.GetInt32("num_parking_gate"),
                                GaRamps = reader.GetInt32("num_parking_ga_ramp"),
                                Runways = reader.GetInt32("num_runways"),
                                LongestRunwayLength = reader.GetInt32("longest_runway_length"),
                                LongestRunwaySurface = reader.GetString("longest_runway_surface"),
                                Latitude = reader.GetDouble("laty"),
                                Longitude = reader.GetDouble("lonx")
                            };
                            this.db.Airports.Add(newAirport);
                        }
                        else
                        {
                            existingAirport.Name = !await reader.IsDBNullAsync("name") ? new string(reader.GetString("name").Take(50).ToArray()) : "???";
                            existingAirport.City = !await reader.IsDBNullAsync("city") ? new string(reader.GetString("city").Take(50).ToArray()) : null;
                            existingAirport.HasAvGas = reader.GetBoolean("has_avgas");
                            existingAirport.HasJetFuel = reader.GetBoolean("has_jetfuel");
                            existingAirport.TowerFrequency = !await reader.IsDBNullAsync("tower_frequency") ? (reader.GetInt32("tower_frequency") != 0 ? reader.GetInt32("tower_frequency") : null) : null;
                            existingAirport.AtisFrequency = !await reader.IsDBNullAsync("atis_frequency") ? (reader.GetInt32("atis_frequency") != 0 ? reader.GetInt32("atis_frequency") : null) : null;
                            existingAirport.UnicomFrequency = !await reader.IsDBNullAsync("unicom_frequency") ? (reader.GetInt32("unicom_frequency") != 0 ? reader.GetInt32("unicom_frequency") : null) : null;
                            existingAirport.IsClosed = reader.GetBoolean("is_closed");
                            existingAirport.IsMilitary = reader.GetBoolean("is_military");
                            existingAirport.Gates = reader.GetInt32("num_parking_gate");
                            existingAirport.GaRamps = reader.GetInt32("num_parking_ga_ramp");
                            existingAirport.Runways = reader.GetInt32("num_runways");
                            existingAirport.LongestRunwayLength = reader.GetInt32("longest_runway_length");
                            existingAirport.LongestRunwaySurface = reader.GetString("longest_runway_surface");
                            existingAirport.Latitude = reader.GetDouble("laty");
                            existingAirport.Longitude = reader.GetDouble("lonx");
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }

                return this.Ok(new ApiResponse<string>($"Successfully processed {airportsProcessed} airports."));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Unhandled exception processing LittleNavmapMSFS sqlite database. Last ident was {lastIdent}");
                return this.StatusCode(StatusCodes.Status500InternalServerError,new ApiResponse<string>(ex));
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}