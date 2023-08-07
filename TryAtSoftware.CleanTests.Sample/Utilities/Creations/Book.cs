namespace TryAtSoftware.CleanTests.Sample.Utilities.Creations;

using TryAtSoftware.CleanTests.Core.Attributes;
using TryAtSoftware.CleanTests.Sample.Utilities.People;

[CleanUtility(Categories.Creations, "Book")]
[WithRequirements(Categories.People)]
[InternalDemands(Categories.People, Characteristics.People.LiteraryWorkAuthor)]
public class Book : ICreation
{
    private readonly IPerson _author;

    public Book(IPerson author)
    {
        this._author = author ?? throw new ArgumentNullException(nameof(author));
    }

    public string Name => $"Volume #1 - {this._author.LastName}, {this._author.FirstName}";
    public string Type => "Book";
}