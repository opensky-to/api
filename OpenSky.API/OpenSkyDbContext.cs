// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenSkyDbContext.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Microsoft.Extensions.Logging;

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
        /// Gets or sets the aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<Aircraft> Aircraft { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the aircraft types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<AircraftType> AircraftTypes { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airlines.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<Airline> Airlines { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline share holders.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<AirlineShareHolder> AirlineShareHolders { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline user permissions.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<AirlineUserPermission> AirlineUserPermissions { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the client airport packages.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<AirportClientPackage> AirportClientPackages { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<Airport> Airports { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the approaches.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<Approach> Approaches { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the data imports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<DataImport> DataImports { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight navlog fixes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<FlightNavlogFix> FlightNavlogFixes { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flights.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<Flight> Flights { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the OpenSky tokens.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<OpenSkyToken> OpenSkyTokens { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the runway ends.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<RunwayEnd> RunwayEnds { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the runways.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public virtual DbSet<Runway> Runways { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Resets the MSFS flag for all airports.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/06/2021.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields an int of rows affected.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<int> ResetAirportsMSFS()
        {
            return await this.Database.ExecuteSqlRawAsync("UPDATE Airports SET MSFS=0");
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Save database changes using a transaction.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// <param name="logger">
        /// The logger to use in case of an error.
        /// </param>
        /// <param name="errorMessage">
        /// (Optional) Message describing the error.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<Exception> SaveDatabaseChangesAsync(ILogger logger, string errorMessage = null)
        {
            await using var transaction = await this.Database.BeginTransactionAsync();
            try
            {
                await this.SaveChangesAsync();
                await transaction.CommitAsync();
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Error saving database changes.");
                await transaction.RollbackAsync();
                this.ChangeTracker.Clear();
                return ex;
            }
        }

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

            // Rename tables on the default identity entities
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<OpenSkyUser>().ToTable("Users");

            // Composite primary keys
            builder.Entity<AirlineShareHolder>().HasKey(sh => new { sh.AirlineICAO, sh.UserID });
            builder.Entity<AirlineUserPermission>().HasKey(p => new { p.AirlineICAO, p.UserID });
            builder.Entity<FlightNavlogFix>().HasKey(nf => new { nf.FlightID, nf.FixNumber });

            // Custom relationships
            builder.Entity<AircraftType>().HasMany(t => t.Variants).WithOne(t => t.VariantType).HasForeignKey(t => t.IsVariantOf);

            // DateTime specifics
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (entityType.IsKeyless)
                {
                    continue;
                }

                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// DB context extensions.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class DbContextExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A TContext extension method that queries if an entity is attached.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/07/2021.
        /// </remarks>
        /// <typeparam name="TContext">
        /// Type of the context.
        /// </typeparam>
        /// <typeparam name="TEntity">
        /// Type of the entity.
        /// </typeparam>
        /// <param name="context">
        /// The context to act on.
        /// </param>
        /// <param name="entity">
        /// The entity to check is attached or not.
        /// </param>
        /// <returns>
        /// True if the entity is attached, false if not.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static bool IsAttached<TContext, TEntity>(this TContext context, TEntity entity)
            where TContext : DbContext
            where TEntity : class
        {
            return context.Set<TEntity>().Local.Any(e => e == entity);
        }
    }
}