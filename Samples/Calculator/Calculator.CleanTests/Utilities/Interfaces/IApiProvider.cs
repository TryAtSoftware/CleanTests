namespace Calculator.CleanTests.Utilities.Interfaces;

public interface IApiProvider : IManagedResourcesProvider<Nothing>
{
    IApiAccessor GetApiAccessor(int resourceId);
}