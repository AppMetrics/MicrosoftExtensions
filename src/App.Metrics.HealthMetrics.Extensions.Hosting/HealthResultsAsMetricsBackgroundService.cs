// <copyright file="HealthResultsAsMetricsBackgroundService.cs" company="App Metrics Contributors">
// Copyright (c) App Metrics Contributors. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Counter;
using App.Metrics.Health.Logging;
using Microsoft.Extensions.Hosting;

namespace App.Metrics.Health.Extensions.Hosting
{
    public class HealthResultsAsMetricsBackgroundService : BackgroundService
    {
        private static readonly ILog Logger = LogProvider.For<HealthResultsAsMetricsBackgroundService>();
        private static readonly TimeSpan WaitBetweenReportRunChecks = TimeSpan.FromMilliseconds(500);
        private readonly IMetrics _metrics;
        private readonly CounterOptions _successCounter;
        private readonly CounterOptions _failedCounter;
        private readonly List<SchedulerTaskWrapper> _scheduledReporters = new List<SchedulerTaskWrapper>();

        public HealthResultsAsMetricsBackgroundService(
            IMetrics metrics,
            IRunHealthChecks healthCheckRunner,
            TimeSpan checkInterval)
        {
            _metrics = metrics;

            if (checkInterval == TimeSpan.Zero)
            {
                throw new ArgumentException("Must be greater than zero", nameof(checkInterval));
            }

            var referenceTime = DateTime.UtcNow;

            _successCounter = new CounterOptions
            {
                Context = AppMetricsConstants.InternalMetricsContext,
                MeasurementUnit = Unit.Items,
                ResetOnReporting = true,
                Name = "report_health_success"
            };

            _failedCounter = new CounterOptions
            {
                Context = AppMetricsConstants.InternalMetricsContext,
                MeasurementUnit = Unit.Items,
                ResetOnReporting = true,
                Name = "report_health_failed"
            };

            _scheduledReporters.Add(
                new SchedulerTaskWrapper
                {
                    Interval = checkInterval,
                    HealthCheckRunner = healthCheckRunner,
                    NextRunTime = referenceTime
                });
        }

        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!_scheduledReporters.Any())
            {
                await Task.CompletedTask;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(cancellationToken);

                Logger.Trace($"Delaying for {WaitBetweenReportRunChecks}");

                await Task.Delay(WaitBetweenReportRunChecks, cancellationToken);
            }
        }

        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);
            var referenceTime = DateTime.UtcNow;

            foreach (var recordTask in _scheduledReporters)
            {
                if (!recordTask.ShouldRun(referenceTime))
                {
                    Logger.Trace($"Skipping {recordTask.HealthCheckRunner.GetType().FullName}, next run in {recordTask.NextRunTime.Subtract(referenceTime).Milliseconds} ms");
                    continue;
                }

                recordTask.Increment();

                await taskFactory.StartNew(
                    async () =>
                    {
                        try
                        {
                            Logger.Trace($"Executing reporter {recordTask.HealthCheckRunner.GetType().FullName} FlushAsync");

                            var healthStatus = await recordTask.HealthCheckRunner.ReadAsync(cancellationToken);

                            foreach (var healthResult in healthStatus.Results)
                            {
                                var tags = new MetricTags(HealthReportingConstants.TagKeys.HealthCheckName, healthResult.Name);

                                if (healthResult.Check.Status == HealthCheckStatus.Degraded)
                                {
                                    _metrics.Measure.Gauge.SetValue(ApplicationHealthMetricRegistry.Checks, tags, HealthConstants.HealthScore.degraded);
                                }
                                else if (healthResult.Check.Status == HealthCheckStatus.Unhealthy)
                                {
                                    _metrics.Measure.Gauge.SetValue(ApplicationHealthMetricRegistry.Checks, tags, HealthConstants.HealthScore.unhealthy);
                                }
                                else if (healthResult.Check.Status == HealthCheckStatus.Healthy)
                                {
                                    _metrics.Measure.Gauge.SetValue(ApplicationHealthMetricRegistry.Checks, tags, HealthConstants.HealthScore.healthy);
                                }
                            }

                            var overallHealthStatus = HealthConstants.HealthScore.healthy;

                            if (healthStatus.Status == HealthCheckStatus.Unhealthy)
                            {
                                overallHealthStatus = HealthConstants.HealthScore.unhealthy;
                            }
                            else if (healthStatus.Status == HealthCheckStatus.Degraded)
                            {
                                overallHealthStatus = HealthConstants.HealthScore.degraded;
                            }

                            _metrics.Measure.Gauge.SetValue(ApplicationHealthMetricRegistry.HealthGauge, overallHealthStatus);

                            _metrics.Measure.Counter.Increment(_successCounter, recordTask.HealthCheckRunner.GetType().FullName);

                            Logger.Trace($"HealthCheckRunner {recordTask.HealthCheckRunner.GetType().FullName} executed successfully");
                        }
                        catch (Exception ex)
                        {
                            _metrics.Measure.Counter.Increment(_failedCounter, recordTask.HealthCheckRunner.GetType().FullName);

                            var args = new UnobservedTaskExceptionEventArgs(
                                ex as AggregateException ?? new AggregateException(ex));

                            Logger.Error($"HealthCheckRunner {recordTask.HealthCheckRunner.GetType().FullName} FlushAsync failed", ex);

                            UnobservedTaskException?.Invoke(this, args);

                            if (!args.Observed)
                            {
                                throw;
                            }
                        }
                    },
                    cancellationToken);
            }
        }

        private class SchedulerTaskWrapper
        {
            public TimeSpan Interval { get; set; }

            public DateTime LastRunTime { get; set; }

            public DateTime NextRunTime { get; set; }

            public IRunHealthChecks HealthCheckRunner { get; set; }

            public void Increment()
            {
                LastRunTime = NextRunTime;
                NextRunTime = DateTime.UtcNow.Add(Interval);
            }

            public bool ShouldRun(DateTime currentTime) { return NextRunTime < currentTime && LastRunTime != NextRunTime; }
        }
    }
}