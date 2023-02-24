namespace JobAgency.CleanTests;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;
using Xunit.Abstractions;

public class JobOfferRepositoryCleanTests : JobAgencyCleanTest
{
    public JobOfferRepositoryCleanTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [CleanFact, WithRequirements(CleanUtilitiesCategories.JobAgency, CleanUtilitiesCategories.JobOffer)]
    public async Task JobOffersShouldBeCreatedSuccessfully()
    {
        // Arrange
        var jobAgency = await this.CreateJobAgencyAsync();
        var originalJobOffer = await this.CreateJobOfferAsync(jobAgency);

        // Act
        var retrievedJobOffer = await this.JobOfferRepository.GetAsync(originalJobOffer.Id, this.GetCancellationToken());
        
        // Assert
        this.Equalizer.AssertEquality(originalJobOffer, retrievedJobOffer);
    }

    [CleanFact, WithRequirements(CleanUtilitiesCategories.JobAgency, CleanUtilitiesCategories.JobOffer)]
    public async Task JobOffersShouldBeDeletedSuccessfully()
    {
        // Arrange
        var jobAgency = await this.CreateJobAgencyAsync();
        var jobOffer = await this.CreateJobOfferAsync(jobAgency);

        // Act
        var deleteResult = await this.JobOfferRepository.DeleteAsync(jobOffer.Id, this.GetCancellationToken());
        Assert.True(deleteResult);
        
        // Assert
        var retrievedJobOffer = await this.JobOfferRepository.GetAsync(jobOffer.Id, this.GetCancellationToken());
        Assert.Null(retrievedJobOffer);
    }
}