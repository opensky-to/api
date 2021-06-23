// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyUser.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    using Microsoft.AspNetCore.Identity;

    using OpenSky.API.Helpers;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky user model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 05/05/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Identity.IdentityUser"/>
    /// -------------------------------------------------------------------------------------------------
    public class OpenSkyUser : IdentityUser
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The access tokens of the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private ICollection<OpenSkyToken> tokens;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyUser"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyUser()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyUser"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/05/2021.
        /// </remarks>
        /// <param name="lazyLoader">
        /// The lazy loader.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyUser(Action<object, string> lazyLoader)
        {
            this.LazyLoader = lazyLoader;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time of the last login.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime? LastLogin { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the last login geo location (country).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LastLoginGeo { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the last login IP.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string LastLoginIP { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Date/Time when the user registered.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public DateTime RegisteredOn { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the access tokens of the user.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [JsonIgnore]
        public ICollection<OpenSkyToken> Tokens
        {
            get => this.LazyLoader.Load(this, ref this.tokens);
            set => this.tokens = value;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the lazy loader.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private Action<object, string> LazyLoader { get; }
    }
}