namespace TryAtSoftware.CleanTests.Core.XUnit;

internal class TestOutputHelperPlaceholder
{
    private static TestOutputHelperPlaceholder? _instance;

    public static TestOutputHelperPlaceholder Instance => _instance ??= new TestOutputHelperPlaceholder();

    private TestOutputHelperPlaceholder() { }
}