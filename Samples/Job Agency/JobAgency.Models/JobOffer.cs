namespace JobAgency.Models;

using Interfaces;

public class JobOffer : IJobOffer
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public decimal MinSalary { get; set; }
    public decimal MaxSalary { get; set; }
    public long AgencyId { get; set; }
    public ICollection<IJobOfferRequirement> Requirements { get; set; } = new List<IJobOfferRequirement>();
    public ICollection<IJobOfferBenefit> Benefits { get; set; } = new List<IJobOfferBenefit>();
}