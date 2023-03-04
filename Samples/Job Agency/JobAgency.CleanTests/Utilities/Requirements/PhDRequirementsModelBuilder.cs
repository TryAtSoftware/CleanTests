namespace JobAgency.CleanTests.Utilities.Requirements;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Interfaces;
using JobAgency.Models.Requirements;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;

[CleanUtility(CleanUtilitiesCategories.Requirements, "phd", RequirementsCharacteristics.High, IsGlobal = true)]
public class PhDRequirementsModelBuilder : BaseModelBuilder<IEnumerable<IJobOfferRequirement>>
{
    protected override IRandomizer<IEnumerable<IJobOfferRequirement>> ConstructRandomizer()
    {
        var requirements = new IJobOfferRequirement[] { new MustHaveEducation { Level = "PhD", MinimumGrade = 5.5 } };
        return requirements.AsConstantRandomizer();
    }
}