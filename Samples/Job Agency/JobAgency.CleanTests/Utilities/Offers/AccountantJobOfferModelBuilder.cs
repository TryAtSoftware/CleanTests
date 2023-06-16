namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobOffer, "accountant")]
[ExternalDemands(CleanUtilitiesCategories.JobAgency, JobAgencyCharacteristics.OtherIndustry)]
[WithRequirements(CleanUtilitiesCategories.JobOfferRequirements)]
[InternalDemands(CleanUtilitiesCategories.JobOfferRequirements, RequirementsCharacteristics.Medium)]
public class AccountantJobOfferModelBuilder : BaseJobOfferModelBuilder
{
    public AccountantJobOfferModelBuilder(IModelBuilder<IEnumerable<IJobOfferRequirement>, Nothing> requirementModelBuilder)
        : base (requirementModelBuilder)
    {
    }
    
    protected override string GetTitle() => "Accountant";
}