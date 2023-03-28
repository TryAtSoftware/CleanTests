﻿namespace TryAtSoftware.CleanTests.UnitTests.Parametrization;

using System.Text;

public static class TestParameters
{
    public static IEnumerable<CombinatorialMachineSetup> ConstructObservableCombinatorialMachineSetups()
    {
        var setup1 = new CombinatorialMachineSetup("Setup #1").WithCategory("A", 3).WithCategory("B", 2).WithCategory("C", 3).WithCategory("D", 3);
        setup1.WithCharacteristics("C", 1, "exec_A2").WithCharacteristics("C", 3, "exec_A2").WithCharacteristics("D", 1, "exec_C3").WithCharacteristics("D", 2, "exec_B1", "exec_B2", "exec_C3").WithCharacteristics("D", 3, "exec_B1", "exec_B2");
        setup1.WithDemands("A", 2, "C", "exec_A2").WithDemands("B", 1, "D", "exec_B1").WithDemands("B", 2, "D", "exec_B2").WithDemands("C", 3, "D", "exec_C3");
        yield return setup1;

        var setup2 = new CombinatorialMachineSetup("Setup #2").WithCategory("A", 2).WithCategory("B", 2).WithCategory("C", 2).WithCategory("D", 2).WithCategory("E", 2);
        setup2.WithCharacteristics("B", 2, "exec_A1").WithCharacteristics("C", 1, "exec_B2").WithCharacteristics("D", 2, "exec_C1").WithCharacteristics("E", 1, "exec_D2");
        setup2.WithDemands("A", 1, "B", "exec_A1").WithDemands("B", 2, "C", "exec_B2").WithDemands("C", 1, "D", "exec_C1").WithDemands("D", 2, "E", "exec_D2");
        yield return setup2;

        // 5 categories; 10 utilities in each; no demands
        var setup3 = new CombinatorialMachineSetup("Setup #3");
        for (var i = 0; i < 5; i++) setup3.WithCategory(ConstructCategoryName(i), 10);
        yield return setup3;

        // 10 categories; 3 utilities in each; no demands
        var setup4 = new CombinatorialMachineSetup("Setup #4");
        for (var i = 0; i < 10; i++) setup4.WithCategory(ConstructCategoryName(i), 3);
        yield return setup4;

        // 10 categories; 5 utilities in each; demands
        var setup5 = new CombinatorialMachineSetup("Setup #5");
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
        var setup6 = new CombinatorialMachineSetup("Setup #6");
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
        
        // TODO: Add a setup for which none of the last level utilities satisfy the first level's demands.
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