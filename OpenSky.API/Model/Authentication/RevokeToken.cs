// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RevokeToken.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System.ComponentModel.DataAnnotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Revoke token model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class RevokeToken
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the OpenSky refresh token ID to invalidate.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "Token ID is required")]
        public string Token { get; set; }
    }
}