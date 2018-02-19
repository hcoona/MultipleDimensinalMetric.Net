using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace MultipleDimensionalMetric.CentralizedProcessing
{
    public class MetricWriter<TQueueItem> : IDisposable
    {
        private readonly ISourceBlock<TQueueItem> metricQueue;
        private readonly CancellationToken cancellationToken;
        private readonly Func<TQueueItem, Task> metricWriteProcessor;
        private readonly Task backgroundTask;

        public MetricWriter(
            ISourceBlock<TQueueItem> metricQueue,
            CancellationToken cancellationToken,
            Func<TQueueItem, Task> metricWriteProcessor)
        {
            this.metricQueue = metricQueue;
            this.cancellationToken = cancellationToken;
            this.metricWriteProcessor = metricWriteProcessor;

            backgroundTask = Task.Run(() => BackgroundLoopMain(cancellationToken));
        }

        protected virtual async Task BackgroundLoopMain(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested
                && await metricQueue.OutputAvailableAsync(cancellationToken))
            {
                await metricWriteProcessor.Invoke(await metricQueue.ReceiveAsync(cancellationToken));
            }
        }

        public void Dispose()
        {
            metricQueue.Complete();
            backgroundTask.Dispose();
        }
    }
}
