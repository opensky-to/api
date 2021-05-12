// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Startup.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;

    using MailKit.Security;

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;

    using OpenSky.API.DbModel;
    using OpenSky.API.Helpers;
    using OpenSky.API.Services;
    using OpenSky.API.Workers;

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
        /// <param name="dbContext">
        /// OpenSky database context.
        /// </param>
        /// <param name="config">
        /// The configuration.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, OpenSkyDbContext dbContext, IConfiguration config)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenSky.API v1"));
            }

            app.UseRouting();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                   ForwardedHeaders.XForwardedProto
            });
            app.UseCors("OpenSkyAllowSpecificOrigins");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            // Apply automatic database migrations?
            if (bool.Parse(config["ConnectionStrings:ApplyMigrations"]))
            {
                dbContext.Database.Migrate();
            }
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
                        builder.WithOrigins("https://www.opensky.to", "http://localhost:5001").AllowAnyHeader().AllowAnyMethod();
                    }));

            // Primary database connection pool
            services.AddDbContextPool<OpenSkyDbContext>(options =>
            {
                options.UseMySql(this.Configuration.GetConnectionString("OpenSkyConnectionString"), ServerVersion.Parse("10.4.18", ServerType.MariaDb));
            });
            
            // Add swagger
            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenSky.API", Version = "v1" });
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                            "Enter 'Bearer' [space] and then your token in the text input below." +
                            "\r\n\r\nExample: 'Bearer xxxxxxxxxxxxxxxxx'",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                },
                                Scheme = "oauth2",
                                Name = "Bearer",
                                In = ParameterLocation.Header,

                            },
                            new List<string>()
                        }
                    });
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath);
                });

            // Email service
            services.AddSingleton(
                new SendMail(
                    this.Configuration["Email:SmtpServer"],
                    int.Parse(this.Configuration["Email:SmtpPort"]),
                    this.Configuration["Email:UserName"],
                    this.Configuration["Email:Password"],
                    Enum.Parse<SecureSocketOptions>(this.Configuration["Email:SecureSocketOptions"])));

            // Set identity with rules inspired by reading https://blog.codinghorror.com/password-rules-are-bullshit/
            services.AddIdentity<OpenSkyUser, IdentityRole>(
                        options =>
                        {
                            options.Password.RequiredLength = 10;
                            options.Password.RequiredUniqueChars = 6;
                            options.User.RequireUniqueEmail = true;
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

            // Set up Google reCAPTCHAv3 service
            services.AddHttpClient<GoogleRecaptchaV3Service>();
            services.AddSingleton<GoogleRecaptchaV3Service>();
            
            // Set up geo location service
            services.AddHttpClient<GeoLocateIPService>();
            services.AddSingleton<GeoLocateIPService>();

            // Set up hosted worker services
            services.AddHostedService<DataImportWorkerService>();

            // API controllers
            services.AddControllers();
        }
    }
}