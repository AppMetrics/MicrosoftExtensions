// <copyright file="ServiceCollectionHealthBuilderExtensions.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using App.Metrics.Health;
using App.Metrics.Health.Formatters;
using App.Metrics.Health.Internal;
using App.Metrics.Health.Internal.NoOp;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
    // ReSharper restore CheckNamespace
{
    public static class ServiceCollectionHealthBuilderExtensions
    {
        private static bool _hasResolvedChecks;
        private static IEnumerable<HealthCheck> _resolvedChecks = Enumerable.Empty<HealthCheck>();

        public static IHealthRoot BuildAndAddTo(
            this IHealthBuilder builder,
            IServiceCollection services)
        {
            var health = builder.Build();

            services.TryAddSingleton<IHealth>(provider =>
            {
                var checks = ResolveAllHealthChecks(provider, health);
                return new DefaultHealth(checks);
            });
            services.TryAddSingleton<IHealthRoot>(
                provider =>
                {
                    var resolvedHealth = provider.GetRequiredService<IHealth>();
                    var resolvedHealthChecksRunner = provider.GetRequiredService<IRunHealthChecks>();

                    return new HealthRoot(
                        resolvedHealth,
                        health.Options,
                        health.OutputHealthFormatters as HealthFormatterCollection,
                        health.DefaultOutputHealthFormatter,
                        resolvedHealthChecksRunner);
                });
            services.TryAddSingleton<IRunHealthChecks>(provider =>
            {
                var checks = ResolveAllHealthChecks(provider, health).ToList();

                if (!health.Options.Enabled || !checks.Any())
                {
                    return new NoOpHealthCheckRunner();
                }

                return new DefaultHealthCheckRunner(checks);
            });

            AppMetricsHealthServiceCollectionExtensions.AddCoreServices(services, health);

            return health;
        }

        private static IEnumerable<HealthCheck> ResolveAllHealthChecks(IServiceProvider provider, IHealth health)
        {
            if (_hasResolvedChecks)
            {
                return _resolvedChecks;
            }

            _resolvedChecks = provider.GetRequiredService<IEnumerable<HealthCheck>>();
            var result = health.Checks.ToList();

            var existingNames = result.Select(c => c.Name).ToList();

            foreach (var check in _resolvedChecks)
            {
                if (existingNames.Contains(check.Name))
                {
                    throw new InvalidOperationException($"Health check names should be unique, found more than one health check named `{check.Name}`");
                }

                result.Add(check);
            }

            _hasResolvedChecks = true;

            return result;
        }
    }
}
