namespace TryAtSoftware.CleanTests.Core.XUnit;

using Xunit.Sdk;

internal class TestOutputHelperPlaceholder
{
    private TestOutputHelper? _instance;
    
    internal TestOutputHelper Build() => this._instance ??= new TestOutputHelper();
}