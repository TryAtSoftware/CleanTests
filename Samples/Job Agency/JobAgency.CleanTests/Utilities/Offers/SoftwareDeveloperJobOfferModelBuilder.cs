namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobOffer, "software_developer")]
[ExternalDemands(CleanUtilitiesCategories.JobAgency, JobAgencyCharacteristics.SoftwareDevelopmentIndustry)]
public class SoftwareDeveloperJobOfferModelBuilder : BaseJobOfferModelBuilder
{
}