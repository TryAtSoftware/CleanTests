namespace JobAgency.CleanTests.Utilities.Benefits;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Benefits;
using JobAgency.Models.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;

[CleanUtility(CleanUtilitiesCategories.Benefits, "more_activity", IsGlobal = true)]
public class MoreActivityBenefitsModelBuilder : BaseModelBuilder<IEnumerable<IJobOfferBenefit>>
{
    protected override IRandomizer<IEnumerable<IJobOfferBenefit>> ConstructRandomizer()
    {
        var benefits = new IJobOfferBenefit[] { new CanHaveMoreFreeDays { Days = 5 }, new CanUseMultiSportCard { Category = "lite" } };
        return benefits.AsConstantRandomizer();
    }
}