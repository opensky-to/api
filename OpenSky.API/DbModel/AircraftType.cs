// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AircraftType.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;

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
        /// Gets or sets the ATCModel property in the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(100)]
        public string AtcModel { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ATCType property in the sim.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(100)]
        public string AtcType { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the category (sep, mep, set, met, jet, nbairliner, wbairliner, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(10)]
        public string Category { get; set; }

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
        public Guid? NextVersion { get; set; }
    }
}