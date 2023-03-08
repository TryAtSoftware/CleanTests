namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobOffer, "actor")]
[ExternalDemands(CleanUtilitiesCategories.JobAgency, JobAgencyCharacteristics.OtherIndustry)]
[WithRequirements(CleanUtilitiesCategories.Requirements, CleanUtilitiesCategories.Benefits)]
public class ActorJobOfferModelBuilder : BaseJobOfferModelBuilder
{
    public ActorJobOfferModelBuilder(IModelBuilder<IEnumerable<IJobOfferRequirement>, Nothing> requirementModelBuilder, IModelBuilder<IEnumerable<IJobOfferBenefit>, Nothing> benefitModelBuilder)
        : base (requirementModelBuilder, benefitModelBuilder)
    {
    }
    
    protected override string GetTitle() => "Actor";
}