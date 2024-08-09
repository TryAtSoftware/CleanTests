namespace TryAtSoftware.CleanTests.Core.XUnit.Interfaces;

using System;
using System.Collections.Generic;
using Xunit.Abstractions;

internal interface IDecoratedComponent
{
    IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType);
}