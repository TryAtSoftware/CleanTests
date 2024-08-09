namespace TryAtSoftware.CleanTests.Core.XUnit.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using Xunit.Abstractions;

internal class SerializableInitializationUtility : IXunitSerializable
{
    private ICleanUtilityDescriptor? _initializationUtility;

    public ICleanUtilityDescriptor CleanUtilityDescriptor
    {
        get
        {
            this._initializationUtility.ValidateInstantiated("initialization utility");
            return this._initializationUtility;
        }
        private set => this._initializationUtility = value;
    }

    public SerializableInitializationUtility()
    {
    }

    public SerializableInitializationUtility(ICleanUtilityDescriptor cleanUtilityDescriptor)
    {
        this.CleanUtilityDescriptor = cleanUtilityDescriptor ?? throw new ArgumentNullException(nameof(cleanUtilityDescriptor));
    }

    /// <inheritdoc />
    public void Deserialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));
            
        var initializationCategory = info.GetValue<string>("c");

        var deserializedUtilityType = info.GetValue<Type>("ut");
        var utilityName = info.GetValue<string>("un");
        var isGlobal = info.GetValue<bool>("ig");
        
        var characteristics = info.GetValue<string[]>("ch");

        var deserializedExternalDemands = info.GetValue<SerializableDemand[]>("gd");
        var deserializedInternalDemands = info.GetValue<SerializableDemand[]>("ld");
        var deserializedOuterDemands = info.GetValue<SerializableDemand[]>("od");

        var deserializedRequirements = info.GetValue<string[]>("r");
        var requirements = new HashSet<string>();
        foreach (var requirement in deserializedRequirements) requirements.Add(requirement);

        this.CleanUtilityDescriptor = new CleanUtilityDescriptor(initializationCategory, deserializedUtilityType, utilityName, isGlobal, characteristics, requirements);
        DeserializeDemands(deserializedExternalDemands, this.CleanUtilityDescriptor.ExternalDemands);
        DeserializeDemands(deserializedInternalDemands, this.CleanUtilityDescriptor.InternalDemands);
        DeserializeDemands(deserializedOuterDemands, this.CleanUtilityDescriptor.OuterDemands);
    }

    /// <inheritdoc />
    public void Serialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        info.AddValue("c", this.CleanUtilityDescriptor.Category);
        info.AddValue("id", this.CleanUtilityDescriptor.Id);
        info.AddValue("ut", this.CleanUtilityDescriptor.Type);
        info.AddValue("un", this.CleanUtilityDescriptor.Name);
        info.AddValue("ig", this.CleanUtilityDescriptor.IsGlobal);

        info.AddValue("gd", Serialize(this.CleanUtilityDescriptor.ExternalDemands));
        info.AddValue("ld", Serialize(this.CleanUtilityDescriptor.InternalDemands));
        info.AddValue("od", Serialize(this.CleanUtilityDescriptor.OuterDemands));

        var characteristics = this.CleanUtilityDescriptor.Characteristics.ToArray();
        info.AddValue("ch", characteristics);

        var requirements = this.CleanUtilityDescriptor.InternalRequirements.ToArray();
        info.AddValue("r", requirements);
    }

    private static SerializableDemand[] Serialize(ICleanTestInitializationCollection<string> demands)
    {
        if (demands is null) throw new ArgumentNullException(nameof(demands));
            
        var serializableDemands = new List<SerializableDemand>();
        foreach (var (category, categoryDemands) in demands) serializableDemands.AddRange(categoryDemands.Select(categoryDemand => new SerializableDemand(category, categoryDemand)));
        return serializableDemands.ToArray();
    }

    private static void DeserializeDemands(IEnumerable<SerializableDemand> deserializedGlobalDemands, ICleanTestInitializationCollection<string> demandsCollection)
    {
        if (deserializedGlobalDemands is null) throw new ArgumentNullException(nameof(deserializedGlobalDemands));
        if (demandsCollection is null) throw new ArgumentNullException(nameof(demandsCollection));
        foreach (var deserializedDemand in deserializedGlobalDemands) demandsCollection.Register(deserializedDemand.InitializationCategory, deserializedDemand.Demand);
    }
}