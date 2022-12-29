namespace TryAtSoftware.CleanTests.Core.XUnit.Interfaces;

using System;
using System.Collections.Generic;
using Xunit.Abstractions;

public interface IDecoratedComponent
{
    IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType);
}