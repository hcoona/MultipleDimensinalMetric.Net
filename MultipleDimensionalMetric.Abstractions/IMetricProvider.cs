namespace MultipleDimensionalMetric.Abstractions
{
    public interface IMetricProvider
    {
        IMetric<long> CreateInt64Metric(
            string metricNamespace,
            string metricName,
            string[] metricDimensionNames);

        IMetric<double> CreateDoubleMetric(
            string metricNamespace,
            string metricName,
            string[] metricDimensionNames);
    }
}
