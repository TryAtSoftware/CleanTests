namespace JobAgency.Models.Requirements;

public class MustHaveMinimumExperience : BaseJobOfferRequirement
{
    public int Years { get; set; }
}