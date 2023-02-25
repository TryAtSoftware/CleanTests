namespace JobAgency.Data.MongoDb;

using System.Linq.Expressions;
using JobAgency.Data.Configuration;
using JobAgency.Data.Interfaces;
using JobAgency.Models.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

public abstract class BaseMongoDbRepository<TEntity> : IRepository<TEntity>
    where TEntity : IIdentifiable
{
    private readonly IMongoDatabase _database;
    
    protected BaseMongoDbRepository(IOptions<DatabaseConnection> options)
    {
        var mongoClient = new MongoClient(options.Value.DatabaseServerUrl);
        this._database = mongoClient.GetDatabase(options.Value.DatabaseName);
    }
    
    protected abstract string CollectionName { get; }
    protected IMongoCollection<TEntity> Collection => this._database.GetCollection<TEntity>(this.CollectionName);

    public Task CreateAsync(TEntity entity, CancellationToken cancellationToken) => this.Collection.InsertOneAsync(entity, cancellationToken: cancellationToken);

    public async Task<TEntity> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = ConstructIdFilter(id);
        var findCursor = await this.Collection.FindAsync(filter, cancellationToken: cancellationToken);
        return await findCursor.FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
    {
        var findCursor = await this.Collection.FindAsync(filter, cancellationToken: cancellationToken);
        return await findCursor.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateField<TValue>(Guid id, Expression<Func<TEntity, TValue>> fieldExpression, TValue newValue, CancellationToken cancellationToken)
    {
        var filter = ConstructIdFilter(id);
        var updateDefinition = Builders<TEntity>.Update.Set(fieldExpression, newValue);

        var updateResult = await this.Collection.UpdateOneAsync(filter, updateDefinition, cancellationToken: cancellationToken);
        return updateResult.ModifiedCount == 1;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = ConstructIdFilter(id);
        var deleteResult = await this.Collection.DeleteOneAsync(filter, cancellationToken);
        return deleteResult.DeletedCount == 1;
    }

    private static FilterDefinition<TEntity> ConstructIdFilter(Guid id) => Builders<TEntity>.Filter.Eq(x => x.Id, id);
}