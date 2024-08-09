namespace TryAtSoftware.CleanTests.UnitTests.Parametrization;

using System.Text;

internal static class TestParameters
{
    public static IEnumerable<object?[]> GetInvalidStringParameters()
    {
        yield return new object?[] { null };
        yield return new object?[] { string.Empty };
        yield return new object?[] { " " };
    }

    public static IEnumerable<(EnvironmentSetup EnvironmentSetup, int ExpectedCombinationsCount)> ConstructObservableCombinatorialMachineSetups()
    {
        var setup1 = new EnvironmentSetup("Setup #1").WithCategory("A", 3).WithCategory("B", 2).WithCategory("C", 3).WithCategory("D", 3);
        setup1.WithCharacteristics("C", 1, "exec_A2").WithCharacteristics("C", 3, "exec_A2").WithCharacteristics("D", 1, "exec_C3").WithCharacteristics("D", 2, "exec_B1", "exec_B2", "exec_C3").WithCharacteristics("D", 3, "exec_B1", "exec_B2");
        setup1.WithExternalDemands("A", 2, "C", "exec_A2").WithExternalDemands("B", 1, "D", "exec_B1").WithExternalDemands("B", 2, "D", "exec_B2").WithExternalDemands("C", 3, "D", "exec_C3");
        yield return (setup1, 26);

        var setup2 = new EnvironmentSetup("Setup #2").WithCategory("A", 2).WithCategory("B", 2).WithCategory("C", 2).WithCategory("D", 2).WithCategory("E", 2);
        setup2.WithCharacteristics("B", 2, "exec_A1").WithCharacteristics("C", 1, "exec_B2").WithCharacteristics("D", 2, "exec_C1").WithCharacteristics("E", 1, "exec_D2");
        setup2.WithExternalDemands("A", 1, "B", "exec_A1").WithExternalDemands("B", 2, "C", "exec_B2").WithExternalDemands("C", 1, "D", "exec_C1").WithExternalDemands("D", 2, "E", "exec_D2");
        yield return (setup2, 6);

        // 5 categories; 10 utilities in each; no demands
        var setup3 = new EnvironmentSetup("Setup #3");
        for (var i = 0; i < 5; i++) setup3.WithCategory(ConstructCategoryName(i), 10);
        yield return (setup3, 100_000);

        // 10 categories; 3 utilities in each; no demands
        var setup4 = new EnvironmentSetup("Setup #4");
        for (var i = 0; i < 10; i++) setup4.WithCategory(ConstructCategoryName(i), 3);
        yield return (setup4, 59049);

        // 10 categories; 5 utilities in each; demands
        var setup5 = new EnvironmentSetup("Setup #5");
        for (var i = 0; i < 10; i++) setup5.WithCategory(ConstructCategoryName(i), 5);
        for (var i = 1; i < 10; i++)
        {
            for (var j = 1; j <= 5; j++)
            {
                for (var l = 0; l < 3; l++)
                    setup5.WithCharacteristics(ConstructCategoryName(i), (j + l + 1) % 5 + 1, $"exec_{ConstructCategoryName(i - 1)}{j}");
            }
        }

        for (var i = 0; i < 9; i++)
        {
            for (var j = 1; j <= 5; j++) setup5.WithExternalDemands(ConstructCategoryName(i), j, ConstructCategoryName(i + 1), $"exec_{ConstructCategoryName(i)}{j}");
        }

        yield return (setup5, 98415);

        // 100 categories; 10 utilities in each; only one matching utility in each
        var setup6 = new EnvironmentSetup("Setup #6");
        for (var i = 0; i < 100; i++) setup6.WithCategory(ConstructCategoryName(i), 10);
        for (var i = 1; i < 100; i++)
        {
            for (var j = 1; j <= 10; j++) setup6.WithCharacteristics(ConstructCategoryName(i), j, $"exec_{ConstructCategoryName(i - 1)}{j}");
        }

        for (var i = 0; i < 99; i++)
        {
            for (var j = 1; j <= 10; j++) setup6.WithExternalDemands(ConstructCategoryName(i), j, ConstructCategoryName(i + 1), $"exec_{ConstructCategoryName(i)}{j}");
        }

        yield return (setup6, 10);

        // 1000 categories; 100 utilities in each; all utilities in the first category are incompatible with all utilities in the last category.
        var setup7 = new EnvironmentSetup("Setup #7");
        for (var i = 0; i < 1000; i++) setup7.WithCategory(ConstructCategoryName(i), 100);
        for (var i = 1; i <= 100; i++) setup7.WithExternalDemands(ConstructCategoryName(0), i, ConstructCategoryName(999), "q");
        yield return (setup7, 0);

        // 3 categories; 2 utilities in each; Inapplicable demands
        var setup8 = new EnvironmentSetup("Setup #8");
        for (var i = 0; i < 5; i++) setup8.WithCategory(ConstructCategoryName(i), 2);
        for (var i = 0; i < 5; i++)
            setup8.WithExternalDemands(ConstructCategoryName(i), 1, "non-existing demand category", "demand1", "demand2");
        yield return (setup8, 32);

        var setup9 = new EnvironmentSetup("Setup #9");
        for (var i = 0; i < 5; i++) setup9.WithCategory(ConstructCategoryName(i), 3);
        for (var i = 0; i < 4; i++) setup9.WithExternalDemands(ConstructCategoryName(i), 1, ConstructCategoryName(4), $"exec_{ConstructCategoryName(i)}{1}");
        yield return (setup9, 48);
    }

