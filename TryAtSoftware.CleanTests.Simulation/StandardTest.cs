﻿namespace TryAtSoftware.CleanTests.Simulation;

using TryAtSoftware.CleanTests.Core.Attributes;

public class StandardTest : CleanTest
{
    [CleanFact]
    public void StandardTestCase()
    {
        Assert.Equal(2 + 2, 4);
    }
}