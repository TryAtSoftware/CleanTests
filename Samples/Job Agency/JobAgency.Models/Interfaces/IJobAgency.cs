namespace JobAgency.Models.Interfaces;

public interface IJobAgency : IIdentifiable
{
    string Name { get; set; }
}