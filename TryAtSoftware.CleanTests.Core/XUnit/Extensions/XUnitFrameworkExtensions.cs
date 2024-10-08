﻿namespace TryAtSoftware.CleanTests.Core.XUnit.Extensions;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Construction;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;
using TryAtSoftware.Extensions.Reflection;
using Xunit.Abstractions;

internal static class XUnitFrameworkExtensions
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

    public static (ICleanUtilityDescriptor InitializationUtility, Type ImplementationType) Materialize(this IndividualCleanUtilityConstructionGraph constructionGraph, IDictionary<string, ICleanUtilityDescriptor> cleanUtilitiesById, IDictionary<Type, Type> genericTypesMap)
    {
        var initializationUtility = cleanUtilitiesById[constructionGraph.Id];

        var genericTypesSetup = initializationUtility.Type.ExtractGenericParametersSetup(genericTypesMap);
        var implementationType = initializationUtility.Type.MakeGenericType(genericTypesSetup);

        return (initializationUtility, implementationType);
    }

    public static string GetUniqueId(this IndividualCleanUtilityConstructionGraph node)
    {
        var value = node.Id;
        if (node.Dependencies.Count == 0) return value;
        return $"{value} ({string.Join(", ", node.Dependencies.Select(x => x.GetUniqueId()))})";
    }

    public static bool IsCleanTest(this ITypeInfo? typeInfo) => typeInfo is not null && typeInfo.Interfaces.Any(i => i.ToRuntimeType() == typeof(ICleanTest));
}