namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit.Abstractions;

internal class DecoratedType(ITypeInfo typeInfo) : IDecoratedComponent
{
    private readonly ITypeInfo _typeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));

    public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType) => this._typeInfo.GetCustomAttributes(attributeType);
}