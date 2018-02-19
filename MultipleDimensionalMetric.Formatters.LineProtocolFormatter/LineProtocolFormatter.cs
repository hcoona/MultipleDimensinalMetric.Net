using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultipleDimensionalMetric.Formatters
{
    public class LineProtocolFormatter<TTimestamp>
    {
        public delegate long NanosecondPrecisionUnixTimeFormatter(TTimestamp timestamp);

        private readonly NanosecondPrecisionUnixTimeFormatter timeFormatter;

        public LineProtocolFormatter(NanosecondPrecisionUnixTimeFormatter timeFormatter)
        {
            this.timeFormatter = timeFormatter;
        }

        public int Format<T>(
            TTimestamp timestamp,
            string metricNamespace,
            string metricName,
            IEnumerable<string> metricDimensionNames,
            IEnumerable<string> metricDimensionValues,
            T metricRawValue,
            out string queueItem)
        {
            var stringBuilder = new StringBuilder();

            // Measurement
            stringBuilder.Append(metricNamespace);

            stringBuilder.Append(",");

            // Tag set
            stringBuilder.Append(string.Join(",", metricDimensionNames.Zip(metricDimensionValues, (name, value) => $"{name}={value}")));

            // Whitespace I
            stringBuilder.Append(" ");

            // Field set
            stringBuilder.Append($"{metricName}={metricRawValue}");

            // Whitespace II
            stringBuilder.Append(" ");

            // Timestamp
            try
            {
                stringBuilder.Append(timeFormatter.Invoke(timestamp));
            }
            catch
            {
                queueItem = string.Empty;
                return -10;
            }

            queueItem = stringBuilder.ToString();
            return 0;
        }
    }
}
