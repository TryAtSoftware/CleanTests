using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Core.Enums;

[assembly: TestFramework("TryAtSoftware.CleanTests.Core.XUnit.CleanTestFramework", "TryAtSoftware.CleanTests.Core")]
[assembly: ConfigureCleanTestsFramework(UtilitiesPresentation = CleanTestMetadataPresentation.InTraits, MaxDegreeOfParallelism = 3)]
[assembly: SharesUtilitiesWith("TryAtSoftware.CleanTests.Sample.Mathematics")]
[assembly: SharesUtilitiesWith("SomeMissingAssembly")]