// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyDbContext.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API
{
    using Microsoft.EntityFrameworkCore;

    using OpenSky.API.Model;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky database context.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/05/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.EntityFrameworkCore.DbContext"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class OpenSkyDbContext : DbContext
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSkyDbContext"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/05/2021.
        /// </remarks>
        /// <param name="options">
        /// Database context options.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public OpenSkyDbContext(DbContextOptions<OpenSkyDbContext> options) : base(options)
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<Airport> Airports { get; set; }
    }
}