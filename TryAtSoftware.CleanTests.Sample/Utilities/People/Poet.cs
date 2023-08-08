namespace TryAtSoftware.CleanTests.Sample.Utilities.People;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.People, "Poet", Characteristics.People.KnownPerson, Characteristics.People.LiteraryWorkAuthor)]
public class Poet : IPerson
{
    public string FirstName => "William";
    public string LastName => "Shakespear";
}