// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatisticsService.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;
    using OpenSky.API.Model;
    using OpenSky.API.Services.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// A service for collecting statistics.
    /// </summary>
    /// <remarks>
    /// sushi.at, 28/01/2022.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class StatisticsService
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The OpenSky database context
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly OpenSkyDbContext db;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight mutex object.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly object flightMutex = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The job mutex object.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly object jobMutex = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The job generated mutex object.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly object jobGeneratedMutex = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The logger.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly ILogger<StatisticsService> logger;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsService"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 28/01/2022.
        /// </remarks>
        /// <param name="services">
        /// The services context.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public StatisticsService(IServiceProvider services, ILogger<StatisticsService> logger)
        {
            this.logger = logger;
            this.db = services.CreateScope().ServiceProvider.GetRequiredService<OpenSkyDbContext>();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight aircraft type pie chart series.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the flight aircraft type pie series
        /// in this collection.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> GetFlightAircraftTypePieSeries()
        {
            var series = new List<PieChartValue>();
            var stats = this.db.Statistics.Where(s => s.Key.StartsWith("flight|aircraftType|"));

            foreach (var stat in stats)
            {
                series.Add(
                    new PieChartValue
                    {
                        Key = stat.Key.Split('|')[2],
                        Value = (long)stat.Value
                    });
            }

            return series;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the flight operator pie chart series.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the flight operator pie series in
        /// this collection.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> GetFlightOperatorPieSeries()
        {
            var series = new List<PieChartValue>();
            var stats = this.db.Statistics.Where(s => s.Key.StartsWith("flight|operator|"));

            foreach (var stat in stats)
            {
                series.Add(
                    new PieChartValue
                    {
                        Key = stat.Key.Split('|')[2],
                        Value = (long)stat.Value
                    });
            }

            return series;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the job aircraft type pie chart series.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the job aircraft type pie series in
        /// this collection.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> GetJobAircraftTypePieSeries()
        {
            var series = new List<PieChartValue>();
            var stats = this.db.Statistics.Where(s => s.Key.StartsWith("job|aircraftType|"));

            foreach (var stat in stats)
            {
                series.Add(
                    new PieChartValue
                    {
                        Key = stat.Key.Split('|')[2],
                        Value = (long)stat.Value
                    });
            }

            return series;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the job operator pie chart series.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the job operator pie series in this
        /// collection.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> GetJobOperatorPieSeries()
        {
            var series = new List<PieChartValue>();
            var stats = this.db.Statistics.Where(s => s.Key.StartsWith("job|operator|"));

            foreach (var stat in stats)
            {
                series.Add(
                    new PieChartValue
                    {
                        Key = stat.Key.Split('|')[2],
                        Value = (long)stat.Value
                    });
            }

            return series;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the job type pie chart series.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the job type pie series in this
        /// collection.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public IEnumerable<PieChartValue> GetJobTypePieSeries()
        {
            var series = new List<PieChartValue>();
            var stats = this.db.Statistics.Where(s => s.Key.StartsWith("job|type|"));

            foreach (var stat in stats)
            {
                series.Add(
                    new PieChartValue
                    {
                        Key = stat.Key.Split('|')[2],
                        Value = (long)stat.Value
                    });
            }

            return series;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the total flight count.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the total flight count.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<int> GetTotalFlightCount()
        {
            try
            {
                const string flightTotalKey = "flight";
                var flightTotalStat = await this.db.Statistics.SingleOrDefaultAsync(s => s.Key == flightTotalKey);
                if (flightTotalStat != null)
                {
                    return (int)flightTotalStat.Value;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving total flight count");
            }

            return 0;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the total job count.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the total job count.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<int> GetTotalJobCount()
        {
            try
            {
                const string jobTotalKey = "job";
                var jobTotalStat = await this.db.Statistics.SingleOrDefaultAsync(s => s.Key == jobTotalKey);
                if (jobTotalStat != null)
                {
                    return (int)jobTotalStat.Value;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving total job count");
            }

            return 0;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets job generated count.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <returns>
        /// An asynchronous result that yields the job generated count.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public async Task<int> GetJobGeneratedCount()
        {
            try
            {
                const string jobTotalKey = "jobGenerated";
                var jobTotalStat = await this.db.Statistics.SingleOrDefaultAsync(s => s.Key == jobTotalKey);
                if (jobTotalStat != null)
                {
                    return (int)jobTotalStat.Value;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving job generated count");
            }

            return 0;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Record completed flight stats.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <param name="flightOperator">
        /// The flight operator.
        /// </param>
        /// <param name="aircraftTypeCategory">
        /// Category type of the aircraft.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public void RecordCompletedFlight(FlightOperator flightOperator, AircraftTypeCategory aircraftTypeCategory)
        {
            try
            {
                lock (this.flightMutex)
                {
                    const string flightTotalKey = "flight";
                    var flightTotalStat = this.db.Statistics.SingleOrDefault(s => s.Key == flightTotalKey);
                    if (flightTotalStat != null)
                    {
                        flightTotalStat.Value++;
                    }
                    else
                    {
                        this.db.Statistics.Add(
                            new Statistic
                            {
                                Key = flightTotalKey,
                                Value = 1
                            });
                    }

                    var operatorKey = $"flight|operator|{flightOperator}";
                    var operatorStat = this.db.Statistics.SingleOrDefault(s => s.Key == operatorKey);
                    if (operatorStat != null)
                    {
                        operatorStat.Value++;
                    }
                    else
                    {
                        this.db.Statistics.Add(
                            new Statistic
                            {
                                Key = operatorKey,
                                Value = 1
                            });
                    }

                    var aircraftTypeKey = $"flight|aircraftType|{aircraftTypeCategory}";
                    var aircraftTypeStat = this.db.Statistics.SingleOrDefault(s => s.Key == aircraftTypeKey);
                    if (aircraftTypeStat != null)
                    {
                        aircraftTypeStat.Value++;
                    }
                    else
                    {
                        this.db.Statistics.Add(
                            new Statistic
                            {
                                Key = aircraftTypeKey,
                                Value = 1
                            });
                    }

                    this.db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error recording completed flight.");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Record job generated count.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <param name="numberOfJobs">
        /// Number of jobs that were generated.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public void RecordJobsGenerated(int numberOfJobs)
        {
            try
            {
                lock (this.jobGeneratedMutex)
                {
                    const string jobGeneratedKey = "jobGenerated";
                    var jobGeneratedStat = this.db.Statistics.SingleOrDefault(s => s.Key == jobGeneratedKey);
                    if (jobGeneratedStat != null)
                    {
                        jobGeneratedStat.Value += numberOfJobs;
                    }
                    else
                    {
                        this.db.Statistics.Add(
                            new Statistic
                            {
                                Key = jobGeneratedKey,
                                Value = numberOfJobs
                            });
                    }

                    this.db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error recording jobs generated.");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Record completed job stats.
        /// </summary>
        /// <remarks>
        /// sushi.at, 29/01/2022.
        /// </remarks>
        /// <param name="flightOperator">
        /// The flight operator.
        /// </param>
        /// <param name="aircraftTypeCategory">
        /// Category type of the aircraft.
        /// </param>
        /// <param name="jobType">
        /// Type of the job.
        /// </param>
        /// <returns>
        /// An asynchronous result.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public void RecordCompletedJob(FlightOperator flightOperator, AircraftTypeCategory aircraftTypeCategory, JobType jobType)
        {
            try
            {
                lock(this.jobMutex)
                {
                    const string jobTotalKey = "job";
                    var jobTotalStat = this.db.Statistics.SingleOrDefault(s => s.Key == jobTotalKey);
                    if (jobTotalStat != null)
                    {
                        jobTotalStat.Value++;
                    }
                    else
                    {
                        this.db.Statistics.Add(
                            new Statistic
                            {
                                Key = jobTotalKey,
                                Value = 1
                            });
                    }

                    var operatorKey = $"job|operator|{flightOperator}";
                    var operatorStat = this.db.Statistics.SingleOrDefault(s => s.Key == operatorKey);
                    if (operatorStat != null)
                    {
                        operatorStat.Value++;
                    }
                    else
                    {
                        this.db.Statistics.Add(
                            new Statistic
                            {
                                Key = operatorKey,
                                Value = 1
                            });
                    }

                    var aircraftTypeKey = $"job|aircraftType|{aircraftTypeCategory}";
                    var aircraftTypeStat = this.db.Statistics.SingleOrDefault(s => s.Key == aircraftTypeKey);
                    if (aircraftTypeStat != null)
                    {
                        aircraftTypeStat.Value++;
                    }
                    else
                    {
                        this.db.Statistics.Add(
                            new Statistic
                            {
                                Key = aircraftTypeKey,
                                Value = 1
                            });
                    }

                    var jobTypeKey = $"job|type|{jobType}";
                    var jobTypeStat = this.db.Statistics.SingleOrDefault(s => s.Key == jobTypeKey);
                    if (jobTypeStat != null)
                    {
                        jobTypeStat.Value++;
                    }
                    else
                    {
                        this.db.Statistics.Add(
                            new Statistic
                            {
                                Key = jobTypeKey,
                                Value = 1
                            });
                    }

                    this.db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error recording completed job.");
            }
        }
    }
}