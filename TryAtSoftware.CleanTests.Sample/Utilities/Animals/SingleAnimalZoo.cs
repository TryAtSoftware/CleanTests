namespace TryAtSoftware.CleanTests.Sample.Utilities.Animals;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.Zoos, "A zoo containing a single animal", IsGlobal = true)]
[WithRequirements(Categories.Animals)]
public class SingleAnimalZoo : IZoo
{
    private readonly IAnimal _animal;

    public SingleAnimalZoo(IAnimal animal)
    {
        this._animal = animal ?? throw new ArgumentNullException(nameof(animal));
    }

    public IEnumerable<IAnimal> GetAnimals() => new[] { this._animal };
}