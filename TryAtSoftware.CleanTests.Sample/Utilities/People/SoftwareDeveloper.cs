namespace TryAtSoftware.CleanTests.Sample.Utilities.People;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.People, "Software developer", Characteristics.People.KnownPerson)]
public class SoftwareDeveloper : IPerson
{
    public string FirstName => "Tony";
    public string LastName => "Troeff";
}