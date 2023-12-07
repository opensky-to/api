// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataImport.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OpenSky.API.Model;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Data import model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class DataImport
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when the import was finished.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? Finished { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the import.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets information describing the import data source (filename, url, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(50)]
        public string ImportDataSource { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the import status (only when returned in API call).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public DataImportStatus ImportStatus { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the optional import status JSON text.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
        public string ImportStatusJson { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when the import was started.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Started { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of records processed.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalRecordsProcessed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the import (ex. LittleNavmapMSFS).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(50)]
        [Required]
        public string Type { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the user that initiated the import.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(256)]
        [Required]
        public string UserName { get; set; }
    }
}