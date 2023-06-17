namespace Calculator.CleanTests.Utilities.Interfaces;

public interface IApiProvider : IManagedResourcesProvider<object>
{
    IApiAccessor GetApiAccessor(int resourceId);
}