    public static IEnumerable<(EnvironmentSetup EnvironmentSetup, string PathToExpectedResult)> ConstructObservableConstructionManagerSetups()
    {
        // 10 categories; 3 utilities in each; no demands
        // The utilities in every even category depend on utilities from the next one
        var setup1 = new EnvironmentSetup("Setup #1");
        for (var i = 0; i < 10; i++) setup1.WithCategory(ConstructCategoryName(i), 3);
        for (var i = 0; i < 10; i += 2)
        {
            for (var j = 1; j <= 3; j++) setup1.WithRequirements(ConstructCategoryName(i), j, ConstructCategoryName(i + 1));
        }
        yield return (setup1, "ExpectedResults/DependenciesManager/setup1.txt");
        
        // 3 categories; 3 utilities in each; no demands
        // The utilities in each category depend on utilities from the next one
        var setup2 = new EnvironmentSetup("Setup #2");
        for (var i = 0; i < 3; i++) setup2.WithCategory(ConstructCategoryName(i), 3);
        for (var i = 0; i < 2; i++)
        {
            for (var j = 1; j <= 3; j++) setup2.WithRequirements(ConstructCategoryName(i), j, ConstructCategoryName(i + 1));
        }
        yield return (setup2, "ExpectedResults/DependenciesManager/setup2.txt");

        // 1 category; 5 utilities; no demands
        // Self dependencies - utilities depend on utilities from the same category
        var setup3 = new EnvironmentSetup("Setup #3");
        setup3.WithCategory(ConstructCategoryName(0), 5);
        for (var j = 1; j <= 4; j++) setup3.WithRequirements(ConstructCategoryName(0), j, ConstructCategoryName(0));
        yield return (setup3, "ExpectedResults/DependenciesManager/setup3.txt");

        // 2 categories; 5 utilities; no demands
        // Transitive dependencies - utilities from the first category depend on utilities from the second while utilities from the second category depend on utilities from the first.
        var setup4 = new EnvironmentSetup("Setup #4");
        for (var i = 0; i < 2; i++) setup4.WithCategory(ConstructCategoryName(i), 5);
        for (var i = 1; i <= 4; i++)
        {
            setup4.WithRequirements(ConstructCategoryName(0), i, ConstructCategoryName(1));
            setup4.WithRequirements(ConstructCategoryName(1), i, ConstructCategoryName(0));
        }
        yield return (setup4, "ExpectedResults/DependenciesManager/setup4.txt");
        
        // 2 categories; 3 utilities; no demands
        // Self dependencies + Transitive dependencies - utilities from the first category depend on utilities from the second while utilities from the second category depend on utilities from the first.
        var setup5 = new EnvironmentSetup("Setup #5");
        for (var i = 0; i < 2; i++) setup5.WithCategory(ConstructCategoryName(i), 3);
        for (var i = 1; i <= 2; i++)
        {
            setup5.WithRequirements(ConstructCategoryName(0), i, ConstructCategoryName(0), ConstructCategoryName(1));
            setup5.WithRequirements(ConstructCategoryName(1), i, ConstructCategoryName(0), ConstructCategoryName(1));
        }
        yield return (setup5, "ExpectedResults/DependenciesManager/setup5.txt");

        // 3 categories; 3 utilities; no demands
        // Utilities from the first two categories depend on utilities from the third
        var setup6 = new EnvironmentSetup("Setup #6");
        for (var i = 0; i < 3; i++) setup6.WithCategory(ConstructCategoryName(i), 3);
        for (var i = 0; i < 2; i++)
        {
            for (var j = 1; j <= 2; j++) setup6.WithRequirements(ConstructCategoryName(i), j, ConstructCategoryName(2));
        }
        yield return (setup6, "ExpectedResults/DependenciesManager/setup6.txt");
    }

    private static string ConstructCategoryName(int letterIndex)
    {
        var sb = new StringBuilder();
        
        do
        {
            sb.Append((char)('A' + letterIndex % 26));
            letterIndex /= 26;
        } while (letterIndex != 0);

        return sb.ToString();
    }
}