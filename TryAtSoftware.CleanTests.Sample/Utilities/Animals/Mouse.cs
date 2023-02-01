namespace TryAtSoftware.CleanTests.Sample.Utilities.Animals;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.Animals, "Mouse", IsGlobal = true)]
public class Mouse : IAnimal
{
    public string Name => "Jerry";
    public string Phrase => "click";
}