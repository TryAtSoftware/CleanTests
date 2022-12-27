﻿namespace TryAtSoftware.CleanTests.Core.Attributes;

using System;
using Quantum.DMS.CleanTests.Attributes;
using TryAtSoftware.CleanTests.Core.XUnit;
using Xunit;

[CleanTestCaseDiscoverer(typeof(CleanFactTestCaseDiscoverer))]
[AttributeUsage(AttributeTargets.Method)]
public class CleanFact : FactAttribute
{
}