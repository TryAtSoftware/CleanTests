namespace TryAtSoftware.CleanTests.Core.XUnit.Serialization;

using System;
using TryAtSoftware.CleanTests.Core.Extensions;
using Xunit.Abstractions;

internal class SerializableDemand : IXunitSerializable
{
    private string? _initializationCategory;
    private string? _demand;

    public string InitializationCategory
    {
        get
        {
            this._initializationCategory.ValidateInstantiated("initialization category");
            return this._initializationCategory;
        }
        private set => this._initializationCategory = value;
    }

    public string Demand
    {
        get
        {
            this._demand.ValidateInstantiated("demand");
            return this._demand;
        }
        private set => this._demand = value;
    }

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