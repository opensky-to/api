// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Main entry point.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class Program
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/05/2021.
        /// </remarks>
        /// <param name="args">
        /// The command line arguments arguments.
        /// </param>
        /// <returns>
        /// The new host builder.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Main entry-point for this application.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/05/2021.
        /// </remarks>
        /// <param name="args">
        /// The command line arguments arguments.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}