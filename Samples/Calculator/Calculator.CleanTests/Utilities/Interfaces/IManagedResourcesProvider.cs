namespace Calculator.CleanTests.Utilities.Interfaces;

public interface IManagedResourcesProvider<in TOptions>
{
    Task<int> GetResourceIdAsync(TOptions options, CancellationToken cancellationToken);
    Task ReleaseResourceAsync(int resourceId, CancellationToken cancellationToken);
}