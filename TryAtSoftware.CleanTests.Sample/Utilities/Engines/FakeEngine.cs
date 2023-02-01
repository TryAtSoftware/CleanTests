namespace TryAtSoftware.CleanTests.Sample.Utilities.Engines;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.Engines, "Fake engine", IsGlobal = true)]
public class FakeEngine : IEngine
{
    public string ReadLine() => "Pre-defined text";
}
