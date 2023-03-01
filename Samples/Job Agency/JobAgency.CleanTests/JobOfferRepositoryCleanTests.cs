namespace JobAgency.CleanTests;

using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Equalizer.Core.PartialValues;
using TryAtSoftware.Randomizer.Core.Helpers;
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
    
    [CleanFact, WithRequirements(CleanUtilitiesCategories.JobAgency, CleanUtilitiesCategories.JobOffer)]
    public async Task JobOfferDescriptionShouldBeUpdatedSuccessfully()
    {
        // Arrange
        var jobAgency = await this.CreateJobAgencyAsync();
        var jobOffer = await this.CreateJobOfferAsync(jobAgency);
        var newDescription = RandomizationHelper.GetRandomString();

        // Act
        var updateResult = await this.JobOfferRepository.UpdateField(jobOffer.Id, jo => jo.Description, newDescription, this.GetCancellationToken());
        Assert.True(updateResult);
        
        // Assert
        var retrievedJobOffer = await this.JobOfferRepository.GetAsync(jobOffer.Id, this.GetCancellationToken());
        this.Equalizer.AssertEquality(jobOffer.Exclude(nameof(IJobOffer.Description)), retrievedJobOffer);
        Assert.Equal(newDescription, retrievedJobOffer.Description);
    }
}