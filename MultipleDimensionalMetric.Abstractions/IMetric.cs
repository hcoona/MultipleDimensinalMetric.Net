namespace MultipleDimensionalMetric.Abstractions
{
    public interface IMetric<TValue>
    {
        int Update(TValue rawValue, string[] dimensionValues);
    }

    public interface IMetric<TTimestamp, TValue> : IMetric<TValue>
    {
        int Update(TTimestamp timestamp, TValue rawValue, string[] dimensionValues);
    }
}
