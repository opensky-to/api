// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API
{
    using System;
    using System.Text;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;

    using OpenSky.API.DbModel;

    using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// API startup class.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class Startup
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/05/2021.
        /// </remarks>
        /// <param name="configuration">
        /// Gets the configuration.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IConfiguration Configuration { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request
        /// pipeline.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/05/2021.
        /// </remarks>
        /// <param name="app">
        /// The application.
        /// </param>
        /// <param name="env">
        /// The environment.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenSky.API v1"));
            }

            app.UseRouting();
            app.UseCors("OpenSkyAllowSpecificOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/05/2021.
        /// </remarks>
        /// <param name="services">
        /// The services collection.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void ConfigureServices(IServiceCollection services)
        {
            // Set up cross-origin resource sharing policy for our website and local development
            services.AddCors(
                options => options.AddPolicy(
                    "OpenSkyAllowSpecificOrigins",
                    builder =>
                    {
                        builder.WithOrigins("https://www.opensky.to", "http://localhost:5000").AllowAnyHeader().AllowAnyMethod();
                    }));

            services.AddDbContextPool<OpenSkyDbContext>(options => options.UseMySql(this.Configuration.GetConnectionString("OpenSkyConnectionString"), ServerVersion.Parse("10.4.18", ServerType.MariaDb)));
            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenSky.API", Version = "v1" }); });

            // Set identity with rules inspired by reading https://blog.codinghorror.com/password-rules-are-bullshit/
            services.AddIdentity<OpenSkyUser, IdentityRole>(
                        options =>
                        {
                            options.Password.RequiredLength = 10;
                            options.Password.RequiredUniqueChars = 6;
                        })
                    .AddEntityFrameworkStores<OpenSkyDbContext>()
                    .AddDefaultTokenProviders()
                    .AddTop1000PasswordValidator<OpenSkyUser>();
            services.AddAuthentication(
                options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                    .AddJwtBearer(options =>
                    {
                        options.SaveToken = true;
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidAudience = this.Configuration["JWT:ValidAudience"],
                            ValidIssuer = this.Configuration["JWT:ValidIssuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["JWT:Secret"]))
                        };
                    });
        }
    }
}