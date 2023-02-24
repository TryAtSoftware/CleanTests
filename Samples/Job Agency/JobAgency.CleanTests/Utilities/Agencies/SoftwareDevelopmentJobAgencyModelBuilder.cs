namespace JobAgency.CleanTests.Utilities.Agencies;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobAgency, "software_development", JobAgencyCharacteristics.SoftwareDevelopmentIndustry)]
public class SoftwareDevelopmentJobAgencyModelBuilder : BaseJobAgencyModelBuilder
{
    protected override IEnumerable<string> GetOfferedJobTypes() => new[] { "IT positions" };
}