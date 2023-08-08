namespace TryAtSoftware.CleanTests.Core.Extensions;

using TryAtSoftware.CleanTests.Core.Construction;

public static class ConstructionGraphExtensions
{
    public static FullCleanUtilityConstructionGraph Copy(this FullCleanUtilityConstructionGraph graph)
    {
        var graphCopy = new FullCleanUtilityConstructionGraph(graph.Id);
        foreach (var descriptor in graph.ConstructionDescriptors)
        {
            var descriptorCopy = new FullCleanUtilityConstructionGraph[descriptor.Length];
            for (var i = 0; i < descriptor.Length; i++) descriptorCopy[i] = descriptor[i].Copy();

            graphCopy.ConstructionDescriptors.Add(descriptorCopy);
        }

        return graphCopy;
    }
}