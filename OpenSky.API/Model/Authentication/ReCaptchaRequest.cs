// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReCaptchaRequest.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Authentication
{
    using System;
    using System.Runtime.Serialization;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Google reCAPTCHAv3 request model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    [DataContract]
    public class ReCaptchaRequest
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ReCaptchaRequest"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <exception cref="Exception">
        /// Thrown when an exception error condition occurs.
        /// </exception>
        /// <param name="path">
        /// The API url path.
        /// </param>
        /// <param name="secret">
        /// The secret key.
        /// </param>
        /// <param name="token">
        /// The token from the client.
        /// </param>
        /// <param name="remoteIP">
        /// The remote IP of the client.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public ReCaptchaRequest(string path, string secret, string token, string remoteIP)
        {
            this.Token = token;
            this.RemoteIP = remoteIP;
            this.Secret = secret;
            this.Path = path;

            if (string.IsNullOrEmpty(this.Secret) || string.IsNullOrEmpty(this.Path))
            {
                //Invoke logger
                throw new Exception("Invalid 'Secret' or 'Path' properties in appsettings.json. Parent: GoogleReCaptchaV3.");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [DataMember(Name = "path")]
        public string Path { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the remote IP.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [DataMember(Name = "remoteip")]
        public string RemoteIP { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [DataMember(Name = "response")]
        public string Token { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the secret.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [DataMember(Name = "secret")]
        public string Secret { get; set; }
    }
}