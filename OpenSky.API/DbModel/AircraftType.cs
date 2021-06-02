// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftType.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text.Json.Serialization;

    using OpenSky.API.Helpers;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Aircraft type model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 12/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AircraftType
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Type of the next version.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType nextVersionType;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The uploader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyUser uploader;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The type this aircraft is a variant of.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftType variantType;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftType"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AircraftType"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 01/06/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AircraftType(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ATCModel property in the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(100)]
        [Required]
        public string AtcModel { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ATCType property in the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(100)]
        [Required]
        public string AtcType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the category (sep, mep, set, met, jet, nbairliner, wbairliner, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public AircraftTypeCategory Category { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the comments (moderation status, retired, needs fixing, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Comments { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the detailed checks are TEMPORARILY disabled - only
        /// use this on patch days until a new version of the plane can be added.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool DetailedChecksDisabled { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the empty weight in pounds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double EmptyWeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this aircraft type is enabled.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool Enabled { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the number of engines.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int EngineCount { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the engine (engine type as reported in the sim).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public string EngineType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this aircraft has flaps.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool FlapsAvailable { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel total capacity in gallons.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelTotalCapacity { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this aircraft has retractable landing gear.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsGearRetractable { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this aircraft is available in the vanilla sim or is
        /// coming from a mod.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsVanilla { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ID of the aircraft that this one is a variant of.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("VariantType")]
        public Guid? IsVariantOf { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the maximum gross weight in pounds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxGrossWeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the maximum selling price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int MaxPrice { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the minimum selling price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int MinPrice { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the aircraft needs a co-pilot.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool NeedsCoPilot { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the next version of this aircraft type - to migrate existing aircraft to this
        /// new type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("NextVersionType")]
        public Guid? NextVersion { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the next version - to migrate existing aircraft to this new type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("NextVersion")]
        [JsonIgnore]
        public AircraftType NextVersionType
        {
            get => this.LazyLoader.Load(this, ref this.nextVersionType);
            set => this.nextVersionType = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the uploader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("UploaderID")]
        [JsonIgnore]
        public OpenSkyUser Uploader
        {
            get => this.LazyLoader.Load(this, ref this.uploader);
            set => this.uploader = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the uploader user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(255)]
        [Required]
        [ForeignKey("Uploader")]
        public string UploaderID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type this one is a variant of.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("IsVariantOf")]
        [JsonIgnore]
        public AircraftType VariantType
        {
            get => this.LazyLoader.Load(this, ref this.variantType);
            set => this.variantType = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Values that represent aircraft type categories.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public enum AircraftTypeCategory
        {
            /// <summary>
            /// Single Engine Piston
            /// </summary>
            SEP,

            /// <summary>
            /// Multi Engine Piston
            /// </summary>
            MEP, 

            /// <summary>
            /// Single Engine Turboprop
            /// </summary>
            SET, 

            /// <summary>
            /// Multi Engine Turboprop
            /// </summary>
            MET, 

            /// <summary>
            /// Jet (small private and business jets)
            /// </summary>
            Jet, 

            /// <summary>
            /// Narrow-Body Airliner
            /// </summary>
            NBAirliner, 

            /// <summary>
            /// Wide-Body Airliner
            /// </summary>
            WBAirliner
        }
    }
}