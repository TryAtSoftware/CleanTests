﻿namespace TryAtSoftware.CleanTests.UnitTests.Parametrization;

using System.Text;

public static class TestParameters
{
    public static IEnumerable<CombinatorialMachineSetup> ConstructObservableCombinatorialMachineSetups()
    {
        var setup1 = new CombinatorialMachineSetup("Setup #1").WithCategory("A", 3).WithCategory("B", 2).WithCategory("C", 3).WithCategory("D", 3);
        yield return setup1;
        
        var setup1_1 = new CombinatorialMachineSetup("Setup #1.1", setup1).WithCharacteristics("C", 1, "exec_A2").WithCharacteristics("C", 3, "exec_A2").WithDemands("A", 2, "C", "exec_A2");
        yield return setup1_1;
        
        var setup1_2 = new CombinatorialMachineSetup("Setup #1.2", setup1_1).WithCharacteristics("D", 2, "exec_B1", "exec_B2").WithCharacteristics("D", 3, "exec_B1", "exec_B2").WithDemands("B", 1, "D", "exec_B1").WithDemands("B", 2, "D", "exec_B2");
        yield return setup1_2;

        var setup1_3 = new CombinatorialMachineSetup("Setup #1.3", setup1_2).WithCharacteristics("D", 1, "exec_C3").WithCharacteristics("D", 2, "exec_C3").WithDemands("C", 3, "D", "exec_C3");
        yield return setup1_3;

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

        /*// 10 categories; 5 utilities in each; demands
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
        
        // 1000 categories; 100 utilities in each; all utilities in the first category are incompatible with all utilities in the last category.
        var setup7 = new CombinatorialMachineSetup("Setup #7");
        for (var i = 0; i < 1000; i++) setup7.WithCategory(ConstructCategoryName(i), 100);
        for (var i = 1; i <= 100; i++) setup7.WithDemands(ConstructCategoryName(0), i, ConstructCategoryName(999), "q");
        yield return setup7;*/

        foreach (var randomizedSetup in RandomizeSetup("Setup #9", 1, 2)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #10", 2, 2)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #11", 3, 2)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #12", 4, 2)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #13", 5, 2)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #14", 6, 2)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #15", 1, 3)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #16", 2, 3)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #17", 3, 3)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #18", 4, 3)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #19", 5, 3)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #20", 1, 4)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #21", 2, 4)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #22", 3, 4)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #23", 4, 4)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #24", 5, 4)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #25", 1, 5)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #25", 2, 5)) yield return randomizedSetup;
        foreach (var randomizedSetup in RandomizeSetup("Setup #25", 3, 5)) yield return randomizedSetup;
    }

    private static IEnumerable<CombinatorialMachineSetup> RandomizeSetup(string name, int categories, int utilitiesCount)
    {
        var emptySetup = new CombinatorialMachineSetup(name);
        for (var i = 0; i < categories; i++) emptySetup.WithCategory(ConstructCategoryName(i), utilitiesCount);
        
        var ans = new List<CombinatorialMachineSetup> { emptySetup };
        Randomize(0, emptySetup);
        
        return ans;

        void Randomize(int categoryIndex, CombinatorialMachineSetup baseSetup)
        {
            if (categoryIndex >= categories - 1) return;

            for (var i = 1; i <= utilitiesCount; i++)
            {
                for (var j = 1; j <= utilitiesCount; j++)
                {
                    var nextSetup = new CombinatorialMachineSetup($"{name}.{ans.Count}", baseSetup);
                    for (var k = 1; k <= utilitiesCount; k++)
                        if (j != k) nextSetup.WithCharacteristics(ConstructCategoryName(categoryIndex + 1), k, $"exec_{ConstructCategoryName(categoryIndex)}{i}");

                    nextSetup.WithDemands(ConstructCategoryName(categoryIndex), i, ConstructCategoryName(categoryIndex + 1), $"exec_{ConstructCategoryName(categoryIndex)}{i}");
                    ans.Add(nextSetup);

                    Randomize(categoryIndex + 1, nextSetup);
                }
            }
        }
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