namespace JobAgency.Data.Configuration;

using JobAgency.Models;
using JobAgency.Models.Benefits;
using JobAgency.Models.Interfaces;
using JobAgency.Models.Requirements;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

public static class MongoDbEntitiesConfiguration
{
    private static bool _isApplied = false;
    private static object _lockObj = new ();
    
    public static void Apply()
    {
        lock (_lockObj)
        {
            if (_isApplied) return;
            _isApplied = true;
        }

        // BsonClassMap.RegisterClassMap<IIdentifiable>(x => x.MapIdMember(y => y.Id));
        // BsonClassMap.RegisterClassMap<IJobOfferRequirement>(x => x.SetDiscriminatorIsRequired(true));
        // BsonClassMap.RegisterClassMap<IJobOfferBenefit>(x => x.SetDiscriminatorIsRequired(true));
        
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IJobAgency, JobAgency>());
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IJobOffer, JobOffer>());
        
        /*// Benefits
        BsonClassMap.RegisterClassMap<CanHaveMoreFreeDays>(
            x =>
            {
                x.SetDiscriminatorIsRequired(true);
                x.MapMember(y => y.Days);
            });
        BsonClassMap.RegisterClassMap<CanHavePerformanceBonus>(x => x.SetDiscriminatorIsRequired(true));
        BsonClassMap.RegisterClassMap<CanUseInsurance>(
            x =>
            {
                x.SetDiscriminatorIsRequired(true);
                x.MapMember(y => y.Description);
            });
        BsonClassMap.RegisterClassMap<CanUseMultiSportCard>(
            x =>
            {
                x.SetDiscriminatorIsRequired(true);
                x.MapMember(y => y.Category);
            });

        // Requirements
        BsonClassMap.RegisterClassMap<MustHaveDrivingLicense>(
            x =>
            {
                x.SetDiscriminatorIsRequired(true);
                x.MapMember(y => y.Categories);
            });
        BsonClassMap.RegisterClassMap<MustHaveEducation>(
            x =>
            {
                x.SetDiscriminatorIsRequired(true);
                x.MapMember(y => y.Level);
                x.MapMember(y => y.MinimumGrade);
            });
        BsonClassMap.RegisterClassMap<MustHaveMinimumExperience>(
            x =>
            {
                x.SetDiscriminatorIsRequired(true);
                x.MapMember(y => y.Years);
            });
        BsonClassMap.RegisterClassMap<MustWorkFromOffice>(x => x.SetDiscriminatorIsRequired(true));*/
        
        // Benefits
        BsonClassMap.RegisterClassMap<CanHaveMoreFreeDays>(x => x.MapMember(y => y.Days));
        BsonClassMap.RegisterClassMap<CanHavePerformanceBonus>();
        BsonClassMap.RegisterClassMap<CanUseInsurance>(x => x.MapMember(y => y.Description));
        BsonClassMap.RegisterClassMap<CanUseMultiSportCard>(x => x.MapMember(y => y.Category));

        // Requirements
        BsonClassMap.RegisterClassMap<MustHaveDrivingLicense>(x => x.MapMember(y => y.Categories));
        BsonClassMap.RegisterClassMap<MustHaveEducation>(
            x =>
            {
                x.MapMember(y => y.Level);
                x.MapMember(y => y.MinimumGrade);
            });
        BsonClassMap.RegisterClassMap<MustHaveMinimumExperience>(x => x.MapMember(y => y.Years));
        BsonClassMap.RegisterClassMap<MustWorkFromOffice>();
    }
}