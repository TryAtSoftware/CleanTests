
#if DEBUG

namespace JobAgency.CleanTests.Utilities.Benefits;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Benefits;
using JobAgency.Models.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;

[CleanUtility(CleanUtilitiesCategories.JobOfferBenefits, "bonus", IsGlobal = true)]
public class BonusPaymentBenefitsModelBuilder : BaseModelBuilder<IEnumerable<IJobOfferBenefit>>
{
    protected override IRandomizer<IEnumerable<IJobOfferBenefit>> ConstructRandomizer()
    {
        var benefits = new IJobOfferBenefit[] { new CanHavePerformanceBonus() };
        return benefits.AsConstantRandomizer();
    }
}

#endif