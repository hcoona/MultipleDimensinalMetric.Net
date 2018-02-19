using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using MultipleDimensionalMetric.Abstractions;

namespace MultipleDimensionalMetric.CentralizedProcessing
{
    public class Metric
    {
        public delegate TTimestamp TimestampFactory<TTimestamp>();

        public static readonly int ErrorCodeDimensionMismatch = -1;
        public static readonly int ErrorCodeInternalQueueError = -2;
    }

    public class Metric<TTimestamp, TMetric, TQueueItem> : IMetric<TTimestamp, TMetric>
    {
        public delegate int MetricFormatter(
            TTimestamp timestamp,
            string metricNamespace,
            string metricName,
            IEnumerable<string> metricDimensionNames,
            IEnumerable<string> metricDimensionValues,
            TMetric metricRawValue,
            out TQueueItem queueItem);

        private readonly string metricNamespace;
        private readonly string metricName;
        private readonly IReadOnlyList<string> metricDimensionNames;
        private readonly MetricFormatter metricFormatter;
        private readonly Metric.TimestampFactory<TTimestamp> timestampFactory;
        private readonly ITargetBlock<TQueueItem> metricQueue;

        public Metric(
            string metricNamespace, string metricName,
            string[] metricDimensionNames,
            MetricFormatter metricFormatter,
            Metric.TimestampFactory<TTimestamp> timestampFactory,
            ITargetBlock<TQueueItem> metricQueue)
        {
            this.metricNamespace = metricNamespace;
            this.metricName = metricName;
            this.metricDimensionNames = metricDimensionNames;
            this.metricFormatter = metricFormatter;
            this.timestampFactory = timestampFactory;
            this.metricQueue = metricQueue;
        }

        public int Update(TMetric rawValue, string[] dimensionValues) =>
            Update(timestampFactory.Invoke(), rawValue, dimensionValues);

        public int Update(TTimestamp timestamp, TMetric rawValue, string[] dimensionValues)
        {
            if (dimensionValues.Length != metricDimensionNames.Count)
            {
                return Metric.ErrorCodeDimensionMismatch;
            }

            var returnCode = metricFormatter.Invoke(
                timestamp,
                metricNamespace,
                metricName,
                metricDimensionNames,
                dimensionValues,
                rawValue,
                out var queueItem);
            if (returnCode == 0)
            {
                if (metricQueue.Post(queueItem))
                {
                    return 0;
                }
                else
                {
                    return Metric.ErrorCodeInternalQueueError;
                }
            }
            else
            {
                return returnCode;
            }
        }
    }
}
