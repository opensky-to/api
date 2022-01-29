// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FinancialCategory.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.DbModel.Enums
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Financial categories.
    /// </summary>
    /// <remarks>
    /// sushi.at, 27/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum FinancialCategory
    {
        /// <summary>
        /// No financial category.
        /// </summary>
        None = 0,

        /// <summary>
        /// Purchase or sale of aircraft.
        /// </summary>
        Aircraft = 1,

        /// <summary>
        /// Purchase or sale of fuel.
        /// </summary>
        Fuel = 2,

        /// <summary>
        /// Purchase or sale of aircraft maintenance.
        /// </summary>
        Maintenance = 3,

        /// <summary>
        /// Airport fees (landing, parking, handling etc.).
        /// </summary>
        AirportFees = 4,

        /// <summary>
        /// Salaries for employees.
        /// </summary>
        Salaries = 5,

        /// <summary>
        /// Initial loan received as well as it's repayments.
        /// </summary>
        Loan = 6,

        /// <summary>
        /// Loan interests.
        /// </summary>
        Interest = 7,

        /// <summary>
        /// Purchase or sale of airline shares.
        /// </summary>
        Shares = 8,

        /// <summary>
        /// Dividends paid or received for airline shares.
        /// </summary>
        Dividend = 9,

        /// <summary>
        /// Construction/expansion of FBOs.
        /// </summary>
        FBO = 10,

        /// <summary>
        /// Income from transporting cargo.
        /// </summary>
        Cargo = 11,

        /// <summary>
        /// Income from transporting passengers.
        /// </summary>
        Passengers = 12,

        /// <summary>
        /// Income from specialty jobs like skydiving or sightseeing.
        /// </summary>
        SpecialtyJobs = 13,

        /// <summary>
        /// Fines and penalties for late delivery, rule violations, job cancellation etc.
        /// </summary>
        Fines = 14,
    }
}