// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Base64.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Helpers
{
    using System;
    using System.Text;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Base64 string extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 06/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class Base64Extensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A string extension method that decodes from Base64.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="base64EncodedString">
        /// The Base64 encoded string to decode.
        /// </param>
        /// <returns>
        /// A plain text string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static string Base64Decode(this string base64EncodedString)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedString));
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A string extension method that encodes in Base64.
        /// </summary>
        /// <remarks>
        /// sushi.at, 06/05/2021.
        /// </remarks>
        /// <param name="plainText">
        /// The plainText to encode.
        /// </param>
        /// <returns>
        /// A Base64 string.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static string Base64Encode(this string plainText)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }
    }
}