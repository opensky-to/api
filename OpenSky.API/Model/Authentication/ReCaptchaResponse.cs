// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReCaptchaResponse.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System.Runtime.Serialization;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Google reCAPTCHAv3 response model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [DataContract]
    public class ReCaptchaResponse
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the challenge timestamp (ISO format yyyy-MM-dd'T'HH:mm:ssZZ).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [DataMember(Name = "challenge_ts")]
        public string ChallengeTS { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the error codes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [DataMember(Name = "error-codes")]
        public string[] ErrorCodes { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the hostname.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [DataMember(Name = "hostname")]
        public string Hostname { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the score (1.0 good interaction, 0.0 bot).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [DataMember(Name = "score")]
        public double Score { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether the request was successful.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [DataMember(Name = "success")]
        public bool Success { get; set; }
    }
}