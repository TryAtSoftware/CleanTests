namespace JobAgency.CleanTests;

using JobAgency.CleanTests.Utilities;
using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.CleanTests.Utilities.Database;
using JobAgency.Models;
using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Attributes;
using Xunit.Abstractions;

[WithRequirements(CleanUtilitiesCategories.Database)]
public abstract class JobAgencyCleanTest : CleanTest
{
    private int _databaseId;

    protected JobAgencyCleanTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected IDatabaseManager DatabaseManager => this.GetGlobalService<IDatabaseManager>();
    protected IModelBuilder<JobAgency, Nothing> JobAgencyModelBuilder => this.GetService<IModelBuilder<JobAgency, Nothing>>();

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

    protected CancellationToken GetCancellationToken() => CancellationToken.None;
}