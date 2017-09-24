// <copyright file="ServiceCollectionMetricsBuilderExtensions.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace App.Metrics.Extensions.DependencyInjection
{
    public static class ServiceCollectionMetricsBuilderExtensions
    {
        public static IMetricsRoot BuildAndAddTo(
            this IMetricsBuilder builder,
            IServiceCollection services)
        {
            var metrics = builder.Build();

            services.AddMetrics(metrics);

            return metrics;
        }
    }
}
