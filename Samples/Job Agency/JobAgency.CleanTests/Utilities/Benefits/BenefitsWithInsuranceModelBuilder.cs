namespace JobAgency.CleanTests.Utilities.Benefits;

using JobAgency.CleanTests.Utilities.Common;
using JobAgency.CleanTests.Utilities.Constants;
using JobAgency.Models.Benefits;
using JobAgency.Models.Interfaces;
using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;
using TryAtSoftware.Randomizer.Core.Primitives;

[CleanUtility(CleanUtilitiesCategories.Benefits, "with_insurance", IsGlobal = true)]
[WithRequirements(CleanUtilitiesCategories.Benefits)]
public class BenefitsWithInsuranceModelBuilder : BaseModelBuilder<IEnumerable<IJobOfferBenefit>, Nothing, IEnumerable<IJobOfferBenefit>>
{
    private readonly IModelBuilder<IEnumerable<IJobOfferBenefit>, Nothing> _benefitsModelBuilder;

    public BenefitsWithInsuranceModelBuilder(IModelBuilder<IEnumerable<IJobOfferBenefit>, Nothing> benefitsModelBuilder)
    {
        this._benefitsModelBuilder = benefitsModelBuilder ?? throw new ArgumentNullException(nameof(benefitsModelBuilder));
    }

    protected override Task<IEnumerable<IJobOfferBenefit>> ExecuteInitializationAsync(Nothing options, CancellationToken cancellationToken) => this._benefitsModelBuilder.BuildInstanceAsync(options, cancellationToken);

    protected override IRandomizer<IEnumerable<IJobOfferBenefit>> ConstructRandomizer(Nothing options, IEnumerable<IJobOfferBenefit> setup)
    {
        var insuranceBenefitRandomizer = new ComplexRandomizer<CanUseInsurance>();
        insuranceBenefitRandomizer.AddRandomizationRule(x => x.Description, new StringRandomizer());
        var insuranceBenefit = insuranceBenefitRandomizer.PrepareRandomValue();

        var allBenefits = new List<IJobOfferBenefit> { insuranceBenefit };
        allBenefits.AddRange(setup);

        return allBenefits.AsConstantRandomizer();
    }
}