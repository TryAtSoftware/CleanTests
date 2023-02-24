namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.Models.Interfaces;

public record JobOfferModelBuildingSetup(IEnumerable<IJobOfferRequirement> Requirements, IEnumerable<IJobOfferBenefit> Benefits);