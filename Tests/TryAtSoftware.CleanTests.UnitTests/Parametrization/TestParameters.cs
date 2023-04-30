﻿namespace TryAtSoftware.CleanTests.UnitTests.Parametrization;

using System.Text;

public static class TestParameters
{
    public static IEnumerable<object?[]> GetInvalidStringParameters()
    {
        yield return new object?[] { null };
        yield return new object?[] { string.Empty };
        yield return new object?[] { " " };
    }

    public static IEnumerable<CombinatorialMachineSetup> ConstructObservableCombinatorialMachineSetups()
    {
        var setup1 = new CombinatorialMachineSetup("Setup #1", 26).WithCategory("A", 3).WithCategory("B", 2).WithCategory("C", 3).WithCategory("D", 3);
        setup1.WithCharacteristics("C", 1, "exec_A2").WithCharacteristics("C", 3, "exec_A2").WithCharacteristics("D", 1, "exec_C3").WithCharacteristics("D", 2, "exec_B1", "exec_B2", "exec_C3").WithCharacteristics("D", 3, "exec_B1", "exec_B2");
        setup1.WithDemands("A", 2, "C", "exec_A2").WithDemands("B", 1, "D", "exec_B1").WithDemands("B", 2, "D", "exec_B2").WithDemands("C", 3, "D", "exec_C3");
        yield return setup1;

        var setup2 = new CombinatorialMachineSetup("Setup #2", 6).WithCategory("A", 2).WithCategory("B", 2).WithCategory("C", 2).WithCategory("D", 2).WithCategory("E", 2);
        setup2.WithCharacteristics("B", 2, "exec_A1").WithCharacteristics("C", 1, "exec_B2").WithCharacteristics("D", 2, "exec_C1").WithCharacteristics("E", 1, "exec_D2");
        setup2.WithDemands("A", 1, "B", "exec_A1").WithDemands("B", 2, "C", "exec_B2").WithDemands("C", 1, "D", "exec_C1").WithDemands("D", 2, "E", "exec_D2");
        yield return setup2;

        // 5 categories; 10 utilities in each; no demands
        var setup3 = new CombinatorialMachineSetup("Setup #3", 100_000);
        for (var i = 0; i < 5; i++) setup3.WithCategory(ConstructCategoryName(i), 10);
        yield return setup3;

        // 10 categories; 3 utilities in each; no demands
        var setup4 = new CombinatorialMachineSetup("Setup #4", 59049);
        for (var i = 0; i < 10; i++) setup4.WithCategory(ConstructCategoryName(i), 3);
        yield return setup4;

        // 10 categories; 5 utilities in each; demands
        var setup5 = new CombinatorialMachineSetup("Setup #5", 98415);
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
            for (var j = 1; j <= 5; j++) setup5.WithDemands(ConstructCategoryName(i), j, ConstructCategoryName(i + 1), $"exec_{ConstructCategoryName(i)}{j}");
        }
        
        yield return setup5;
        
        // 100 categories; 10 utilities in each; only one matching utility in each
        var setup6 = new CombinatorialMachineSetup("Setup #6", 10);
        for (var i = 0; i < 100; i++) setup6.WithCategory(ConstructCategoryName(i), 10);
        for (var i = 1; i < 100; i++)
        {
            for (var j = 1; j <= 10; j++) setup6.WithCharacteristics(ConstructCategoryName(i), j, $"exec_{ConstructCategoryName(i - 1)}{j}");
        }

        for (var i = 0; i < 99; i++)
        {
            for (var j = 1; j <= 10; j++) setup6.WithDemands(ConstructCategoryName(i), j, ConstructCategoryName(i + 1), $"exec_{ConstructCategoryName(i)}{j}");
        }

        yield return setup6;
        
        // 1000 categories; 100 utilities in each; all utilities in the first category are incompatible with all utilities in the last category.
        var setup7 = new CombinatorialMachineSetup("Setup #7", 0);
        for (var i = 0; i < 1000; i++) setup7.WithCategory(ConstructCategoryName(i), 100);
        for (var i = 1; i <= 100; i++) setup7.WithDemands(ConstructCategoryName(0), i, ConstructCategoryName(999), "q");
        yield return setup7;

        // 3 categories; 2 utilities in each; Inapplicable demands
        var setup8 = new CombinatorialMachineSetup("Setup #8", 32);
        for (var i = 0; i < 5; i++) setup8.WithCategory(ConstructCategoryName(i), 2);
        for (var i = 0; i < 5; i++)
            setup8.WithDemands(ConstructCategoryName(i), 1, "non-existing demand category", "demand1", "demand2");
        yield return setup8;

        var setup9 = new CombinatorialMachineSetup("Setup #9", 48);
        for (var i = 0; i < 5; i++) setup9.WithCategory(ConstructCategoryName(i), 3);
        for (var i = 0; i < 4; i++) setup9.WithDemands(ConstructCategoryName(i), 1, ConstructCategoryName(4), $"exec_{ConstructCategoryName(i)}{1}");
        yield return setup9;
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