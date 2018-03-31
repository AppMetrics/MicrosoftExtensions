// <copyright file="Host.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Extensions.Hosting;
using App.Metrics.Scheduling;
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
            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Verbose()
                         .WriteTo.LiterateConsole(LogEventLevel.Information)
                         .WriteTo.Seq("http://localhost:5341", LogEventLevel.Verbose)
                         .CreateLogger();

            var host = new HostBuilder()
                .ConfigureAppConfiguration(
                    (hostContext, config) =>
                    {
                        config.SetBasePath(Directory.GetCurrentDirectory());
                        config.AddEnvironmentVariables();
                        config.AddJsonFile("appsettings.json", optional: true);
                        config.AddCommandLine(args);
                    })
                 .ConfigureMetrics(
                    (context, builder) =>
                    {
                        builder.Report.Using<SimpleConsoleMetricsReporter>(TimeSpan.FromSeconds(5));
                    })
                .Build();

            var metrics = host.Services.GetRequiredService<IMetricsRoot>();

            var cancellationTokenSource = new CancellationTokenSource();

            await host.WriteEnvAsync(cancellationTokenSource, metrics);

            host.PressAnyKeyToContinue();

            await host.WriteMetricsAsync(metrics, cancellationTokenSource);

            host.PressAnyKeyToContinue();

            var recordMetricsTask = new AppMetricsTaskScheduler(
                TimeSpan.FromSeconds(2),
                () =>
                {
                    Clear();
                    host.RecordMetrics(metrics);
                    return Task.CompletedTask;
                });

            recordMetricsTask.Start();

            await host.RunAsync(token: cancellationTokenSource.Token);
        }
    }
}