namespace TryAtSoftware.CleanTests.Core.XUnit.Extensions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;
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
}