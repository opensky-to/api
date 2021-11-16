// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlightTrackingEventType.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Flight tracking event types.
    /// </summary>
    /// <remarks>
    /// sushi.at, 16/11/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum FlightTrackingEventType
    {
        /// <summary>
        /// Tracking started.
        /// </summary>
        TrackingStarted = 0,

        /// <summary>
        /// Tracking resumed.
        /// </summary>
        TrackingResumed = 1,

        /// <summary>
        /// Tracking paused.
        /// </summary>
        TrackingPaused = 2,

        /// <summary>
        /// Tracking stopped.
        /// </summary>
        TrackingStopped = 3,

        /// <summary>
        /// Pushback started.
        /// </summary>
        PushbackStarted = 4,

        /// <summary>
        /// Pushback finished.
        /// </summary>
        PushbackFinished = 5,

        /// <summary>
        /// Ground handling complete (fuel and payload).
        /// </summary>
        GroundHandlingComplete = 6,

        /// <summary>
        /// Sim rate changed.
        /// </summary>
        SimRateChanged = 10,

        /// <summary>
        /// Skipped all of ground handling.
        /// </summary>
        SkippedGroundHandling = 11,

        /// <summary>
        /// Skipped half of ground handling duration.
        /// </summary>
        SkippedHalfGroundHandling = 12,

        /// <summary>
        /// Airborne.
        /// </summary>
        Airborne = 20,

        /// <summary>
        /// Touchdown.
        /// </summary>
        Touchdown = 21,

        /// <summary>
        /// Crashed :(.
        /// </summary>
        Crashed = 22,

        /// <summary>
        /// Beacon ON/OFF.
        /// </summary>
        Beacon = 30,

        /// <summary>
        /// Nav lights ON/OFF.
        /// </summary>
        NavLights = 31,

        /// <summary>
        /// Strobe ON/OFF.
        /// </summary>
        Strobe = 32,

        /// <summary>
        /// Taxi lights ON/OFF.
        /// </summary>
        TaxiLights = 33,

        /// <summary>
        /// Landing lights ON/OFF.
        /// </summary>
        LandingLights = 34,

        /// <summary>
        /// Beacon OFF while engines ON :(.
        /// </summary>
        BeaconOffEnginesOn = 35,

        /// <summary>
        /// Landing lights off below 10k feet (for jets and turbos).
        /// </summary>
        LandingLightsOffBelow10K = 36,

        /// <summary>
        /// Landing lights off below 300 AGL (for pistons, etc.).
        /// </summary>
        LandingLightsOffBelow300AGL = 37,

        /// <summary>
        /// Taxi or landing lights were ON while the engines were turned on/off.
        /// </summary>
        TaxiLandingLightsEngine = 38,

        /// <summary>
        /// Engine ON/OFF.
        /// </summary>
        Engine = 40,

        /// <summary>
        /// Engine turned OFF on the runway (after landing).
        /// </summary>
        EngineOffRunway = 41,

        /// <summary>
        /// Battery master ON/OFF.
        /// </summary>
        BatteryMaster = 42,

        /// <summary>
        /// Landing gear lowered/raised.
        /// </summary>
        LandingGear = 43,

        /// <summary>
        /// Tried to raise gear while on the ground :(.
        /// </summary>
        GearUpOnGround = 44,

        /// <summary>
        /// Flaps moved.
        /// </summary>
        Flaps = 45,

        /// <summary>
        /// Auto pilot ON/OFF.
        /// </summary>
        AutoPilot = 46,

        /// <summary>
        /// Parking brake  ON/OFF.
        /// </summary>
        ParkingBrake = 47,

        /// <summary>
        /// Spoilers armed/disarmed.
        /// </summary>
        Spoilers = 48,

        /// <summary>
        /// APU  ON/OFF.
        /// </summary>
        APU = 49,

        /// <summary>
        /// Seat belt sings  ON/OFF.
        /// </summary>
        SeatbeltSigns = 50,

        /// <summary>
        /// No smoking signs  ON/OFF.
        /// </summary>
        NoSmokingSigns = 51,

        /// <summary>
        /// Over-speed :(.
        /// </summary>
        Overspeed = 60,

        /// <summary>
        /// Stall :(.
        /// </summary>
        Stall = 61
    }
}