namespace JobAgency.CleanTests.Utilities.Agencies;

using JobAgency.CleanTests.Randomization;
using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;
using TryAtSoftware.Randomizer.Core.Primitives;

[CleanUtility(CleanUtilitiesCategories.JobAgency, "software_development", JobAgencyCharacteristics.SoftwareDevelopmentIndustry)]
public class SoftwareDevelopmentJobAgencyModelBuilder : BaseModelBuilder<JobAgency>
{
    protected override IRandomizer<JobAgency> ConstructRandomizer()
    {
        var randomizer = new ComplexRandomizer<JobAgency>();
        randomizer.RegisterCommonIdentifiableRandomizationRules();
        randomizer.AddRandomizationRule(x => x.Name, new StringRandomizer());
        return randomizer;
    }
}