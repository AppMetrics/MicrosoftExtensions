// <copyright file="ConfigurationMetricsConfigurationExtensions.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
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

        public static IMetricsConfigurationBuilder Configure(
            this IMetricsConfigurationBuilder builder,
            IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            builder.Configure(configuration.GetSection(DefaultSectionName));

            return builder;
        }

        public static IMetricsConfigurationBuilder Configure(
            this IMetricsConfigurationBuilder builder,
            IConfigurationSection configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var keyValuePairs = new Dictionary<string, string>();

            configuration.Bind(keyValuePairs);
            builder.Configure(keyValuePairs.ToDictionary(k => $"{DefaultSectionName}:{k.Key}", k => k.Value));

            return builder;
        }
    }
}
