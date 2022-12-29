namespace TryAtSoftware.CleanTests.Simulation.Utilities.People;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.People, "Poet", Characteristics.KnownPerson, Characteristics.LiteraryWorkAuthor)]
public class Poet : IPerson
{
    public string FirstName => "William";
    public string LastName => "Shakespear";
}