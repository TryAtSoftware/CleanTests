namespace JobAgency.CleanTests.Utilities.Agencies;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobAgency, "transport", JobAgencyCharacteristics.TransportIndustry)]
public class TransportJobAgencyModelBuilder : BaseJobAgencyModelBuilder
{
    protected override IEnumerable<string> GetOfferedJobTypes() => new[] { "Positions for drivers" };
}