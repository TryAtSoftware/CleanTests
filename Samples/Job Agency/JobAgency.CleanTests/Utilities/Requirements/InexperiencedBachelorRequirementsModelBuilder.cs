namespace JobAgency.CleanTests.Utilities.Requirements;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Interfaces;
using JobAgency.Models.Requirements;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;

[CleanUtility(CleanUtilitiesCategories.JobOfferRequirements, "inexperienced_bachelor", RequirementsCharacteristics.Medium, RequirementsCharacteristics.DoesNotRequireExperience)]
public class InexperiencedBachelorRequirementsModelBuilder : BaseModelBuilder<IEnumerable<IJobOfferRequirement>>
{
    protected override IRandomizer<IEnumerable<IJobOfferRequirement>> ConstructRandomizer()
    {
        var requirements = new IJobOfferRequirement[] { new MustHaveEducation { Level = "Bachelor", MinimumGrade = 5 } };
        return requirements.AsConstantRandomizer();
    }
}