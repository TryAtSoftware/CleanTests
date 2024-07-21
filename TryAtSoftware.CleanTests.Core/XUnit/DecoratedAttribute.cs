namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit.Abstractions;

internal class DecoratedAttribute(IAttributeInfo attributeInfo) : IDecoratedComponent
{
    private readonly IAttributeInfo _attributeInfo = attributeInfo ?? throw new ArgumentNullException(nameof(attributeInfo));

    public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType) => this._attributeInfo.GetCustomAttributes(attributeType);
}