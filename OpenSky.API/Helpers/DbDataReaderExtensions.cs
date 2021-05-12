// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DbDataReaderExtensions.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Helpers
{
    using System.Data.Common;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Database reader extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 12/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class DbDataReaderExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// A DbDataReader extension method that calculates the SHA-1 hash code by combining all columns to a single string.
        /// </summary>
        /// <remarks>
        /// sushi.at, 12/05/2021.
        /// </remarks>
        /// <param name="reader">
        /// The reader to act on.
        /// </param>
        /// <returns>
        /// The calculated hash code (hex string).
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static string CalculateHashCode(this DbDataReader reader)
        {
            var stringValue = string.Empty;
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (!reader.IsDBNull(i))
                {
                    stringValue += reader.GetValue(i).ToString();
                }
                else
                {
                    stringValue += "NULL|";
                }
            }

            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(stringValue));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }
}