namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobOffer, "dev_ops")]
[ExternalDemands(CleanUtilitiesCategories.JobAgency, JobAgencyCharacteristics.SoftwareDevelopmentIndustry)]
public class DevOpsJobOfferModelBuilder : BaseJobOfferModelBuilder
{
    protected override string GetTitle() => "DevOps";
}