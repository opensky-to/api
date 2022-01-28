// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorldPopulationOverview.cs" company="OpenSky">
// OpenSky project 2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model
{
    using System.Collections.Generic;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// World population overview model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 02/07/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class WorldPopulationOverview
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
        public IEnumerable<PieChartValue> JobCategories { get; set; }

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
        /// Gets or sets the total number of airports.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalAirports { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of approaches.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalApproaches { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of jobs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalJobs { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the total number of runways.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int TotalRunways { get; set; }
    }
}