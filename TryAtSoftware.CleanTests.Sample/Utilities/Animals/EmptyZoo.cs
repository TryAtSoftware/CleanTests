namespace TryAtSoftware.CleanTests.Sample.Utilities.Animals;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.Zoos, "Empty zoo", IsGlobal = true)]
public class EmptyZoo : IZoo
{
    public IEnumerable<IAnimal> GetAnimals() => Enumerable.Empty<IAnimal>();
}