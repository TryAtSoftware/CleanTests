namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Randomization;
using JobAgency.CleanTests.Utilities.Common;
using JobAgency.Models;
using JobAgency.Models.Interfaces;
using TryAtSoftware.Randomizer.Core;
using TryAtSoftware.Randomizer.Core.Interfaces;
using TryAtSoftware.Randomizer.Core.Primitives;

public abstract class BaseJobOfferModelBuilder : BaseModelBuilder<IJobOffer, JobOfferModelBuildingOptions, JobOfferModelBuildingSetup>
{
    private readonly IModelBuilder<IEnumerable<IJobOfferRequirement>, Nothing> _requirementModelBuilder;
    private readonly IModelBuilder<IEnumerable<IJobOfferBenefit>, Nothing> _benefitModelBuilder;
    
    protected BaseJobOfferModelBuilder(IModelBuilder<IEnumerable<IJobOfferRequirement>, Nothing> requirementModelBuilder = null, IModelBuilder<IEnumerable<IJobOfferBenefit>, Nothing> benefitModelBuilder = null)
    {
        this._requirementModelBuilder = requirementModelBuilder;
        this._benefitModelBuilder = benefitModelBuilder;
    }
    
    protected override async Task<JobOfferModelBuildingSetup> ExecuteInitializationAsync(JobOfferModelBuildingOptions options, CancellationToken cancellationToken)
    {
        IEnumerable<IJobOfferRequirement> requirements;
        if (this._requirementModelBuilder is null) requirements = Enumerable.Empty<IJobOfferRequirement>();
        else requirements = await this._requirementModelBuilder.BuildInstanceAsync(Nothing.Instance, cancellationToken);

        IEnumerable<IJobOfferBenefit> benefits;
        if (this._benefitModelBuilder is null) benefits = Enumerable.Empty<IJobOfferBenefit>();
        else benefits = await this._benefitModelBuilder.BuildInstanceAsync(Nothing.Instance, cancellationToken);

        return new JobOfferModelBuildingSetup(requirements, benefits);
    }

    protected override IRandomizer<IJobOffer> ConstructRandomizer(JobOfferModelBuildingOptions options, JobOfferModelBuildingSetup setup)
    {
        var randomizer = new ComplexRandomizer<JobOffer>();
        randomizer.RegisterCommonIdentifiableRandomizationRules();
        randomizer.Randomize(x => x.Title, this.GetTitle().AsConstantRandomizer());
        randomizer.Randomize(x => x.Description, new StringRandomizer());
        randomizer.Randomize(x => x.AgencyId, options.Agency.Id.AsConstantRandomizer());
        randomizer.Randomize(x => x.MinSalary, new DecimalRandomizer());
        randomizer.Randomize(x => x.MaxSalary, x => new DecimalRandomizer(minValue: x.MinSalary + 10));
        randomizer.Randomize(x => x.Requirements, setup.Requirements.ToList().AsConstantRandomizer());
        randomizer.Randomize(x => x.Benefits, setup.Benefits.ToList().AsConstantRandomizer());
        
        return randomizer;
    }

    protected abstract string GetTitle();
}