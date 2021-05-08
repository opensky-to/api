// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangePassword.cs" company="OpenSky">
// sushi.at for OpenSky 2021
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
    }
}