// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegisterUser.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System.ComponentModel.DataAnnotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Register user model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 05/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class RegisterUser
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
        /// Gets or sets the password.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the recaptcha token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string RecaptchaToken { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "Username is required")]
        [StringLength(15, MinimumLength = 3)]
        public string Username { get; set; }
    }
}