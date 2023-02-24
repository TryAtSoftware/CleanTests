namespace JobAgency.CleanTests;

using JobAgency.CleanTests.Utilities;
using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.CleanTests.Utilities.Database;
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

    protected CancellationToken GetCancellationToken() => CancellationToken.None;

    private static IEqualizer PrepareEqualizer()
    {
        var equalizer = new Equalizer();

        var equalizationProfileProvider = new DedicatedProfileProvider();
        equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<IJobAgency>());
        equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<IJobOffer>());
        
        // Benefits
        equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<CanHaveMoreFreeDays>());
        equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<CanHavePerformanceBonus>());
        equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<CanUseInsurance>());
        equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<CanUseMultiSportCard>());
        
        // Requirements
        equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<MustHaveDrivingLicense>());
        equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<MustHaveEducation>());
        equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<MustHaveMinimumExperience>());
        equalizationProfileProvider.AddProfile(new GeneralEqualizationProfile<MustWorkFromOffice>());
        
        equalizer.AddProfileProvider(equalizationProfileProvider);
        return equalizer;
    }
}