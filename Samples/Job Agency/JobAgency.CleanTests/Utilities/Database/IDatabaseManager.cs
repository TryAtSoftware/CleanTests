namespace JobAgency.CleanTests.Utilities.Database;

using Microsoft.Extensions.DependencyInjection;

public interface IDatabaseManager
{
    Task<int> GetDatabaseIdAsync(CancellationToken cancellationToken);
    Task ReleaseDatabaseAsync(int databaseId, CancellationToken cancellationToken);
    void RegisterDependencies(int databaseId, IServiceCollection serviceCollection);
    void SetupEntities();
}