namespace TryAtSoftware.CleanTests.Core.XUnit.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit.Abstractions;

public class SerializableInitializationUtility : IXunitSerializable
{
    public IInitializationUtility? InitializationUtility { get; private set; }

    public SerializableInitializationUtility()
    {
    }

    public SerializableInitializationUtility(IInitializationUtility initializationUtility)
    {
        this.InitializationUtility = initializationUtility ?? throw new ArgumentNullException(nameof(initializationUtility));
    }

    /// <inheritdoc />
    public void Deserialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));
            
        var initializationCategory = info.GetValue<string>("c");
        var idValue = info.GetValue<string>("id");
        var id = Guid.Parse(idValue);

        var deserializedUtilityType = info.GetValue<Type>("ut");
        var utilityName = info.GetValue<string>("un");
        var isGlobal = info.GetValue<bool>("ig");
        
        var characteristics = info.GetValue<string[]>("ch");

        var deserializedGlobalDemands = info.GetValue<SerializableDemand[]>("gd");
        var deserializedLocalDemands = info.GetValue<SerializableDemand[]>("ld");

        var deserializedRequirements = info.GetValue<string[]>("r");
        var requirements = new HashSet<string>();
        foreach (var requirement in deserializedRequirements) requirements.Add(requirement);

        this.InitializationUtility = new InitializationUtility(initializationCategory, id, deserializedUtilityType, utilityName, isGlobal, characteristics, requirements);
        DeserializeDemands(deserializedGlobalDemands, this.InitializationUtility.GlobalDemands);
        DeserializeDemands(deserializedLocalDemands, this.InitializationUtility.LocalDemands);
    }

    /// <inheritdoc />
    public void Serialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));
        if (this.InitializationUtility is null) return;

        info.AddValue("c", this.InitializationUtility.Category);
        info.AddValue("id", this.InitializationUtility.Id.ToString());
        info.AddValue("ut", this.InitializationUtility.Type);
        info.AddValue("un", this.InitializationUtility.DisplayName);
        info.AddValue("ig", this.InitializationUtility.IsGlobal);

        info.AddValue("gd", Serialize(this.InitializationUtility.GlobalDemands));
        info.AddValue("ld", Serialize(this.InitializationUtility.LocalDemands));

        var characteristics = this.InitializationUtility.Characteristics.ToArray();
        info.AddValue("ch", characteristics);

        var requirements = this.InitializationUtility.Requirements.ToArray();
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
        foreach (var deserializedDemand in deserializedGlobalDemands)
        {
            if (!string.IsNullOrWhiteSpace(deserializedDemand.InitializationCategory) && !string.IsNullOrWhiteSpace(deserializedDemand.Demand))
                demandsCollection.Register(deserializedDemand.InitializationCategory, deserializedDemand.Demand);
        }
    }
}