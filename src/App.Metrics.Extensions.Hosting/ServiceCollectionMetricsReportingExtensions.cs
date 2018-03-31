// <copyright file="ServiceCollectionMetricsReportingExtensions.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Extensions.Hosting;
using App.Metrics.Reporting;
using Microsoft.Extensions.Hosting;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
    // ReSharper restore CheckNamespace
{
    public static class ServiceCollectionMetricsReportingExtensions
    {
        public static IServiceCollection AddMetricsReportingHostedService(
            this IServiceCollection services,
            EventHandler<UnobservedTaskExceptionEventArgs> unobservedTaskExceptionHandler = null)
        {
            services.AddSingleton<IHostedService, MetricsReporterBackgroundService>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<MetricsOptions>();
                var metrics = serviceProvider.GetRequiredService<IMetrics>();
                var reporters = serviceProvider.GetService<IReadOnlyCollection<IReportMetrics>>();

                var instance = new MetricsReporterBackgroundService(metrics, options, reporters);

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
