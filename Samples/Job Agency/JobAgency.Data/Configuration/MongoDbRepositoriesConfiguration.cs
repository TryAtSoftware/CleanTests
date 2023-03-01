namespace JobAgency.Data.Configuration;

using JobAgency.Data.Interfaces;
using JobAgency.Data.MongoDb;
using JobAgency.Models.Interfaces;
using Microsoft.Extensions.DependencyInjection;

public static class MongoDbRepositoriesConfiguration
{
    public static void RegisterMongoDbRepositories(this IServiceCollection serviceCollection)
    {
        if (serviceCollection is null) throw new ArgumentNullException(nameof(serviceCollection));
        
        serviceCollection.AddScoped<IRepository<IJobAgency>, JobAgencyMongoDbRepository>();
        serviceCollection.AddScoped<IRepository<IJobOffer>, JobOfferMongoDbRepository>();
    }
}