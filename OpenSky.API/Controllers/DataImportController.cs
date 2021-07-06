// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportController.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.Model;
    using OpenSky.API.Model.Authentication;
    using OpenSky.API.Workers;

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
    public partial class DataImportController : ControllerBase
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
        /// Get data import status.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/05/2021.
        /// </remarks>
        /// <param name="importID">
        /// Identifier for the import.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields the import status model.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("status/{importID:guid}", Name = "GetImportStatus")]
        public async Task<ActionResult<ApiResponse<DataImportStatus>>> GetImportStatus(Guid importID)
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | GET DataImport/status/{importID}");
            if (DataImportWorkerService.Status.ContainsKey(importID))
            {
                return new ApiResponse<DataImportStatus> { Data = DataImportWorkerService.Status[importID] };
            }

            var dataImport = await this.db.DataImports.SingleOrDefaultAsync(i => i.ID == importID);
            if (dataImport != null)
            {
                if (dataImport.Finished.HasValue)
                {
                    return new ApiResponse<DataImportStatus>($"Data import completed in {(dataImport.Finished - dataImport.Started).Value.TotalMinutes:F1} minutes, total number of records processed is {dataImport.TotalRecordsProcessed}.") { IsError = false, Status = "COMPLETE", Data = new DataImportStatus() };
                }

                if (!string.IsNullOrEmpty(dataImport.ImportStatusJson))
                {
                    var dataImportStatus = JsonSerializer.Deserialize<DataImportStatus>(dataImport.ImportStatusJson);
                    return new ApiResponse<DataImportStatus> { Data = dataImportStatus, Status = "PROCESSING" };
                }

                return new ApiResponse<DataImportStatus>("Specified import has no status saved.") { IsError = true, Data = new DataImportStatus() };
            }

            return new ApiResponse<DataImportStatus>("No import with specified ID was found.") { IsError = true, Data = new DataImportStatus() };
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get data imports.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/07/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the data imports.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        [HttpGet("", Name = "GetDataImports")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DataImport>>>> GetDataImports()
        {
            this.logger.LogInformation($"{this.User.Identity?.Name} | GET DataImport");
            var dataImports = await this.db.DataImports.ToListAsync();
            foreach (var dataImport in dataImports)
            {
                if (!string.IsNullOrEmpty(dataImport.ImportStatusJson))
                {
                    try
                    {
                        dataImport.ImportStatus = JsonSerializer.Deserialize<DataImportStatus>(dataImport.ImportStatusJson);
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, $"Error deserializing JSON import status for data import {dataImport.ID}");
                        dataImport.ImportStatus = new DataImportStatus();
                    }
                }
                else
                {
                    dataImport.ImportStatus = new DataImportStatus();
                }
            }

            return new ApiResponse<IEnumerable<DataImport>>(dataImports);
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
        [HttpPost("littleNavmapMSFS")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<ApiResponse<Guid?>>> PostLittleNavmapMSFS(IFormFile fileUpload)
        {
            var filePath = Path.GetTempFileName();
            try
            {
                var username = this.User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return new ApiResponse<Guid?>("Unable to determine current user name, aborting.") { IsError = true };
                }

                this.logger.LogInformation($"{this.User.Identity?.Name} | PostLittleNavmapMSFS received file with length {fileUpload.Length} bytes, saving to temporary file {filePath}");
                await using (var stream = System.IO.File.Create(filePath))
                {
                    await fileUpload.CopyToAsync(stream);
                }

                // Create data import record
                var dataImport = new DataImport
                {
                    ID = Guid.NewGuid(),
                    Type = "LittleNavmapMSFS",
                    Started = DateTime.UtcNow,
                    UserName = username,
                    ImportDataSource = filePath
                };
                await this.db.DataImports.AddAsync(dataImport);
                var saveEx = await this.db.SaveDatabaseChangesAsync(this.logger, "Error adding data import record to database.");
                if (saveEx != null)
                {
                    return new ApiResponse<Guid?>("Error adding data import record to database.", saveEx);
                }

                return new ApiResponse<Guid?>("Successfully added data import to queue.") { Data = dataImport.ID };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unhandled exception processing LittleNavmapMSFS sqlite database.");
                return new ApiResponse<Guid?>(ex);
            }
        }
    }
}