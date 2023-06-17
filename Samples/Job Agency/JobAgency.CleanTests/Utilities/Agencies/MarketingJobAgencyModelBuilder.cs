namespace JobAgency.CleanTests.Utilities.Agencies;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobAgency, "marketing", JobAgencyCharacteristics.OtherIndustry)]
public class MarketingJobAgencyModelBuilder : BaseJobAgencyModelBuilder
{
    protected override IEnumerable<string> GetOfferedJobTypes() => new[] { "Marketing", "Advertisement", "Promotions" };
}