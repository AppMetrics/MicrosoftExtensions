// <copyright file="Host.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Extensions.Configuration;
using App.Metrics.Extensions.Hosting;
using App.Metrics.Reporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using static System.Console;

namespace MetricsHostingMicrosoftExtensionsSandbox
{
    public class Host
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.LiterateConsole(LogEventLevel.Information).WriteTo.
                                                   Seq("http://localhost:5341", LogEventLevel.Verbose).CreateLogger();

            var host = new HostBuilder().ConfigureAppConfiguration(
                (hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddEnvironmentVariables();
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddCommandLine(args);
                }).ConfigureMetricsWithDefaults(
                (context, builder) =>
                {
                    builder.Configuration.ReadFrom(context.Configuration);
                    builder.OutputEnvInfo.AsPlainText();
                    builder.OutputMetrics.AsPlainText();
                    builder.OutputMetrics.AsJson();
                    builder.Report.Using<SimpleConsoleMetricsReporter>(TimeSpan.FromSeconds(5));
                }).Build();

            var metrics = host.Services.GetRequiredService<IMetricsRoot>();
            var reporter = host.Services.GetRequiredService<IRunMetricsReports>();

            var cancellationTokenSource = new CancellationTokenSource();

            await host.WriteEnvAsync(cancellationTokenSource, metrics);

            host.PressAnyKeyToContinue();

            await host.WriteMetricsAsync(metrics, cancellationTokenSource);

            host.PressAnyKeyToContinue();

            await host.RunUntilEscAsync(
                TimeSpan.FromSeconds(5),
                cancellationTokenSource,
                async () =>
                {
                    Clear();
                    host.RecordMetrics(metrics);
                    await Task.WhenAll(reporter.RunAllAsync(cancellationTokenSource.Token));
                });

            await host.RunAsync(token: cancellationTokenSource.Token);
        }
    }
}