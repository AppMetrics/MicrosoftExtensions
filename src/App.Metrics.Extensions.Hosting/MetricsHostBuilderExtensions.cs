// <copyright file="MetricsHostBuilderExtensions.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using App.Metrics.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace App.Metrics.Extensions.Hosting
{
    public static class MetricsHostBuilderExtensions
    {
        private static bool _metricsBuilt;

        public static IHostBuilder ConfigureMetricsWithDefaults(
            this IHostBuilder hostBuilder,
            Action<HostBuilderContext, IMetricsBuilder> configureMetrics)
        {
            if (_metricsBuilt)
            {
                throw new InvalidOperationException("MetricsBuilder allows creation only of a single instance of IMetrics");
            }

            return hostBuilder.ConfigureServices(
                (context, services) =>
                {
                    var metricsBuilder = AppMetrics.CreateDefaultBuilder();
                    configureMetrics(context, metricsBuilder);
                    metricsBuilder.Configuration.ReadFrom(context.Configuration);
                    services.AddMetrics(metricsBuilder);
                    _metricsBuilt = true;
                });
        }

        public static IHostBuilder ConfigureMetricsWithDefaults(this IHostBuilder hostBuilder, Action<IMetricsBuilder> configureMetrics)
        {
            if (_metricsBuilt)
            {
                throw new InvalidOperationException("MetricsBuilder allows creation only of a single instance of IMetrics");
            }

            hostBuilder.ConfigureMetricsWithDefaults(
                (context, builder) =>
                {
                    configureMetrics(builder);
                });

            return hostBuilder;
        }

        public static IHostBuilder ConfigureMetrics(this IHostBuilder hostBuilder, IMetricsRoot metrics)
        {
            if (_metricsBuilt)
            {
                throw new InvalidOperationException("MetricsBuilder allows creation only of a single instance of IMetrics");
            }

            return hostBuilder.ConfigureServices(
                (context, services) =>
                {
                    services.AddMetrics(metrics);
                    _metricsBuilt = true;
                });
        }

        public static IHostBuilder ConfigureMetrics(
            this IHostBuilder hostBuilder,
            Action<HostBuilderContext, IMetricsBuilder> configureMetrics)
        {
            if (_metricsBuilt)
            {
                throw new InvalidOperationException("MetricsBuilder allows creation only of a single instance of IMetrics");
            }

            return hostBuilder.ConfigureServices(
                (context, services) =>
                {
                    services.AddMetrics(
                        builder =>
                        {
                            configureMetrics(context, builder);
                            builder.Configuration.ReadFrom(context.Configuration);
                            _metricsBuilt = true;
                        });
                });
        }

        public static IHostBuilder ConfigureMetrics(this IHostBuilder hostBuilder, Action<IMetricsBuilder> configureMetrics)
        {
            if (_metricsBuilt)
            {
                throw new InvalidOperationException("MetricsBuilder allows creation only of a single instance of IMetrics");
            }

            hostBuilder.ConfigureMetrics(
                (context, builder) =>
                {
                    configureMetrics(builder);
                });

            return hostBuilder;
        }

        public static IHostBuilder ConfigureMetrics(this IHostBuilder hostBuilder)
        {
            if (_metricsBuilt)
            {
                return hostBuilder;
            }

            return hostBuilder.ConfigureServices(
                (context, services) =>
                {
                    if (!_metricsBuilt)
                    {
                        var builder = AppMetrics.CreateDefaultBuilder()
                            .Configuration.ReadFrom(context.Configuration);
                        services.AddMetrics(builder);
                        _metricsBuilt = true;
                    }
                });
        }
    }
}