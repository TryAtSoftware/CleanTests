namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit.Abstractions;

public class DecoratedAttribute : IDecoratedComponent
{
    private readonly IAttributeInfo _attributeInfo;

    public DecoratedAttribute(IAttributeInfo attributeInfo)
    {
        this._attributeInfo = attributeInfo ?? throw new ArgumentNullException(nameof(attributeInfo));
    }

    public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType) => this._attributeInfo.GetCustomAttributes(attributeType);
}