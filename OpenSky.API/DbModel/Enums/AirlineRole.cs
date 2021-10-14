// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirlineRole.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airline roles.
    /// </summary>
    /// <remarks>
    /// sushi.at, 14/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum AirlineRole
    {
        /// <summary>
        /// External shareholder.
        /// </summary>
        Shareholder = 0,

        /// <summary>
        /// Shareholder that is also a member of the airline.
        /// </summary>
        MemberShareholder = 1,

        /// <summary>
        /// Member with dispatcher privileges (can also be shareholder).
        /// </summary>
        Dispatcher = 2,

        /// <summary>
        /// Member that also has the right to purchase and sell aircraft and FBOs (includes all lower roles).
        /// </summary>
        Procurement = 3,

        /// <summary>
        /// A member of the airline's board granting special voting rights (includes all lower roles).
        /// </summary>
        BoardMember = 4
    }
}