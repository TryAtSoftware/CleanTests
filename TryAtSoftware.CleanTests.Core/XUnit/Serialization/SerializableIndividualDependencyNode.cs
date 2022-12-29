namespace TryAtSoftware.CleanTests.Core.XUnit.Serialization;

using System;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;

public class SerializableIndividualDependencyNode : IXunitSerializable
{
    private IndividualInitializationUtilityDependencyNode? _dependencyNode;

    public IndividualInitializationUtilityDependencyNode DependencyNode
    {
        get
        {
            this._dependencyNode.ValidateInstantiated("individual initialization utility dependency node");
            return this._dependencyNode;
        }
        private set => this._dependencyNode = value;
    }

    public SerializableIndividualDependencyNode()
    {
    }

    public SerializableIndividualDependencyNode(IndividualInitializationUtilityDependencyNode dependencyNode)
    {
        this.DependencyNode = dependencyNode ?? throw new ArgumentNullException(nameof(dependencyNode));
    }

    /// <inheritdoc />
    public void Deserialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        var id = Guid.Parse(info.GetValue<string>("id"));
        this.DependencyNode = new IndividualInitializationUtilityDependencyNode(id);

        var deserializedDependencies = info.GetValue<SerializableIndividualDependencyNode[]>("d");
        foreach (var dependency in deserializedDependencies.OrEmptyIfNull().Select(x => x?.DependencyNode).IgnoreNullValues())
            this.DependencyNode.Dependencies.Add(dependency);
    }

    /// <inheritdoc />
    public void Serialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        info.AddValue("id", this.DependencyNode.Id.ToString());

        var serializableDependencies = this.DependencyNode.Dependencies.Select(x => new SerializableIndividualDependencyNode(x)).ToArray();
        info.AddValue("d", serializableDependencies);
    }
}