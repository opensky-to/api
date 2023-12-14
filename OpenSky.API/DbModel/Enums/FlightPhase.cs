// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightPhase.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Values that represent OpenSky flight phases.
    /// </summary>
    /// <remarks>
    /// sushi.at, 22/09/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum FlightPhase
    {
        /// <summary>
        /// Flight has been started but agent hasn't reported any status yet.
        /// </summary>
        Briefing = 0,

        /// <summary>
        /// Unknown flight phase (not connected, or not status received yet).
        /// </summary>
        Unknown = 1,

        /// <summary>
        /// Flight phase is not currently tracked.
        /// </summary>
        UnTracked = 2,

        /// <summary>
        /// Pre-flight phase (Plane spawned in, but engines are off).
        /// </summary>
        PreFlight = 10,

        /// <summary>
        /// Pushback phase (Pushback state active, engines either on or off).
        /// </summary>
        PushBack = 11,

        /// <summary>
        /// Taxi-out phase (Engines on, on the ground at the departure airport, and never airborne).
        /// </summary>
        TaxiOut = 12,

        /// <summary>
        /// Takeoff phase (Speeding up on the runway (GS > 40) and never airborne, including first few hundred feet of climb (depending on plane type).
        /// </summary>
        Takeoff = 13,

        /// <summary>
        /// Departure phase (distance to departure airport less 10nm and climbing).
        /// </summary>
        Departure = 14,

        /// <summary>
        /// Climb phase (can occur multiple times).
        /// </summary>
        Climb = 15,

        /// <summary>
        /// Not climbing or descending (can occur multiple times).
        /// </summary>
        Cruise = 16,

        /// <summary>
        /// Descent phase (can occur multiple times).
        /// </summary>
        Descent = 17,

        /// <summary>
        /// Distance to destination/alternate airport less than 40nm and descending, or distance less than 10nm.
        /// </summary>
        Approach = 18,

        /// <summary>
        /// Close the ground (less than 500 feet above ground) and less than 5nm from destination or alternate.
        /// </summary>
        Landing = 19,

        /// <summary>
        /// Go-around phase, ascending after landing or approach phase activated.
        /// </summary>
        GoAround = 20,

        /// <summary>
        /// Taxi-in phase (Moving on the ground after airborne, even if back at origin).
        /// </summary>
        TaxiIn = 21,

        /// <summary>
        /// Post-flight (Engines turned off, tracking is about to stop, just saving flight log reporting back).
        /// </summary>
        PostFlight = 22,

        /// <summary>
        /// Crashed (phase), oh oh :).
        /// </summary>
        Crashed = 99
    }
}