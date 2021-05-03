// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;

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
            services.AddCors(
                options => options.AddPolicy(
                    "OpenSkyAllowSpecificOrigins",
                    builder =>
                    {
                        //builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                        builder.WithOrigins("https://www.opensky.to", "http://localhost:5000").AllowAnyHeader().AllowAnyMethod();
                    }));

            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenSky.API", Version = "v1" }); });
        }
    }
}