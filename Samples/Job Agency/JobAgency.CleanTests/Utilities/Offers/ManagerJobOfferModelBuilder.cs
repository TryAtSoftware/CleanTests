namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobOffer, "manager")]
[WithRequirements(CleanUtilitiesCategories.JobOfferRequirements)]
[InternalDemands(CleanUtilitiesCategories.JobOfferRequirements, RequirementsCharacteristics.High)]
public class ManagerJobOfferModelBuilder : BaseJobOfferModelBuilder
{
    public ManagerJobOfferModelBuilder(IModelBuilder<IEnumerable<IJobOfferRequirement>, Nothing> requirementModelBuilder)
        : base (requirementModelBuilder)
    {
    }
    
    protected override string GetTitle() => "Manager";
}