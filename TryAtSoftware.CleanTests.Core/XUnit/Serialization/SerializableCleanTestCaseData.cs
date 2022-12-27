namespace TryAtSoftware.CleanTests.Core.XUnit.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.XUnit.Data;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;

public class SerializableCleanTestCaseData : IXunitSerializable
{
    public CleanTestCaseData? CleanTestData { get; private set; }

    public SerializableCleanTestCaseData()
    {
    }
    
    public SerializableCleanTestCaseData(CleanTestCaseData cleanTestData)
    {
        this.CleanTestData = cleanTestData ?? throw new ArgumentNullException(nameof(cleanTestData));
    }

    /// <inheritdoc />
    public void Deserialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        var deserializedGenericTypes = info.GetValue<Type[][]>("gt");
        var genericTypesMap = new Dictionary<Type, Type>();
        foreach (var genericTypesArray in deserializedGenericTypes) genericTypesMap[genericTypesArray[0]] = genericTypesArray[1];

        var deserializedInitializationUtilities = info.GetValue<SerializableIndividualDependencyNode[]>("iu");
        var initializationUtilities = deserializedInitializationUtilities.OrEmptyIfNull().Select(x => x?.DependencyNode).IgnoreNullValues();
        this.CleanTestData = new CleanTestCaseData(genericTypesMap, initializationUtilities);
    }

    /// <inheritdoc />
    public void Serialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));
        if (this.CleanTestData is null) return;

        var genericTypes = this.CleanTestData.GenericTypesMap.Select(kvp => new[] { kvp.Key, kvp.Value }).ToArray();
        info.AddValue("gt", genericTypes);

        var serializableDependencyNodes = this.CleanTestData.InitializationUtilities.Select(x => new SerializableIndividualDependencyNode(x)).ToArray();
        info.AddValue("iu", serializableDependencyNodes);
    }
}