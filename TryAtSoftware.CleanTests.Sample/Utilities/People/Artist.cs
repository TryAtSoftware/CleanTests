namespace TryAtSoftware.CleanTests.Sample.Utilities.People;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.People, "Artist", Characteristics.People.KnownPerson)]
[OuterDemands(Categories.Creations, Characteristics.Creations.Art)]
public class Artist : IPerson
{
    public string FirstName => "Piet";
    public string LastName => "Mondrian";
}