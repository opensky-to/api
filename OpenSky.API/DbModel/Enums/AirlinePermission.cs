﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirlinePermission.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#pragma warning disable 1591
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
    public enum AirlinePermission
    {
        BuyAircraft = 10,
        SellAircraft = 11,
        RentAircraft = 12,
        RentOutAircraft = 13,
        AssignAircraft = 14,
        RenameAircraft = 15,

        BuyFBO = 20,
        SellFBO = 21,
        RentFBO = 22,
        RentOutFBO =23,
        RenameFBO = 24,
        OrderFuel = 25,

        AcceptJobs = 30,
        Dispatch = 31,
        OutsourceJobs = 32,

        ModifyAircraft = 40,
        MaintainAircraft = 41,
        ReplaceAircraftParts = 42,

        ChangePermissions = 90,
        BoardMember = 91,
        AllPermissions = 92
    }
}