// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobType.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Job types.
    /// </summary>
    /// <remarks>
    /// sushi.at, 09/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------

    // ReSharper disable InconsistentNaming
    public enum JobType
    {
        /// <summary>
        /// Cargo job (long).
        /// </summary>
        Cargo_L = 0,

        /// <summary>
        /// Cargo job (short).
        /// </summary>
        Cargo_S = 1
    }
}