namespace TryAtSoftware.CleanTests.Sample.Utilities.People;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.People, "Writer", Characteristics.People.KnownPerson, Characteristics.People.LiteraryWorkAuthor)]
public class Writer : IPerson
{
    public string FirstName => "Charles";
    public string LastName => "Bukowski";
}