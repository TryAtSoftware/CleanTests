namespace JobAgency.Data.Configuration;

using JobAgency.Models;
using JobAgency.Models.Benefits;
using JobAgency.Models.Interfaces;
using JobAgency.Models.Requirements;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

public static class MongoDbEntitiesConfiguration
{
    private static bool _isApplied;
    private static readonly object _lockObj = new ();
    
    public static void Apply()
    {
        lock (_lockObj)
        {
            if (_isApplied) return;
            _isApplied = true;
        }

        BsonClassMap.RegisterClassMap<JobAgency>(
            x =>
            {
                ApplyIdentifiableConfigurations(x);
                x.MapMember(y => y.Name);
                x.MapMember(y => y.OfferedJobTypes);
            });
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IJobAgency, JobAgency>());

        BsonClassMap.RegisterClassMap<JobOffer>(
            x =>
            {
                ApplyIdentifiableConfigurations(x);
                x.MapMember(y => y.Description);
                x.MapMember(y => y.AgencyId);
                x.MapMember(y => y.MinSalary);
                x.MapMember(y => y.MaxSalary);
                x.MapMember(y => y.Benefits);
                x.MapMember(y => y.Requirements);
            });
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IJobOffer, JobOffer>());
        
        // Benefits
        BsonClassMap.RegisterClassMap<CanHaveMoreFreeDays>(
            x =>
            {
                ApplyPolymorphicConfigurations(x);
                x.MapMember(y => y.Days);
            });
        BsonClassMap.RegisterClassMap<CanHavePerformanceBonus>(ApplyPolymorphicConfigurations);
        BsonClassMap.RegisterClassMap<CanUseInsurance>(
            x =>
            {
                ApplyPolymorphicConfigurations(x);
                x.MapMember(y => y.Description);
            });
        BsonClassMap.RegisterClassMap<CanUseMultiSportCard>(
            x =>
            {
                ApplyPolymorphicConfigurations(x);
                x.MapMember(y => y.Category);
            });

        // Requirements
        BsonClassMap.RegisterClassMap<MustHaveDrivingLicense>(
            x =>
            {
                ApplyPolymorphicConfigurations(x);
                x.MapMember(y => y.Categories);
            });
        BsonClassMap.RegisterClassMap<MustHaveEducation>(
            x =>
            {
                ApplyPolymorphicConfigurations(x);
                x.MapMember(y => y.Level);
                x.MapMember(y => y.MinimumGrade);
            });
        BsonClassMap.RegisterClassMap<MustHaveMinimumExperience>(
            x =>
            {
                ApplyPolymorphicConfigurations(x);
                x.MapMember(y => y.Years);
            });
        BsonClassMap.RegisterClassMap<MustWorkFromOffice>(ApplyPolymorphicConfigurations);
    }

    private static void ApplyIdentifiableConfigurations<T>(BsonClassMap<T> classMap)
        where T : IIdentifiable
    {
        if (classMap is null) throw new ArgumentNullException(nameof(classMap));
        classMap.MapIdMember(x => x.Id);
    }

    private static void ApplyPolymorphicConfigurations<T>(BsonClassMap<T> classMap)
    {
        if (classMap is null) throw new ArgumentNullException(nameof(classMap));
        classMap.SetDiscriminatorIsRequired(true);
    }
}