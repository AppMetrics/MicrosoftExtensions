// <copyright file="HealthHostBuilderExtensions.cs" company="App Metrics Contributors">
// Copyright (c) App Metrics Contributors. All rights reserved.
// </copyright>

using System;
using System.Reflection;
using App.Metrics.Health;
using App.Metrics.Health.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.Hosting
    // ReSharper restore CheckNamespace
{
    public static class HealthHostBuilderExtensions
    {
        private static bool _healthBuilt;

        public static IHostBuilder ConfigureHealthWithDefaults(
            this IHostBuilder hostBuilder,
            Action<HostBuilderContext, IHealthBuilder> configureHealth,
            DependencyContext dependencyContext = null)
        {
            if (_healthBuilt)
            {
                throw new InvalidOperationException("HealthBuilder allows creation only of a single instance of IMetrics");
            }

            return hostBuilder.ConfigureServices(
                (context, services) =>
                {
                    var healthBuilder = AppMetricsHealth.CreateDefaultBuilder();
                    configureHealth(context, healthBuilder);
                    healthBuilder.HealthChecks.RegisterFromAssembly(services, dependencyContext ?? GetDependencyContext());
                    healthBuilder.Configuration.ReadFrom(context.Configuration);
                    healthBuilder.BuildAndAddTo(services);
                    _healthBuilt = true;
                });
        }

        public static IHostBuilder ConfigureHealthWithDefaults(this IHostBuilder hostBuilder, Action<IHealthBuilder> configureHealth)
        {
            if (_healthBuilt)
            {
                throw new InvalidOperationException("HealthBuilder allows creation only of a single instance of IHealth");
            }

            hostBuilder.ConfigureHealthWithDefaults(
                (context, builder) =>
                {
                    configureHealth(builder);
                });

            return hostBuilder;
        }

        public static IHostBuilder ConfigureHealth(
            this IHostBuilder hostBuilder,
            Action<HostBuilderContext, IHealthBuilder> configureHealth)
        {
            if (_healthBuilt)
            {
                throw new InvalidOperationException("HealthBuilder allows creation only of a single instance of IHealth");
            }

            return hostBuilder.ConfigureServices(
                (context, services) =>
                {
                    services.AddHealth(
                        healthBuilder =>
                        {
                            configureHealth(context, healthBuilder);
                            healthBuilder.Configuration.ReadFrom(context.Configuration);
                            _healthBuilt = true;
                        });
                });
        }

        public static IHostBuilder ConfigureHealth(this IHostBuilder hostBuilder, Action<IHealthBuilder> configureHealth)
        {
            if (_healthBuilt)
            {
                throw new InvalidOperationException("HealthBuilder allows creation only of a single instance of IHealth");
            }

            hostBuilder.ConfigureHealth(
                (context, healthBuilder) =>
                {
                    configureHealth(healthBuilder);
                });

            return hostBuilder;
        }

        public static IHostBuilder ConfigureHealth(
            this IHostBuilder hostBuilder,
            DependencyContext dependencyContext = null)
        {
            if (_healthBuilt)
            {
                return hostBuilder;
            }

            return hostBuilder.ConfigureServices(
                (context, services) =>
                {
                    if (!_healthBuilt)
                    {
                        AppMetricsHealth.CreateDefaultBuilder()
                            .Configuration.ReadFrom(context.Configuration)
                            .HealthChecks.RegisterFromAssembly(services, dependencyContext ?? GetDependencyContext())
                            .BuildAndAddTo(services);

                        _healthBuilt = true;
                    }
                });
        }

        internal static DependencyContext GetDependencyContext()
        {
            return Assembly.GetEntryAssembly() != null ? DependencyContext.Default : null;
        }
    }
}