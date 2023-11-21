// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeoLocateIPService.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Geo locate IP address service.
    /// </summary>
    /// <remarks>
    /// sushi.at, 09/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class GeoLocateIPService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The HTTP client.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly HttpClient httpClient;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<GeoLocateIPService> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoLocateIPService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/05/2021.
        /// </remarks>
        /// <param name="httpClient">
        /// The HTTP client.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public GeoLocateIPService(HttpClient httpClient, ILogger<GeoLocateIPService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;

            this.httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "keycdn-tools:https://api.opensky.to");
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Execute geo IP lookup request.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/05/2021.
        /// </remarks>
        /// <param name="ip">
        /// The IP to geo locate.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a string with the geo location "COUNTRY (CODE)".
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<string> Execute(string ip)
        {
            try
            {
                var requestUrl = $"https://tools.keycdn.com/geo.json?host={ip}";
                this.logger.LogInformation($"Requesting geo location for IP using: {requestUrl}");

                var response = await this.httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var geo = JObject.Parse(responseContent);
                var countryName = (string)geo["data"]?["geo"]?["country_name"];
                var countryCode = (string)geo["data"]?["geo"]?["country_code"];
                if (!string.IsNullOrEmpty(countryName) || !string.IsNullOrEmpty(countryCode))
                {
                    return $"{countryName ?? "Unknown"} ({countryCode ?? "??"})";
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"General error geo locating IP address {ip}");
            }

            return null;
        }
    }
}