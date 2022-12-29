namespace TryAtSoftware.CleanTests.Simulation.Utilities.People;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.People, "Unknown person")]
public class UnknownPerson : IPerson
{
    public string FirstName => "John/Jane";
    public string LastName => "Doe";
}