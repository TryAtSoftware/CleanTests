namespace TryAtSoftware.CleanTests.Core.XUnit.Serialization;

using System;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Dependencies;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.Extensions.Collections;
using Xunit.Abstractions;

public class SerializableIndividualDependencyNode : IXunitSerializable
{
    private IndividualCleanUtilityConstructionGraph? _dependencyNode;

    public IndividualCleanUtilityConstructionGraph ConstructionGraph
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

    public SerializableIndividualDependencyNode(IndividualCleanUtilityConstructionGraph constructionGraph)
    {
        this.ConstructionGraph = constructionGraph ?? throw new ArgumentNullException(nameof(constructionGraph));
    }

    /// <inheritdoc />
    public void Deserialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        var id = info.GetValue<string>("id");
        this.ConstructionGraph = new IndividualCleanUtilityConstructionGraph(id);

        var deserializedDependencies = info.GetValue<SerializableIndividualDependencyNode[]>("d");
        foreach (var dependency in deserializedDependencies.OrEmptyIfNull().Select(x => x?.ConstructionGraph).IgnoreNullValues())
            this.ConstructionGraph.Dependencies.Add(dependency);
    }

    /// <inheritdoc />
    public void Serialize(IXunitSerializationInfo info)
    {
        if (info is null) throw new ArgumentNullException(nameof(info));

        info.AddValue("id", this.ConstructionGraph.Id);

        var serializableDependencies = this.ConstructionGraph.Dependencies.Select(x => new SerializableIndividualDependencyNode(x)).ToArray();
        info.AddValue("d", serializableDependencies);
    }
}