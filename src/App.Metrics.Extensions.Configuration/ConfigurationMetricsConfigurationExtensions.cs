// <copyright file="ConfigurationMetricsConfigurationExtensions.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace App.Metrics.Extensions.Configuration
{
    /// <summary>
    /// Extends <see cref="ConfigurationMetricsConfigurationExtensions"/> with support for System.Configuration appSettings elements.
    /// </summary>
    public static class ConfigurationMetricsConfigurationExtensions
    {
        private const string DefaultSectionName = nameof(MetricsOptions);

        public static IMetricsConfigurationBuilder ReadFrom(
            this IMetricsConfigurationBuilder builder,
            IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            builder.ReadFrom(configuration.GetSection(DefaultSectionName));

            return builder;
        }

        public static IMetricsConfigurationBuilder ReadFrom(
            this IMetricsConfigurationBuilder builder,
            IConfigurationSection configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            builder.Extend(configuration.AsEnumerable());

            return builder;
        }
    }
}
