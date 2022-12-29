namespace TryAtSoftware.CleanTests.Simulation.Utilities.People;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.People, "Standard person", Characteristics.KnownPerson)]
public class LibraryAuthor : IPerson
{
    public string FirstName => "Tony";
    public string LastName => "Troeff";
}