// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinancialRecord.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Helpers;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Financial record model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class FinancialRecord
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airline airline;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The child records.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<FinancialRecord> childRecords;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The parent financial record.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FinancialRecord parentRecord;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyUser user;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialRecord"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/01/2022.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public FinancialRecord()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialRecord"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/01/2022.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public FinancialRecord(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft registry (optional, if record relates to an aircraft).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(12, MinimumLength = 6)]
        public string AircraftRegistry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("AirlineID")]
        [JsonIgnore]
        public Airline Airline
        {
            get => this.LazyLoader.Load(this, ref this.airline);
            set => this.airline = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the airline.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Airline")]
        [StringLength(3)]
        [JsonIgnore]
        public string AirlineID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the financial category.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public FinancialCategory Category { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the child records.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [InverseProperty("ParentRecord")]
        public ICollection<FinancialRecord> ChildRecords
        {
            get => this.LazyLoader.Load(this, ref this.childRecords);
            set => this.childRecords = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the description of the record.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(250)]
        public string Description { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the expense amount in SkyBucks.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public long Expense { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier for the financial record.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [Required]
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the income amount in SkyBucks.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public long Income { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the parent financial record.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("ParentRecordID")]
        [JsonIgnore]
        public FinancialRecord ParentRecord
        {
            get => this.LazyLoader.Load(this, ref this.parentRecord);
            set => this.parentRecord = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the parent financial record.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("ParentRecord")]
        [JsonIgnore]
        public Guid? ParentRecordID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the timestamp of the record.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Timestamp { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("UserID")]
        [JsonIgnore]
        public OpenSkyUser User
        {
            get => this.LazyLoader.Load(this, ref this.user);
            set => this.user = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("User")]
        [StringLength(255)]
        [JsonIgnore]
        public string UserID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}