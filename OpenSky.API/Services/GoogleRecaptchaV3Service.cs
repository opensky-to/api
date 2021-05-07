// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GoogleRecaptchaV3Service.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Services
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;

    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    using OpenSky.API.Model.Authentication;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Google reCAPTCHAv3 service.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class GoogleRecaptchaV3Service
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
        private readonly ILogger<GoogleRecaptchaV3Service> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleRecaptchaV3Service"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="httpClient">
        /// The HTTP client.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public GoogleRecaptchaV3Service(HttpClient httpClient, ILogger<GoogleRecaptchaV3Service> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Execute Google RecaptchaV3 request.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <exception cref="ReCaptchaRequestException">
        /// Thrown when a custom reCAPTCHA error condition occurs.
        /// </exception>
        /// <param name="request">
        /// The reCAPTCHA request.
        /// </param>
        /// <returns>
        /// An asynchronous result that yields a ReCaptchaResponse.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<ReCaptchaResponse> Execute(ReCaptchaRequest request)
        {
            // Notes on error handling:
            // Google will pass back a 200 Status Ok response if no network or server errors occur.
            // If there are errors in on the "business" level, they will be coded in an array;
            // CaptchaRequestException is for these types of errors.

            // CaptchaRequestException and multiple catches are used to help separate the concerns of 
            //  a) an HttpRequest 400+ status code 
            //  b) an error at the "business" level 
            //  c) an unpredicted error that can only be handled generically.

            // It might be worthwhile to implement a "user error message" property in this class so the
            // calling procedure can decide what, if anything besides a server error, to return to the 
            // client and any client handling from there on.

            try
            {
                var requestUrl = request.Path + '?' + HttpUtility.UrlPathEncode($"secret={request.Secret}&response={request.Token}&remoteip={request.RemoteIP}");
                var content = new StringContent(requestUrl);
                this.logger.LogInformation($"Requesting Google reCAPTCHAv3 verification: {requestUrl}");

                var response = await this.httpClient.PostAsync(requestUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                this.logger.LogInformation($"Received Google reCAPTCHAv3 response: {responseContent}");

                var captchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(responseContent);
                if (captchaResponse?.Success != true)
                {
                    throw new ReCaptchaRequestException(captchaResponse);
                }

                return captchaResponse;
            }
            catch (HttpRequestException ex)
            {
                this.logger.LogError(ex, "HTTP request error executing Google RecaptchaV3 request.");
                return null;
            }
            catch (ReCaptchaRequestException ex)
            {
                //Business-level error... values are accessible in error-codes array.
                /*  Here are the possible "business" level codes:
                    missing-input-secret    The secret parameter is missing.
                    invalid-input-secret    The secret parameter is invalid or malformed.
                    missing-input-response  The response parameter is missing.
                    invalid-input-response  The response parameter is invalid or malformed.
                    bad-request             The request is invalid or malformed.
                    timeout-or-duplicate    The response is no longer valid: either is too old or has been used previously.
                */
                var errorDetails = ex.Response.ErrorCodes.Aggregate(string.Empty, (current, errorCode) => current + $"\r\n{errorCode}");
                this.logger.LogError(ex, $"Error executing Google RecaptchaV3 request.{errorDetails}");

                return ex.Response;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "General error executing Google RecaptchaV3 request.");
                return null;
            }
        }
    }
}