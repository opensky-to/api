// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangePassword.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System.ComponentModel.DataAnnotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Change password model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 08/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ChangePassword
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "New password is required")]
        public string NewPassword { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether to reset all OpenSky api tokens.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool ResetTokens { get; set; } = true;
    }
}