namespace JobAgency.CleanTests.Utilities.Requirements;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Interfaces;
using JobAgency.Models.Requirements;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;

[CleanUtility(CleanUtilitiesCategories.Requirements, "experienced_bachelor", RequirementsCharacteristics.Medium, RequirementsCharacteristics.RequiresExperience)]
public class ExperiencedBachelorRequirementsModelBuilder : BaseModelBuilder<IEnumerable<IJobOfferRequirement>>
{
    protected override IRandomizer<IEnumerable<IJobOfferRequirement>> ConstructRandomizer()
    {
        var requirements = new IJobOfferRequirement[] { new MustHaveEducation { Level = "Bachelor", MinimumGrade = 5 }, new MustHaveMinimumExperience { Years = 3 } };
        return requirements.AsConstantRandomizer();
    }
}