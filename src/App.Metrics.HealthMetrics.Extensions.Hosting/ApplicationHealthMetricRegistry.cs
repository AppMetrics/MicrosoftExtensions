// <copyright file="ApplicationHealthMetricRegistry.cs" company="App Metrics Contributors">
// Copyright (c) App Metrics Contributors. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using App.Metrics.Gauge;

namespace App.Metrics.Health.Extensions.Hosting
{
    [ExcludeFromCodeCoverage]
    public static class ApplicationHealthMetricRegistry
    {
        // ReSharper disable MemberCanBePrivate.Global
        public static readonly string Context = "Application.Health";
        // ReSharper restore MemberCanBePrivate.Global

        public static GaugeOptions Checks => new GaugeOptions
                                             {
                                                 Context = Context,
                                                 Name = "Results",
                                                 MeasurementUnit = Unit.Items
                                             };

        public static GaugeOptions HealthGauge => new GaugeOptions
                                                  {
                                                      Context = Context,
                                                      Name = "Score",
                                                      MeasurementUnit = Unit.Custom("Health Score")
                                                  };
    }
}