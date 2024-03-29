﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldStatisticsOverview.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model
{
    using System.Collections.Generic;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// World statistics overview model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class WorldStatisticsOverview
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for aircraft categories.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> AircraftCategories { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for aircraft ownership.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> AircraftOwner { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for airport sizes.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> AirportSizes { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for approach types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> ApproachTypes { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the completed flights.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int CompletedFlights { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the completed flights for MSFS.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int CompletedFlightsMSFS { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the completed flights for XPlane11.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int CompletedFlightsXP11 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for flight aircraft categories.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> FlightAircraftCategories { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for flight operators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> FlightOperators { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for fuel availability at airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> FuelAvailability { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for job categories.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> JobAircraftCategories { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for job operators.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> JobOperators { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the jobs generated.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int JobsGenerated { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for job types.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> JobTypes { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for runway lights.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> RunwayLights { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a pie chart series for runway surfaces.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> RunwaySurfaces { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of aircraft.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalAircraft { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of aircraft for MSFS.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalAircraftMSFS { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of aircraft for XPlane11.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalAircraftXP11 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalAirports { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of airports for MSFS.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalAirportsMSFS { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of airports for XPlane11.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalAirportsXP11 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of approaches.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalApproaches { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of approaches for MSFS.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalApproachesMSFS { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of approaches for XPlane11.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalApproachesXP11 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of jobs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalJobs { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of jobs for MSFS.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalJobsMSFS { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of jobs for XPlane11.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalJobsXP11 { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of runways.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalRunways { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of runways for MSFS.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalRunwaysMSFS { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of runways for XPlane11.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalRunwaysXP11 { get; set; }
    }
}