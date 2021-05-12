// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImportController.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Controllers
{
    using System;
    using System.IO;
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
        public async Task<ActionResult<ApiResponse<Guid?>>> PostLittleNavmapMSFS(IFormFile fileUpload)
        {
            var filePath = Path.GetTempFileName();
            try
            {
                var username = this.User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return this.Ok(new ApiResponse<Guid?>("Unable to determine current user name, aborting.") { IsError = true });
                }

                this.logger.LogInformation($"PostLittleNavmapMSFS received file with length {fileUpload.Length} bytes, saving to temporary file {filePath}");
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
                await this.db.SaveDatabaseChangesAsync(this.logger, "Error adding data import record to database.");

                return this.Ok(new ApiResponse<Guid?>("Successfully added data import to queue.") { Data = dataImport.ID });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unhandled exception processing LittleNavmapMSFS sqlite database.");
                return this.Ok(new ApiResponse<Guid?>(ex));
            }
        }

        // todo add method that returns import status
    }
}