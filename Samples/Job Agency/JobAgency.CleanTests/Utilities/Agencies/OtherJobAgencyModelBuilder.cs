namespace JobAgency.CleanTests.Utilities.Agencies;

using JobAgency.CleanTests.Utilities.Constants;
using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(CleanUtilitiesCategories.JobAgency, "other", JobAgencyCharacteristics.OtherIndustry)]
public class OtherJobAgencyModelBuilder : BaseJobAgencyModelBuilder
{
    protected override IEnumerable<string> GetOfferedJobTypes() => new[] { "Other positions" };
}