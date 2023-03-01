namespace JobAgency.CleanTests.Utilities.Offers;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobOffer, "truck_driver")]
[ExternalDemands(CleanUtilitiesCategories.JobAgency, JobAgencyCharacteristics.TransportIndustry)]
public class TruckDriverJobOfferModelBuilder : BaseJobOfferModelBuilder
{
    protected override string GetTitle() => "Truck driver";
}