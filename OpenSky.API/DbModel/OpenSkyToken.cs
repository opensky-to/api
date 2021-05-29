// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyToken.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// An OpenSky token (used to refresh JWT access tokens).
    /// </summary>
    /// <remarks>
    /// sushi.at, 29/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class OpenSkyToken
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of when the token was created.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Created { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of the expiry of the token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime Expiry { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the token (aka refresh token ID).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Key]
        public Guid ID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the token (ex. website, agent-msfs, etc.).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the user the token belongs to.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [ForeignKey("UserID")]
        public OpenSkyUser User { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the user the token belongs to.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required]
        [StringLength(255)]
        [ForeignKey("User")]
        public string UserID { get; set; }
    }
}