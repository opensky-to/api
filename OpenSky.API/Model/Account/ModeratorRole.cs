// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModeratorRole.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Account
{
    using System.ComponentModel.DataAnnotations;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Moderator role model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 21/11/2023.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ModeratorRole
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this user should be a moderator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsModerator { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
    }
}