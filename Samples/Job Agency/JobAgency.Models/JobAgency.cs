namespace JobAgency.Models;

using Interfaces;

public class JobAgency : IJobAgency
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<string> OfferedJobTypes { get; set; }
}