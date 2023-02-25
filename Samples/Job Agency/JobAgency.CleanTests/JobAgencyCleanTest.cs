namespace JobAgency.CleanTests;

using JobAgency.CleanTests.Utilities;
using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.CleanTests.Utilities.Database;
using JobAgency.CleanTests.Utilities.Offers;
using JobAgency.Data.Interfaces;
using JobAgency.Models.Benefits;
using JobAgency.Models.Interfaces;
using JobAgency.Models.Requirements;
using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Equalizer.Core;
using TryAtSoftware.Equalizer.Core.Interfaces;
using TryAtSoftware.Equalizer.Core.ProfileProviders;
using TryAtSoftware.Equalizer.Core.Profiles.General;
using Xunit.Abstractions;

[Collection("Job agency clean tests collection")]
[WithRequirements(CleanUtilitiesCategories.Database)]
public abstract class JobAgencyCleanTest : CleanTest
{
    private int _databaseId;

    protected JobAgencyCleanTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected IDatabaseManager DatabaseManager => this.GetGlobalService<IDatabaseManager>();
    protected IModelBuilder<IJobAgency, Nothing> JobAgencyModelBuilder => this.GetService<IModelBuilder<IJobAgency, Nothing>>();
    protected IRepository<IJobAgency> JobAgencyRepository => this.GetService<IRepository<IJobAgency>>();
    protected IModelBuilder<IJobOffer, JobOfferModelBuildingOptions> JobOfferModelBuilder => this.GetService<IModelBuilder<IJobOffer, JobOfferModelBuildingOptions>>();
    protected IRepository<IJobOffer> JobOfferRepository => this.GetService<IRepository<IJobOffer>>();
    protected IEqualizer Equalizer { get; } = PrepareEqualizer();

    public override async Task InitializeAsync()
    {
        this.InitializeGlobalDependenciesProvider();

        this._databaseId = await this.DatabaseManager.GetDatabaseIdAsync(this.GetCancellationToken());
        
        this.DatabaseManager.SetupEntities();
        this.DatabaseManager.RegisterDependencies(this._databaseId, this.LocalDependenciesCollection);
        
        this.InitializeLocalDependenciesProvider();
    }

    public override async Task DisposeAsync()
    {
        await this.DatabaseManager.ReleaseDatabaseAsync(this._databaseId, this.GetCancellationToken());
        await base.DisposeAsync();
    }

    protected async Task<IJobAgency> CreateJobAgencyAsync()
    {
        var jobAgencyModel = await this.JobAgencyModelBuilder.BuildInstanceAsync(Nothing.Instance, this.GetCancellationToken());
        Assert.NotNull(jobAgencyModel);

        await this.JobAgencyRepository.CreateAsync(jobAgencyModel, this.GetCancellationToken());
        return jobAgencyModel;
    }

    protected async Task<IJobOffer> CreateJobOfferAsync(IJobAgency jobAgency)
    {
        Assert.NotNull(jobAgency);

        var buildingOptions = new JobOfferModelBuildingOptions(jobAgency);
        var jobOfferModel = await this.JobOfferModelBuilder.BuildInstanceAsync(buildingOptions, this.GetCancellationToken());

        await this.JobOfferRepository.CreateAsync(jobOfferModel, this.GetCancellationToken());
        return jobOfferModel;
    }

    protected CancellationToken GetCancellationToken() => CancellationToken.None;

    private static IEqualizer PrepareEqualizer()
    {
        var equalizer = new Equalizer();

        var equalizationProfileProvider = new DedicatedProfileProvider();
        RegisterGeneralEqualizationProfile<IJobAgency>();
        RegisterGeneralEqualizationProfile<IJobOffer>();
        
        // Benefits
        RegisterGeneralEqualizationProfile<CanHaveMoreFreeDays>();
        RegisterGeneralEqualizationProfile<CanHavePerformanceBonus>();
        RegisterGeneralEqualizationProfile<CanUseInsurance>();
        RegisterGeneralEqualizationProfile<CanUseMultiSportCard>();
        
        // Requirements
        RegisterGeneralEqualizationProfile<MustHaveDrivingLicense>();
        RegisterGeneralEqualizationProfile<MustHaveEducation>();
        RegisterGeneralEqualizationProfile<MustHaveMinimumExperience>();
        RegisterGeneralEqualizationProfile<MustWorkFromOffice>();
        
        equalizer.AddProfileProvider(equalizationProfileProvider);
        return equalizer;

        void RegisterGeneralEqualizationProfile<TEntity>()
        {
            equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<TEntity>());
            equalizationProfileProvider.AddProfile(new PartialGeneralEqualizationProfile<TEntity>());
        }
    }
}