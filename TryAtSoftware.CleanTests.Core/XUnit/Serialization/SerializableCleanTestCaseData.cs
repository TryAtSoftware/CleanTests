namespace TryAtSoftware.CleanTests.Core.XUnit.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;

public class SerializableCleanTestCaseData : IXunitSerializable
{
    private CleanTestCaseData? _cleanTestData;

    public CleanTestCaseData CleanTestData
    {
        get
        {
            this._cleanTestData.ValidateInstantiated("clean test case data");
            return this._cleanTestData;
        }
        private set => this._cleanTestData = value;
    }

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
        var displayNamePrefix = info.GetValue<string?>("dnp");
        
        this.CleanTestData = new CleanTestCaseData(genericTypesMap, initializationUtilities, displayNamePrefix);
    }

    /// <inheritdoc />
    public void Serialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        var genericTypes = this.CleanTestData.GenericTypesMap.Select(kvp => new[] { kvp.Key, kvp.Value }).ToArray();
        info.AddValue("gt", genericTypes);

        var serializableDependencyNodes = this.CleanTestData.CleanUtilities.Select(x => new SerializableIndividualDependencyNode(x)).ToArray();
        info.AddValue("iu", serializableDependencyNodes);

        info.AddValue("dnp", this.CleanTestData.DisplayNamePrefix);
    }
}