namespace TryAtSoftware.CleanTests.Sample.Utilities.Creations;

using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Sample.Utilities.People;

[CleanUtility(Categories.Creations, "Picture", Characteristics.Creations.Art)]
[WithRequirements(Categories.People)]
[ExternalDemands(Categories.People, Characteristics.People.KnownPerson)]
public class Picture : ICreation
{
    private readonly IPerson _author;

    public Picture(IPerson author)
    {
        this._author = author ?? throw new ArgumentNullException(nameof(author));
    }

    public string Name => $"Composition with Red, Blue and Yellow - {this._author.LastName}, {this._author.FirstName}";
    public string Type => "Picture";
}