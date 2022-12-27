namespace TryAtSoftware.CleanTests.Core.XUnit.Serialization;

using System;
using System.Linq;
using TryAtSoftware.CleanTests.Core.XUnit.Data;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;

public class SerializableCleanTestAssemblyData : IXunitSerializable
{
    public CleanTestAssemblyData? CleanTestData { get; private set; }

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

        var initializationUtilities = deserializedInitializationUtilities.OrEmptyIfNull().Select(x => x?.InitializationUtility).IgnoreNullValues();
        this.CleanTestData = new CleanTestAssemblyData(initializationUtilities);
    }

    /// <inheritdoc />
    public void Serialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));
        if (this.CleanTestData is null) return;

        info.AddValue("iu", this.CleanTestData.InitializationUtilities.GetAllValues().Select(x => new SerializableInitializationUtility(x)).ToArray());
    }
}