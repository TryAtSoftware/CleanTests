namespace TryAtSoftware.CleanTests.UnitTests.Extensions;

using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.CleanTests.Core.XUnit;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

public static class ParametrizationExtensions
{
    public static CombinatorialMachine MaterializeAsCombinatorialMachine(this EnvironmentSetup environmentSetup)
    {
        var utilitiesCollection = environmentSetup.Materialize();
        return new CombinatorialMachine(utilitiesCollection);
    }

    public static CleanTestAssemblyData MaterializeAsAssemblyData(this EnvironmentSetup environmentSetup)
    {
        var utilitiesCollection = environmentSetup.Materialize();
        return new CleanTestAssemblyData(utilitiesCollection.GetAllValues());
    }
}