namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobOffer, "consultant")]
[WithRequirements(CleanUtilitiesCategories.JobOfferRequirements, CleanUtilitiesCategories.JobOfferBenefits)]
public class ConsultantJobOfferModelBuilder : BaseJobOfferModelBuilder
{
    public ConsultantJobOfferModelBuilder(IModelBuilder<IEnumerable<IJobOfferRequirement>, Nothing> requirementModelBuilder, IModelBuilder<IEnumerable<IJobOfferBenefit>, Nothing> benefitModelBuilder)
        : base(requirementModelBuilder, benefitModelBuilder)
    {
    }
    
    protected override string GetTitle() => "Consultant";
}