﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftType.cs" company="OpenSky">
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
        /// The delivery locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AircraftManufacturerDeliveryLocation> deliveryLocations;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The fuel weight per gallon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private double fuelWeightPerGallon;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last edited by user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private OpenSkyUser lastEditedBy;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftManufacturer manufacturer;

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
        /// The variants of this aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<AircraftType> variants;

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
        /// Gets the valid empty model (no data, but valid for JSON deserialization of "required" attributes).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static AircraftType ValidEmptyModel =>
            new()
            {
                AtcModel = "XXXX",
                AtcType = "XXXX",
                Name = "XXXX",
                UploaderID = "XXXX",
                ManufacturerID = "XXXX",
                Manufacturer = AircraftManufacturer.ValidEmptyModel
            };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft image.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public byte[] AircraftImage { get; set; }

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
        [StringLength(200)]
        public string Comments { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the custom agent module name, default is NULL for none.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(20)]
        public string CustomAgentModule { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the delivery locations.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ICollection<AircraftManufacturerDeliveryLocation> DeliveryLocations
        {
            get => this.LazyLoader.Load(this, ref this.deliveryLocations);
            set => this.deliveryLocations = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the detailed checks are TEMPORARILY disabled - only
        /// use this on patch days until a new version of the plane can be added.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool DetailedChecksDisabled { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the automatic registry setting in the simulator is disabled.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool DisableAutoRegistry { get; set; }

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
        /// Gets or sets the engine model.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(50)]
        public string EngineModel { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the type of the engine (engine type as reported in the sim).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public EngineType EngineType { get; set; }

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
        /// Gets the type of fuel used by the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public FuelType FuelType
        {
            get
            {
                if (this.OverrideFuelType != FuelType.NotUsed)
                {
                    return this.OverrideFuelType;
                }

                // todo Keep an eye on how FS2020 will implement electric aircraft (they are planning to release the VoloCity air taxi)
                switch (this.EngineType)
                {
                    case EngineType.Piston:
                        return FuelType.AvGas;
                    case EngineType.Turboprop:
                    case EngineType.Jet:
                    case EngineType.HeloBellTurbine:
                        return FuelType.JetFuel;
                    case EngineType.None:
                    case EngineType.Unsupported:
                        return FuelType.None;
                    default:
                        return FuelType.None;
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the fuel weight per gallon (default values are 6 lbs/gallon avgas and 6.66 lbs/gallon jetfuel).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double FuelWeightPerGallon
        {
            get
            {
                if (this.fuelWeightPerGallon < 0)
                {
                    switch (this.FuelType)
                    {
                        case FuelType.AvGas:
                            return 6;
                        case FuelType.JetFuel:
                            return 6.7;
                        case FuelType.None:
                            return 0;
                        default:
                            return 0;
                    }
                }

                return this.fuelWeightPerGallon;
            }

            set => this.fuelWeightPerGallon = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this aircraft type has an image uploaded.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public bool HasAircraftImage => this.AircraftImage is { Length: > 0 };

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether this aircraft type has variants.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool HasVariants
        {
            get
            {
                if (this.IsVariantOf.HasValue)
                {
                    return true;
                }

                if (this.Variants?.Count > 0)
                {
                    return true;
                }

                return false;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ICAO type designator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(4)]
        public string IcaoTypeDesignator { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this aircraft type should be included in the world
        /// population, or only when a player buys one (use for popular mods only!).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IncludeInWorldPopulation { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this aircraft has retractable landing gear.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsGearRetractable { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this aircraft is historic (can't be purchased new).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsHistoric { get; set; }

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
        /// Gets or sets the last edited by user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("LastEditedByID")]
        [JsonIgnore]
        public OpenSkyUser LastEditedBy
        {
            get => this.LazyLoader.Load(this, ref this.lastEditedBy);
            set => this.lastEditedBy = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the last edited by user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(255)]
        [ForeignKey("LastEditedBy")]
        public string LastEditedByID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the last edited by user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public string LastEditedByName => this.LastEditedBy?.UserName;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("ManufacturerID")]
        public AircraftManufacturer Manufacturer
        {
            get => this.LazyLoader.Load(this, ref this.manufacturer);
            set => this.manufacturer = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the manufacturer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("Manufacturer")]
        [StringLength(5, MinimumLength = 3)]
        public string ManufacturerID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the maximum gross weight in pounds.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public double MaxGrossWeight { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the maximum payload delta allowed during a flight (to compensate for consumables
        /// like de-icing fluid).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int MaxPayloadDeltaAllowed { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the maximum selling price.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int MaxPrice { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Minimum runway length in feet.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int MinimumRunwayLength { get; set; }

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
        /// Gets or sets a value indicating whether the aircraft needs a flight engineer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool NeedsFlightEngineer { get; set; }

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
        /// Gets or sets the type of fuel the aircraft uses (not derived from engine type).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FuelType OverrideFuelType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the aircraft type requires manual fuelling.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool RequiresManualFuelling { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the aircraft type requires manual loading.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool RequiresManualLoading { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the simulator of the aircraft type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public Simulator Simulator { get; set; }

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
        /// Gets the name of the uploader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [NotMapped]
        public string UploaderName => this.Uploader?.UserName;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this aircraft uses the strobe light instead of the
        /// beacon - most likely because it doesn't have a beacon.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool UsesStrobeForBeacon { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the variants of this aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ICollection<AircraftType> Variants
        {
            get => this.LazyLoader.Load(this, ref this.variants);
            set => this.variants = value;
        }

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
        /// Gets or sets the version number of this type.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int VersionNumber { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}