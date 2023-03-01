namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobOffer, "qa")]
[ExternalDemands(CleanUtilitiesCategories.JobAgency, JobAgencyCharacteristics.SoftwareDevelopmentIndustry)]
[WithRequirements(CleanUtilitiesCategories.Benefits)]
public class QualityAssuranceJobOfferModelBuilder : BaseJobOfferModelBuilder
{
    public QualityAssuranceJobOfferModelBuilder(IModelBuilder<IEnumerable<IJobOfferBenefit>, Nothing> benefitModelBuilder)
        : base(benefitModelBuilder: benefitModelBuilder)
    {
    }
    
    protected override string GetTitle() => "QA";
}