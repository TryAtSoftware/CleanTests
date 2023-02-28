namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobOffer, "taxi_driver")]
[ExternalDemands(CleanUtilitiesCategories.JobAgency, JobAgencyCharacteristics.TransportIndustry)]
public class TaxiDriverJobOfferModelBuilder : BaseJobOfferModelBuilder
{
    protected override string GetTitle() => "Taxi driver";
}