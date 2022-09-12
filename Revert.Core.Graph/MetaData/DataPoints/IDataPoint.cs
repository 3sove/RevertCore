using ProtoBuf;

namespace Revert.Core.Graph.MetaData.DataPoints
{
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public interface IDataPoint<TKey, TValue> : IDataPoint
    {
        TKey Key { get; set; }
        TValue Value { get; set; }
    }

    
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public interface IDataPoint
    {
        string Summary { get; }
        bool IsSearchable { get; set; }
        bool IsResolvable { get; set; }
        object key { get; }
        object value { get; }
    }
}
