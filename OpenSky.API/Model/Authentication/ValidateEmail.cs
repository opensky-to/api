﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidateEmail.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System.ComponentModel.DataAnnotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Validate email model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ValidateEmail
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }
    }
}