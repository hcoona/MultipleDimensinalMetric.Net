using System;
using System.IO;
using System.Threading.Tasks;
using MultipleDimensionalMetric.CentralizedProcessing;
using MultipleDimensionalMetric.Formatters;
using MultipleDimensionalMetric.MetricWriterProcessors;
using Xunit;

namespace MultipleDimensionalMetric.Tests
{
    public class LineProtocol_UnitTest
    {
        [Fact]
        public async Task TestBaselineAsync()
        {
            var epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
            var stringWriter = new StringWriter();

            var formatter = new LineProtocolFormatter<DateTimeOffset>(t => (t - epoch).Ticks * 100);
            var processor = new TextWriterProcessor<string>(stringWriter);

            var metricProvider = new MetricProvider<DateTimeOffset, string>(
                () => DateTimeOffset.Parse("2016-06-13T17:43:50.1004002Z"),
                formatter.Format,
                formatter.Format,
                processor.Process);

            var temperatureMetric = metricProvider.CreateInt64Metric("weather", "temperature", new[] { "location" });
            temperatureMetric.Update(82, new[] { "us-midwest" });

            while (metricProvider.metricQueue.Count != 0)
            {
                await Task.Delay(50);
            }

            Assert.Equal(
                "weather,location=us-midwest temperature=82 1465839830100400200",
                stringWriter.ToString().Trim());
        }
    }
}
