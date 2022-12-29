namespace TryAtSoftware.CleanTests.Sample.Utilities.Creations;

using TryAtSoftware.CleanTests.Core.Attributes;

[CleanUtility(Categories.Creations, "Picture")]
[ExternalDemands(Categories.People, Characteristics.KnownPerson)]
public class Picture : ICreation
{
    public string Name => "Composition with Red, Blue and Yellow - Mondrian, Piet";
    public string Type => "Picture";
}