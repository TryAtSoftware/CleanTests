namespace JobAgency.Models.Requirements;

public class MustHaveEducation : BaseJobOfferRequirement
{
    public string Level { get; set; }
    public double? MinimumGrade { get; set; }
}