// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationToken.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System.ComponentModel.DataAnnotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Application token model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 31/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ApplicationToken
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "Application name is required")]
        public string Name { get; set; }
    }
}