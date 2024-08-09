namespace TryAtSoftware.CleanTests.Core.XUnit;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using Xunit.Abstractions;

internal class DecoratedMethod(IMethodInfo methodInfo) : IDecoratedComponent
{
    private readonly IMethodInfo _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

    public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType) => this._methodInfo.GetCustomAttributes(attributeType);
}