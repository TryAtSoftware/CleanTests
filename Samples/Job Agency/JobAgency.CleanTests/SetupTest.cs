namespace JobAgency.CleanTests;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;
using Xunit.Abstractions;

public class SetupTest : JobAgencyCleanTest
{
    public SetupTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [CleanFact]
    public void EnsureDatabaseManagerIsSuccessfullyProvided() => Assert.NotNull(this.DatabaseManager);

    [CleanFact, WithRequirements(CleanUtilitiesCategories.JobAgency)]
    public void EnsureJobAgencyModelBuilderIsSuccessfullyProvided() => Assert.NotNull(this.JobAgencyModelBuilder);

    [CleanFact]
    public void EnsureJobAgencyModelBuilderIsNotProvidedIfNotRequired() => Assert.ThrowsAny<Exception>(() => this.JobAgencyModelBuilder);
}