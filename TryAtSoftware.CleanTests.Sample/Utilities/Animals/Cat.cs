namespace TryAtSoftware.CleanTests.Sample.Utilities.Animals;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.Animals, "Cat", IsGlobal = true)]
public class Cat : IAnimal
{
    public string Name => "Tom";
    public string Phrase => "Meow";
}