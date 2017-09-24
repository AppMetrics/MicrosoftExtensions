// <copyright file="ServiceCollectionHealthCheckBuilderExtensions.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using App.Metrics.Health;
using App.Metrics.Health.Extensions.DependencyInjection.Internal;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
    // ReSharper restore CheckNamespace
{
    public static class ServiceCollectionHealthCheckBuilderExtensions
    {
        /// <summary>
        ///     Scan the executing assembly and it's refererenced assemblies for health checks and automatically register them.
        /// </summary>
        /// <param name="healthCheckBuilder">The <see cref="IHealthBuilder" /> to add any found health checks.</param>
        /// <param name="services">The <see cref="IServiceCollection" /> where found health checks should be registered.</param>
        /// <param name="startupAssemblyName">The startup assemblies name.</param>
        /// <returns>
        ///     An <see cref="IHealthBuilder" /> that can be used to further configure App Metrics Health.
        /// </returns>
        public static IHealthBuilder RegisterFromAssembly(
            this IHealthCheckBuilder healthCheckBuilder,
            IServiceCollection services,
            string startupAssemblyName)
        {
            HealthChecksAsServices.AddHealthChecksAsServices(services, HealthAssemblyDiscoveryProvider.DiscoverAssemblies(startupAssemblyName));

            return healthCheckBuilder.Builder;
        }
    }
}