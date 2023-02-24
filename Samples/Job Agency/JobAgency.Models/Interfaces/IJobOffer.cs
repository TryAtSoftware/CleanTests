namespace JobAgency.Models.Interfaces;

public interface IJobOffer : IIdentifiable
{
    string Description { get; set; }
    decimal MinSalary { get; set; }
    decimal MaxSalary { get; set; }
    
    long AgencyId { get; set; }
    
    ICollection<IJobOfferRequirement> Requirements { get; set; }
    ICollection<IJobOfferBenefit> Benefits { get; set; }
}