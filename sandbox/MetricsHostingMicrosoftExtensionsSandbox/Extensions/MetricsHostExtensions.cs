// <copyright file="MetricsHostExtensions.cs" company="Allan Hardy">
// Copyright (c) Allan Hardy. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using MetricsHostingMicrosoftExtensionsSandbox;
using static System.Console;

// ReSharper disable CheckNamespace
namespace Microsoft.Extensions.Hosting
    // ReSharper restore CheckNamespace
{
    public static class MetricsHostExtensions
    {
        private static readonly Random Rnd = new Random();

        public static void RecordMetrics(this IHost host, IMetricsRoot metrics)
        {
            metrics.Measure.Counter.Increment(ApplicationsMetricsRegistry.CounterOne);
            metrics.Measure.Gauge.SetValue(ApplicationsMetricsRegistry.GaugeOne, Rnd.Next(0, 100));
            metrics.Measure.Histogram.Update(ApplicationsMetricsRegistry.HistogramOne, Rnd.Next(0, 100));
            metrics.Measure.Meter.Mark(ApplicationsMetricsRegistry.MeterOne, Rnd.Next(0, 100));

            using (metrics.Measure.Timer.Time(ApplicationsMetricsRegistry.TimerOne))
            {
                Thread.Sleep(Rnd.Next(0, 100));
            }

            using (metrics.Measure.Apdex.Track(ApplicationsMetricsRegistry.ApdexOne))
            {
                Thread.Sleep(Rnd.Next(0, 100));
            }
        }

        public static async Task WriteEnvAsync(this IHost host, CancellationTokenSource cancellationTokenSource, IMetricsRoot metrics)
        {
            WriteLine("Environment Information");
            WriteLine("-------------------------------------------");

            foreach (var formatter in metrics.OutputEnvFormatters)
            {
                WriteLine($"Formatter: {formatter.GetType().FullName}");
                WriteLine("-------------------------------------------");

                using (var stream = new MemoryStream())
                {
                    await formatter.WriteAsync(stream, metrics.EnvironmentInfo, cancellationTokenSource.Token);

                    var result = Encoding.UTF8.GetString(stream.ToArray());

                    WriteLine(result);
                }
            }
        }

        public static async Task WriteMetricsAsync(this IHost host, IMetricsRoot metrics, CancellationTokenSource cancellationTokenSource)
        {
            foreach (var unused in Enumerable.Range(0, 10))
            {
                host.RecordMetrics(metrics);
            }

            var metricsData = metrics.Snapshot.Get();

            WriteLine("Metrics Formatters");
            WriteLine("-------------------------------------------");

            foreach (var formatter in metrics.OutputMetricsFormatters)
            {
                WriteLine($"Formatter: {formatter.GetType().FullName}");
                WriteLine("-------------------------------------------");

                using (var stream = new MemoryStream())
                {
                    await formatter.WriteAsync(stream, metricsData, cancellationTokenSource.Token);

                    var result = Encoding.UTF8.GetString(stream.ToArray());

                    WriteLine(result);
                }
            }
        }
    }
}