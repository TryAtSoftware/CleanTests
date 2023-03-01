namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobOffer, "university_teacher")]
[ExternalDemands(CleanUtilitiesCategories.JobAgency, JobAgencyCharacteristics.OtherIndustry)]
[WithRequirements(CleanUtilitiesCategories.Requirements)]
[InternalDemands(CleanUtilitiesCategories.Requirements, RequirementsCharacteristics.High)]
public class UniversityTeacherJobOfferModelBuilder : BaseJobOfferModelBuilder
{
    public UniversityTeacherJobOfferModelBuilder(IModelBuilder<IEnumerable<IJobOfferRequirement>, Nothing> requirementModelBuilder)
        : base (requirementModelBuilder)
    {
    }
    
    protected override string GetTitle() => "University teacher";
}