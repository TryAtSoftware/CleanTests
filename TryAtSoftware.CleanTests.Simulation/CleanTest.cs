﻿namespace TryAtSoftware.CleanTests.Simulation;

using Microsoft.Extensions.DependencyInjection;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;

[TestSuite]
public class CleanTest : ICleanTest
{
    public IServiceCollection LocalDependenciesCollection { get; } = new ServiceCollection();
    public IServiceCollection GlobalDependenciesCollection { get; } = new ServiceCollection();
}