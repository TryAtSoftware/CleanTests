﻿namespace TryAtSoftware.CleanTests.Core.Enums;

using System;

[Flags]
public enum CleanTestMetadataPresentations
{
    None = 0,
    InTestCaseName = 1,
    InTraits = 2
}