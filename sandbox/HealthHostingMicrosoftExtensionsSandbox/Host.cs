// <copyright file="Host.cs" company="App Metrics Contributors">
// Copyright (c) App Metrics Contributors. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics.Health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using static System.Console;

namespace HealthHostingMicrosoftExtensionsSandbox
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
                .ConfigureMetrics()
                .ConfigureHealthWithDefaults(
                    (context, services, builder) =>
                    {
                        builder.OutputHealth.AsPlainText()
                               .OutputHealth.AsJson()
                               .HealthChecks.AddCheck("inline-check", () => new ValueTask<HealthCheckResult>(HealthCheckResult.Healthy()));
                    })
                .Build();

            var health = host.Services.GetRequiredService<IHealthRoot>();
            var cancellationTokenSource = new CancellationTokenSource();

            var healthStatus = await health.HealthCheckRunner.ReadAsync(cancellationTokenSource.Token);

            foreach (var formatter in health.OutputHealthFormatters)
            {
                WriteLine($"Formatter: {formatter.GetType().FullName}");
                WriteLine("-------------------------------------------");

                using (var stream = new MemoryStream())
                {
                    await formatter.WriteAsync(stream, healthStatus, cancellationTokenSource.Token);
                    var result = Encoding.UTF8.GetString(stream.ToArray());
                    WriteLine(result);
                }
            }

            WriteLine("Press any key to continue..");
            ReadKey();

            await host.RunAsync(token: cancellationTokenSource.Token);
        }
    }
}