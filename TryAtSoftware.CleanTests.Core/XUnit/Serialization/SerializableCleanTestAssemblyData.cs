namespace TryAtSoftware.CleanTests.Core.XUnit.Serialization;

using System;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;

public class SerializableCleanTestAssemblyData : IXunitSerializable
{
    private CleanTestAssemblyData? _cleanTestData;

    public CleanTestAssemblyData CleanTestData
    {
        get
        {
            this._cleanTestData.ValidateInstantiated("clean test assembly data");
            return this._cleanTestData;
        }
        private set => this._cleanTestData = value;
    }

    public SerializableCleanTestAssemblyData()
    {
    }
    
    public SerializableCleanTestAssemblyData(CleanTestAssemblyData cleanTestData)
    {
        this.CleanTestData = cleanTestData ?? throw new ArgumentNullException(nameof(cleanTestData));
    }

    /// <inheritdoc />
    public void Deserialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        var deserializedInitializationUtilities = info.GetValue<SerializableInitializationUtility[]>("iu");

        var initializationUtilities = deserializedInitializationUtilities.OrEmptyIfNull().Select(x => x?.CleanUtilityDescriptor).IgnoreNullValues();
        this.CleanTestData = new CleanTestAssemblyData(initializationUtilities);
    }

    /// <inheritdoc />
    public void Serialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        info.AddValue("iu", this.CleanTestData.InitializationUtilities.GetAllValues().Select(x => new SerializableInitializationUtility(x)).ToArray());
    }
}