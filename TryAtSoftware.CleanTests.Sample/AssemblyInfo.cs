using TryAtSoftware.CleanTests.Core.Attributes;

[assembly: TestFramework("TryAtSoftware.CleanTests.Core.XUnit.CleanTestFramework", "TryAtSoftware.CleanTests.Core")]
[assembly: ConfigureCleanTestsFramework(UseTraits = true, MaxDegreeOfParallelism = 3)]
[assembly: SharesUtilitiesWith("TryAtSoftware.CleanTests.Sample.Mathematics")]
[assembly: SharesUtilitiesWith("SomeMissingAssembly")]