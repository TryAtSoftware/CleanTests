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

    [CleanFact, WithRequirements(CleanUtilitiesCategories.JobAgency)]
    public async Task JobAgencyShouldBeCreatedSuccessfully()
    {
        var jobAgency = await this.CreateJobAgencyAsync();
        Assert.NotNull(jobAgency);
    }

    [CleanFact, WithRequirements(CleanUtilitiesCategories.JobAgency, CleanUtilitiesCategories.JobOffer)]
    public void EnsureJobOfferModelBuilderIsSuccessfullyProvided() => Assert.NotNull(this.JobOfferModelBuilder);

    [CleanFact]
    public void EnsureJobOfferModelBuilderIsNotProvidedIfNotRequired() => Assert.ThrowsAny<Exception>(() => this.JobOfferModelBuilder);

    [CleanFact, WithRequirements(CleanUtilitiesCategories.JobAgency, CleanUtilitiesCategories.JobOffer)]
    public async Task JobOfferShouldBeCreatedSuccessfully()
    {
        var jobAgency = await this.CreateJobAgencyAsync();
        var jobOffer = await this.CreateJobOfferAsync(jobAgency);
        Assert.NotNull(jobOffer);
    }
}