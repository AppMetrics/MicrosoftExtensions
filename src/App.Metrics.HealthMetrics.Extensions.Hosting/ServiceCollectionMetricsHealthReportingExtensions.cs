// <copyright file="ServiceCollectionMetricsHealthReportingExtensions.cs" company="App Metrics Contributors">
// Copyright (c) App Metrics Contributors. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Health;
using App.Metrics.Health.Extensions.Hosting;
using Microsoft.Extensions.Hosting;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
    // ReSharper restore CheckNamespace
{
    public static class ServiceCollectionMetricsHealthReportingExtensions
    {
        public static IServiceCollection AddHealthResultsAsMetricsHostedService(
            this IServiceCollection services,
            TimeSpan checkInterval,
            EventHandler<UnobservedTaskExceptionEventArgs> unobservedTaskExceptionHandler = null)
        {
            services.AddSingleton<IHostedService, HealthResultsAsMetricsBackgroundService>(serviceProvider =>
            {
                var metrics = serviceProvider.GetService<IMetrics>();

                if (metrics == null)
                {
                    throw new InvalidOperationException("IMetrics must be registered through for example IHostBuilder.ConfigureMetrics() or IServiceCollection.AddMetrics() to record health results as metrics.");
                }

                var healthCheckRunner = serviceProvider.GetService<IRunHealthChecks>();

                var instance = new HealthResultsAsMetricsBackgroundService(metrics, healthCheckRunner, checkInterval);

                if (unobservedTaskExceptionHandler != null)
                {
                    instance.UnobservedTaskException += unobservedTaskExceptionHandler;
                }

                return instance;
            });

            return services;
        }
    }
}
