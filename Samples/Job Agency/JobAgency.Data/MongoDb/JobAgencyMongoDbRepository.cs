namespace JobAgency.Data.MongoDb;

using JobAgency.Data.Configuration;
using JobAgency.Models.Interfaces;
using Microsoft.Extensions.Options;

public class JobAgencyMongoDbRepository : BaseMongoDbRepository<IJobAgency>
{
    public JobAgencyMongoDbRepository(IOptions<DatabaseConnection> options)
        : base(options)
    {
    }

    protected override string CollectionName => "Job agencies";
}