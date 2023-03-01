namespace JobAgency.CleanTests;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;
using Xunit.Abstractions;

public class JobAgencyRepositoryCleanTests : JobAgencyCleanTest
{
    public JobAgencyRepositoryCleanTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [CleanFact, WithRequirements(CleanUtilitiesCategories.JobAgency)]
    public async Task JobAgenciesShouldBeCreatedSuccessfully()
    {
        // Arrange
        var originalJobAgency = await this.CreateJobAgencyAsync();

        // Act
        var retrievedJobAgency = await this.JobAgencyRepository.GetAsync(originalJobAgency.Id, this.GetCancellationToken());
        
        // Assert
        this.Equalizer.AssertEquality(originalJobAgency, retrievedJobAgency);
    }

    [CleanFact, WithRequirements(CleanUtilitiesCategories.JobAgency)]
    public async Task JobAgenciesShouldBeDeletedSuccessfully()
    {
        // Arrange
        var originalJobAgency = await this.CreateJobAgencyAsync();

        // Act
        var deleteResult = await this.JobAgencyRepository.DeleteAsync(originalJobAgency.Id, this.GetCancellationToken());
        Assert.True(deleteResult);
        
        // Assert
        var retrievedJobAgency = await this.JobAgencyRepository.GetAsync(originalJobAgency.Id, this.GetCancellationToken());
        Assert.Null(retrievedJobAgency);
    }
}