using System.IO;
using System.Threading.Tasks;

namespace MultipleDimensionalMetric.MetricWriterProcessors
{
    public class TextWriterProcessor<TQueueItem>
    {
        private readonly TextWriter textWriter;

        public TextWriterProcessor(TextWriter textWriter)
        {
            this.textWriter = textWriter;
        }

        public Task Process(TQueueItem item)
        {
            return textWriter.WriteLineAsync(item.ToString());
        }
    }
}
