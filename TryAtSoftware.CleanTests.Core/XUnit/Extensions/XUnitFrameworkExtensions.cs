namespace TryAtSoftware.CleanTests.Core.XUnit.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

    public static (ICleanUtilityDescriptor InitializationUtility, Type ImplementationType) Materialize(this IndividualCleanUtilityDependencyNode dependencyNode, IDictionary<string, ICleanUtilityDescriptor> cleanUtilitiesById, IDictionary<Type, Type> genericTypesMap)
    {
        var initializationUtility = cleanUtilitiesById[dependencyNode.Id];

        var genericTypesSetup = initializationUtility.Type.ExtractGenericParametersSetup(genericTypesMap);
        var implementationType = initializationUtility.Type.MakeGenericType(genericTypesSetup);

        return (initializationUtility, implementationType);
    }

    public static string GetUniqueId(this IndividualCleanUtilityDependencyNode node)
    {
        var value = node.Id;
        if (node.Dependencies.Count == 0) return value;
        return $"{value} ({string.Join(", ", node.Dependencies.Select(x => x.GetUniqueId()))})";
    }
}