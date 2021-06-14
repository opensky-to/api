// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResendValidationEmail.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System.ComponentModel.DataAnnotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Resend validation email model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 08/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ResendValidationEmail
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
        /// Gets or sets the recaptcha token.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string RecaptchaToken { get; set; }
    }
}