namespace TryAtSoftware.CleanTests.Core.XUnit.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit.Abstractions;

public static class XUnitFrameworkExtensions
{
    public static bool TryGetSingleAttribute(this IDecoratedComponent? decoratedComponent, Type? attributeType, [NotNullWhen(true)] out IAttributeInfo? attribute)
    {
        attribute = null;
        if (decoratedComponent is null || attributeType is null)
            return false;

        var retrievedSingleAttribute = decoratedComponent.GetCustomAttributes(attributeType).OrEmptyIfNull().IgnoreNullValues().SingleOrDefault();
        if (retrievedSingleAttribute is null)
            return false;

        attribute = retrievedSingleAttribute;
        return true;
    }
    
    public static (ICleanUtilityDescriptor InitializationUtility, Type ImplementationType) Materialize(this IndividualInitializationUtilityDependencyNode dependencyNode, IDictionary<string, ICleanUtilityDescriptor> cleanUtilitiesById, IDictionary<Type, Type> genericTypesMap)
    {
        var initializationUtility = cleanUtilitiesById[dependencyNode.Id];

        var genericTypesSetup = initializationUtility.Type.ExtractGenericParametersSetup(genericTypesMap);
        var implementationType = initializationUtility.Type.MakeGenericType(genericTypesSetup);

        return (initializationUtility, implementationType);
    }

    public static string GetUniqueId(this IndividualInitializationUtilityDependencyNode dependencyNode, IDictionary<string, ICleanUtilityDescriptor> cleanUtilitiesById, IDictionary<Type, Type> genericTypesMap)
    {
        if (dependencyNode is null) throw new ArgumentNullException(nameof(dependencyNode));

        StringBuilder sb = new();
        Iterate(dependencyNode);
        return sb.ToString();

        void Iterate(IndividualInitializationUtilityDependencyNode node, string? id = null)
        {
            var isRoot = id is null;
            if (isRoot) sb.Append(node.Id);
            else sb.Append($"{id}:{node.Id}");

            var (_, implementationType) = node.Materialize(cleanUtilitiesById, genericTypesMap);
            sb.Append($",{TypeNames.Get(implementationType)}|");

            for (var i = 0; i < node.Dependencies.Count; i++)
            {
                var dependencyId = isRoot ? $"{i + 1}" : $"{id}.{i + 1}";
                Iterate(node.Dependencies[i], dependencyId);
            }
        }
    }
}