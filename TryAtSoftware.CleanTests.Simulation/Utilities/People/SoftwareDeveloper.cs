namespace TryAtSoftware.CleanTests.Simulation.Utilities.People;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.People, "Software developer", Characteristics.KnownPerson)]
public class SoftwareDeveloper : IPerson
{
    public string FirstName => "Tony";
    public string LastName => "Troeff";
}