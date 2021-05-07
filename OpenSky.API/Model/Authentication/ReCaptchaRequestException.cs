// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReCaptchaRequestException.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// reCAPTCHA request exception.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ReCaptchaRequestException : Exception
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ReCaptchaRequestException"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="response">
        /// The reCAPTCHA request response.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public ReCaptchaRequestException(ReCaptchaResponse response)
        {
            this.Response = response;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the reCAPTCHA request response.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public ReCaptchaResponse Response { get; }
    }
}