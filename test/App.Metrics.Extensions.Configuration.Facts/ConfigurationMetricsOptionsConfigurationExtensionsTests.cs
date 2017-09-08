// <copyright file="ConfigurationMetricsOptionsConfigurationExtensionsTests.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace App.Metrics.Extensions.Configuration.Facts
{
    public class ConfigurationMetricsOptionsConfigurationExtensionsTests
    {
        [Fact]
        public void Can_bind_metrics_options_from_configuration()
        {
            // Arrange
            var builder = new MetricsBuilder();
            var keyValuePairs = new Dictionary<string, string>
                                {
                                    { "MetricsOptions:DefaultContextLabel", "Testing" },
                                    { "MetricsOptions:GlobalTags", "tag1=value1, tag2=value2" },
                                    { "MetricsOptions:Enabled", "false" }
                                };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(keyValuePairs).Build();

            // Act
            builder.Configuration.Configure(configuration);
            var metrics = builder.Build();

            // Assert
            metrics.Options.DefaultContextLabel.Should().Be("Testing");
            metrics.Options.Enabled.Should().BeFalse();
            metrics.Options.GlobalTags.Count.Should().Be(2);
        }

        [Fact]
        public void Should_merge_global_tags_when_key_values_provided_that_match_an_existing_tag()
        {
            // Arrange
            var builder = new MetricsBuilder();
            var keyValuePairs = new Dictionary<string, string>
                                {
                                    { "MetricsOptions:GlobalTags", "tag1=replaced, tag2=added" }
                                };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(keyValuePairs).Build();
            var options = new MetricsOptions();
            options.GlobalTags.Add("tag1", "value1");

            // Act
            builder.Configuration.Configure(o => o.GlobalTags.Add("tag1", "value1"));
            builder.Configuration.Configure(configuration);
            var metrics = builder.Build();

            // Assert
            metrics.Options.GlobalTags.Count.Should().Be(2);
            metrics.Options.GlobalTags.First().Key.Should().Be("tag1");
            metrics.Options.GlobalTags.First().Value.Should().Be("replaced");
            metrics.Options.GlobalTags.Skip(1).First().Key.Should().Be("tag2");
            metrics.Options.GlobalTags.Skip(1).First().Value.Should().Be("added");
        }
    }
}
