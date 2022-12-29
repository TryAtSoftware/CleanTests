namespace TryAtSoftware.CleanTests.Core.XUnit.Serialization;

using System;
using Xunit.Abstractions;

public class SerializableDemand : IXunitSerializable
{
    public string InitializationCategory { get; private set; }
    public string Demand { get; private set; }

    public SerializableDemand()
    {
    }
    
    public SerializableDemand(string initializationCategory, string demand)
    {
        if (string.IsNullOrWhiteSpace(initializationCategory)) throw new ArgumentNullException(nameof(initializationCategory));
        if (string.IsNullOrWhiteSpace(demand)) throw new ArgumentNullException(nameof(demand));

        this.InitializationCategory = initializationCategory;
        this.Demand = demand;
    }

    /// <inheritdoc />
    public void Deserialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        this.InitializationCategory = info.GetValue<string>("c");
        this.Demand = info.GetValue<string>("d");
    }

    /// <inheritdoc />
    public void Serialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));
        
        info.AddValue("c", this.InitializationCategory);
        info.AddValue("d", this.Demand);
    }
}