// <copyright file="ConfigurationMetricsOptionsConfigurationExtensions.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using App.Metrics.Tagging;
using Microsoft.Extensions.Configuration;

namespace App.Metrics.Extensions.Configuration
{
    /// <summary>
    /// Extends <see cref="ConfigurationMetricsOptionsConfigurationExtensions"/> with support for System.Configuration appSettings elements.
    /// </summary>
    public static class ConfigurationMetricsOptionsConfigurationExtensions
    {
        private const string DefaultSectionName = nameof(MetricsOptions);

        public static MetricsOptionsConfigurationBuilder Configure(
            this MetricsOptionsConfigurationBuilder builder,
            IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            builder.Configure(configuration.GetSection(DefaultSectionName));

            return builder;
        }

        public static MetricsOptionsConfigurationBuilder Configure(
            this MetricsOptionsConfigurationBuilder builder,
            IConfigurationSection configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var configGlobalTags = new GlobalMetricTags();
            configuration.Bind(nameof(MetricsOptions.GlobalTags), configGlobalTags);

            if (configGlobalTags?.Keys?.Any() != null)
            {
                // Keep the orginal global tags set but override with those set in config
                var originalTags = builder.Options.GlobalTags.ToDictionary(t => t.Key, t => t.Value);
                builder.Options.GlobalTags = new GlobalMetricTags();

                configuration.Bind(builder.Options);

                foreach (var tag in originalTags)
                {
                    if (!builder.Options.GlobalTags.ContainsKey(tag.Key))
                    {
                        builder.Options.GlobalTags.Add(tag.Key, tag.Value);
                    }
                }

                return builder;
            }

            configuration.Bind(builder.Options);

            return builder;
        }
    }
}
