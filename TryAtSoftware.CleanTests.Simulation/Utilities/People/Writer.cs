namespace TryAtSoftware.CleanTests.Simulation.Utilities.People;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.People, "Writer", Characteristics.KnownPerson, Characteristics.LiteraryWorkAuthor)]
public class Writer : IPerson
{
    public string FirstName => "Charles";
    public string LastName => "Bukowski";
}