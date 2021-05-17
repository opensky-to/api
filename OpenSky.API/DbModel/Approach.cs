// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Approach.cs" company="OpenSky">
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

    /*
     * APPROACH EXAMPLE RECORD FROM DB (LOWW - Vienna International)
     *
     * INSERT INTO `Approaches` (`ID`, `AirportICAO`, `HashCode`, `RunwayName`, `Suffix`, `Type`)
     *
     * VALUES ('12541', 'LOWW', 'bf2a2f210da42044aa54e78de48ba8d1a4d42651', '16', NULL, 'VORDME'),
     *        ('12542', 'LOWW', '346954e5df0cd362dadf6f80debc83c7553dbd11', '34', NULL, 'VORDME'),
     *        ('12543', 'LOWW', 'd56fbc74739cd64b95c176ad965cd3ad1804bc9e', '11', NULL, 'ILS'),
     *        ('12544', 'LOWW', 'daec8393a47898e4132a8d043dfe0b8bed0e85cb', '16', NULL, 'ILS'),
     *        ('12545', 'LOWW', 'a624126496a5a3535738f87f102236c0d7dd91a4', '29', NULL, 'ILS'),
     *        ('12546', 'LOWW', '83f613115a68ae7de51f98a864e7ba72c0e654e0', '34', NULL, 'ILS'),
     *        ('12547', 'LOWW', '2e9dc69cc07eda0f1dbb8716ad5ee8242c7fc45f', '11', NULL, 'LOC'),
     *        ('12548', 'LOWW', '81620cab61608f85513830b2436c3c2dab648c82', '16', NULL, 'LOC'),
     *        ('12549', 'LOWW', 'ad305259ceaa2d3287ca78e497f201566243bfd4', '29', NULL, 'LOC'),
     *        ('12550', 'LOWW', '75eb41ab3372686b862b266c2f8c13d357111a91', '34', NULL, 'LOC'),
     *        ('12551', 'LOWW', '3a6a49ce02fc6240e060df62fc088e1799dcb516', '11', NULL, 'NDBDME'),
     *        ('12552', 'LOWW', '83e66508f528d140e99ab958765aa6133b8669a2', '29', NULL, 'NDBDME'),
     *        ('12553', 'LOWW', 'e96a3f42f98a13e65225f2f73c23ce68929a525a', '11', 'Z', 'RNAV'),
     *        ('12554', 'LOWW', 'bef666f80445820532daddb0dbdeabdf38178cac', '16', 'E', 'RNAV'),
     *        ('12555', 'LOWW', '1f4661a1bc12d9bab1d37c6771d2e73119aa0065', '16', 'N', 'RNAV'),
     *        ('12556', 'LOWW', 'd9ff1b6129589351975b8f45963f08057663345d', '16', 'Z', 'RNAV'),
     *        ('12557', 'LOWW', '6f37bad5486899cbe3695ebedef93498e9a404ea', '29', 'X', 'RNAV'),
     *        ('12558', 'LOWW', '1d5eeeb3df754025b8cbe84ef1166b64a0ab60df', '29', 'Z', 'RNAV'),
     *        ('12559', 'LOWW', '1d182d1a8c2486a284b7045f5bae5949103c842e', '34', NULL, 'RNAV')
     */

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Approach model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Approach
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Airport airport;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Approach"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public Approach()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Approach"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Approach(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        [ForeignKey("AirportICAO")]
        public Airport Airport
        {
            get => this.LazyLoader.Load(this, ref this.airport);
            set => this.airport = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airport ICAO.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(5, MinimumLength = 3)]
        [Required]
        [ForeignKey("Airport")]
        public string AirportICAO { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the hash code (SHA1 over all data columns to detect if record needs updating).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        public string HashCode { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the approach ID.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the runway (can be NULL if approach doesn't specify a runway).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(6)]
        public string RunwayName { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the approach type suffix (Y, Z, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(1)]
        public string Suffix { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the approach type (ILS, RNAV, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(25)]
        public string Type { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}