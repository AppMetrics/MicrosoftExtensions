// <copyright file="AppMetricsServiceCollectionExtensionsTests.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using App.Metrics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Metrics.Extensions.DependencyInjection.Facts
{
    public class AppMetricsServiceCollectionExtensionsTests
    {
        [Fact]
        public void Can_resolve_metrics_from_service_collection()
        {
            // Arrange
            var builder = AppMetrics.CreateDefaultBuilder();
            var services = new ServiceCollection();

            // Act
            services.AddMetrics(builder);

            // Assert
            var provider = services.BuildServiceProvider();
            provider.GetService<IMetrics>().Should().NotBeNull();
        }

        [Fact]
        public void Can_resolve_metrics_from_service_collection_when_pre_built()
        {
            // Arrange
            var metrics = AppMetrics.CreateDefaultBuilder().Build();
            var services = new ServiceCollection();

            // Act
            services.AddMetrics(metrics);

            // Assert
            var provider = services.BuildServiceProvider();
            provider.GetService<IMetricsRoot>().Should().NotBeNull();
        }

        [Fact]
        public void Can_resolve_metrics_from_service_collection_when_using_builder_setup_action()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMetrics(builder =>
            {
                builder.Configuration.Configure(options => options.Enabled = false)
                    .OutputEnvInfo.AsPlainText()
                    .OutputMetrics.AsPlainText();
            });

            // Assert
            var provider = services.BuildServiceProvider();
            provider.GetService<IMetrics>().Should().NotBeNull();
            provider.GetService<MetricsOptions>().Enabled.Should().BeFalse();
        }
    }
}
