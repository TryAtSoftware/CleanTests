namespace JobAgency.CleanTests.Utilities.Agencies;

using JobAgency.CleanTests.Randomization;
using JobAgency.CleanTests.Utilities.Common;
using JobAgency.Models;
using JobAgency.Models.Interfaces;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;
using TryAtSoftware.Randomizer.Core.Primitives;

public abstract class BaseJobAgencyModelBuilder : BaseModelBuilder<IJobAgency>
{
    protected override IRandomizer<IJobAgency> ConstructRandomizer()
    {
        var randomizer = new ComplexRandomizer<JobAgency>();
        randomizer.RegisterCommonIdentifiableRandomizationRules();
        randomizer.AddRandomizationRule(x => x.Name, new StringRandomizer());
        randomizer.AddRandomizationRule(x => x.OfferedJobTypes, this.GetOfferedJobTypes().AsConstantRandomizer());
        return randomizer;
    }

    protected abstract IEnumerable<string> GetOfferedJobTypes();
}