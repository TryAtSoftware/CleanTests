namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Randomization;
using JobAgency.CleanTests.Utilities.Common;
using JobAgency.Models;
using JobAgency.Models.Interfaces;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;
using TryAtSoftware.Randomizer.Core.Primitives;

public class BaseJobOfferModelBuilder : BaseModelBuilder<IJobOffer, JobOfferModelBuildingOptions>
{
    protected override IRandomizer<IJobOffer> ConstructRandomizer(JobOfferModelBuildingOptions options)
    {
        var randomizer = new ComplexRandomizer<JobOffer>();
        randomizer.RegisterCommonIdentifiableRandomizationRules();
        randomizer.AddRandomizationRule(x => x.Description, new StringRandomizer());
        randomizer.AddRandomizationRule(x => x.AgencyId, options.Agency.Id.AsConstantRandomizer());
        randomizer.AddRandomizationRule(x => x.MinSalary, new DecimalRandomizer());
        randomizer.AddRandomizationRule(x => x.MaxSalary, x => new DecimalRandomizer(minValue: x.MinSalary + 10));
        return randomizer;
    }
}