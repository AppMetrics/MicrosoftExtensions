// <copyright file="HealthMetricsHealthBuilderExtensions.cs" company="App Metrics Contributors">
// Copyright (c) App Metrics Contributors. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable CheckNamespace
namespace App.Metrics.Health
// ReSharper restore CheckNamespace
{
    public static class HealthMetricsHealthBuilderExtensions
    {
        public static IHealthBuilder RecordResultsAsMetrics(
            this IHealthBuilder healthBuilder,
            IServiceCollection services,
            TimeSpan checkInterval)
        {
            services.AddHealthResultsAsMetricsHostedService(checkInterval);

            return healthBuilder;
        }
    }
}