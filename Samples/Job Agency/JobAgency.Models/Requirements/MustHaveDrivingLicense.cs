namespace JobAgency.Models.Requirements;

public class MustHaveDrivingLicense : BaseJobOfferRequirement
{
    public ICollection<string> Categories { get; set; }
}