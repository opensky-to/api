// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyDbContext.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    using OpenSky.API.DbModel;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// OpenSky database context.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/05/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.EntityFrameworkCore.DbContext"/>
    /// -------------------------------------------------------------------------------------------------
    public partial class OpenSkyDbContext : IdentityDbContext<OpenSkyUser>
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

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Configures the schema needed for the identity framework.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="builder">
        /// The builder being used to construct the model for this context.
        /// </param>
        /// <seealso cref="M:Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{OpenSky.API.DbModel.OpenSkyUser, Microsoft.AspNetCore.Identity.IdentityRole, System.String, Microsoft.AspNetCore.Identity.IdentityUserClaim{System.String}, Microsoft.AspNetCore.Identity.IdentityUserRole{System.String}, Microsoft.AspNetCore.Identity.IdentityUserLogin{System.String}, Microsoft.AspNetCore.Identity.IdentityRoleClaim{System.String}, Microsoft.AspNetCore.Identity.IdentityUserToken{System.String}}.OnModelCreating(ModelBuilder)"/>
        /// -------------------------------------------------------------------------------------------------
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<OpenSkyUser>().ToTable("Users");
        }
    }
}