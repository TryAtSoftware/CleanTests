using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Enums;

[assembly: TestFramework("TryAtSoftware.CleanTests.Core.XUnit.CleanTestFramework", "TryAtSoftware.CleanTests.Core")]
[assembly: ConfigureCleanTestsFramework(UtilitiesPresentations = CleanTestMetadataPresentations.InTraits | CleanTestMetadataPresentations.InTestCaseName, MaxDegreeOfParallelism = 3)]
[assembly: SharesUtilitiesWith("TryAtSoftware.CleanTests.Sample.Mathematics")]
[assembly: SharesUtilitiesWith("SomeMissingAssembly")]