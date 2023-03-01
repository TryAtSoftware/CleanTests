namespace JobAgency.Data.MongoDb;

using JobAgency.Data.Configuration;
using JobAgency.Models.Interfaces;
using Microsoft.Extensions.Options;

public class JobOfferMongoDbRepository : BaseMongoDbRepository<IJobOffer>
{
    public JobOfferMongoDbRepository(IOptions<DatabaseConnection> options)
        : base(options)
    {
    }

    protected override string CollectionName => "Job offers";
}