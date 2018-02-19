using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MultipleDimensionalMetric.Abstractions;

namespace MultipleDimensionalMetric.CentralizedProcessing
{
    public class MetricProvider<TTimestamp, TQueueItem> : IMetricProvider, IDisposable
    {
        protected readonly Metric.TimestampFactory<TTimestamp> timestampFactory;
        protected readonly Metric<TTimestamp, double, TQueueItem>.MetricFormatter doubleMetricFormatter;
        protected readonly Metric<TTimestamp, long, TQueueItem>.MetricFormatter longMetricFormatter;
        protected readonly CancellationTokenSource cancellationTokenSource;
        internal readonly BufferBlock<TQueueItem> metricQueue;
        protected readonly MetricWriter<TQueueItem> metricWriter;

        public MetricProvider(
            Metric.TimestampFactory<TTimestamp> timestampFactory,
            Metric<TTimestamp, double, TQueueItem>.MetricFormatter doubleMetricFormatter,
            Metric<TTimestamp, long, TQueueItem>.MetricFormatter longMetricFormatter,
            Func<TQueueItem, Task> metricWriteProcessor)
        {
            this.timestampFactory = timestampFactory;
            this.doubleMetricFormatter = doubleMetricFormatter;
            this.longMetricFormatter = longMetricFormatter;

            cancellationTokenSource = new CancellationTokenSource();
            metricQueue = new BufferBlock<TQueueItem>(new DataflowBlockOptions
            {
                CancellationToken = cancellationTokenSource.Token
            });
            this.metricWriter = new MetricWriter<TQueueItem>(this.metricQueue, this.cancellationTokenSource.Token, metricWriteProcessor);
        }

        public virtual IMetric<double> CreateDoubleMetric(
            string metricNamespace, string metricName,
            string[] metricDimensionNames)
        {
            return new Metric<TTimestamp, double, TQueueItem>(
                metricNamespace, metricName,
                metricDimensionNames,
                doubleMetricFormatter,
                timestampFactory,
                metricQueue);
        }

        public virtual IMetric<long> CreateInt64Metric(
            string metricNamespace, string metricName,
            string[] metricDimensionNames)
        {
            return new Metric<TTimestamp, long, TQueueItem>(
                metricNamespace, metricName,
                metricDimensionNames,
                longMetricFormatter,
                timestampFactory,
                metricQueue);
        }

        public void Dispose()
        {
            metricQueue.Complete();
            cancellationTokenSource.Cancel();
            metricWriter.Dispose();
        }
    }
}
