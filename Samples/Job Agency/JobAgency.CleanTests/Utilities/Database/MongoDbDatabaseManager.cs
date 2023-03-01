namespace JobAgency.CleanTests.Utilities.Database;

using System.Collections.Concurrent;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Data.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.Database, "mongodb", IsGlobal = true)]
public class MongoDbDatabaseManager : IDatabaseManager
{
    private const string DatabaseServerUrl = "mongodb://localhost:27017";
    private readonly IMongoClient _client;

    private readonly object _lockObj = new ();

    private int _lastDatabaseId;
    private readonly ConcurrentQueue<int> _freeDatabaseIdentifiers = new ();

    public MongoDbDatabaseManager()
    {
        this._client = new MongoClient(DatabaseServerUrl);
    }

    public Task<int> GetDatabaseIdAsync(CancellationToken cancellationToken)
    {
        int databaseIdToEnsure;
        lock (this._lockObj)
        {
            if (this._freeDatabaseIdentifiers.TryDequeue(out var freeDatabaseId)) return Task.FromResult(freeDatabaseId);
            databaseIdToEnsure = ++this._lastDatabaseId;
        }

        // If some initialization logic should be applied, add it here.
        return Task.FromResult(databaseIdToEnsure);
    }

    public Task ReleaseDatabaseAsync(int databaseId, CancellationToken cancellationToken)
    {
        this._freeDatabaseIdentifiers.Enqueue(databaseId);
        return this.CleanDatabaseAsync(databaseId, cancellationToken);
    }

    public void RegisterDependencies(int databaseId, IServiceCollection serviceCollection)
    {
        Assert.NotNull(serviceCollection);

        serviceCollection.Configure<DatabaseConnection>(
            options =>
            {
                options.DatabaseServerUrl = DatabaseServerUrl;
                options.DatabaseName = FormatDatabaseName(databaseId);
            });
        serviceCollection.RegisterMongoDbRepositories();
    }

    public void SetupEntities() => MongoDbEntitiesConfiguration.Apply();

    private static string FormatDatabaseName(int databaseId) => $"testdb-{databaseId}";

    private async Task CleanDatabaseAsync(int databaseId, CancellationToken cancellationToken)
    {
        var formattedDatabaseName = FormatDatabaseName(databaseId);
        var database = this._client.GetDatabase(formattedDatabaseName);
        using var collectionsCursor = await database.ListCollectionNamesAsync(cancellationToken: cancellationToken);
        var collectionNames = await collectionsCursor.ToListAsync(cancellationToken: cancellationToken);

        foreach (var collectionName in collectionNames)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);

            var emptyFilter = Builders<BsonDocument>.Filter.Empty;
            await collection.DeleteManyAsync(emptyFilter, cancellationToken);
        }
    }
}