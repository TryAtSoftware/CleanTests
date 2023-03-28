using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TryAtSoftware.CleanTests.UnitTests.Parametrization;

var setups = TestParameters.ConstructObservableCombinatorialMachineSetups();

Console.Write("Path to the file: ");
var pathToFile = Console.ReadLine() ?? throw new InvalidOperationException("The file path was not read successfully.");

File.Delete(pathToFile);
File.WriteAllText(pathToFile, $"Categories IncompatiblePairs Count{Environment.NewLine}");
foreach (var setup in setups)
{
    var (machine, _) = setup.Materialize();
    var combinations = machine.GenerateAllCombinations().ToArray();

    var incompatiblePairs = new List<string>();
    var visited = new HashSet<string>();

    var (incompatibleUtilitiesMap, _, _) = machine.DiscoverIncompatibleUtilities();
    foreach (var (principalUtility, incompatibleUtilities) in incompatibleUtilitiesMap)
    {
        foreach (var incompatibleUtility in incompatibleUtilities.Where(x => !visited.Contains(x))) incompatiblePairs.Add($"{principalUtility}-{incompatibleUtility}");
        visited.Add(principalUtility);
    }

    var categoryDescriptor = string.Join(',', setup.NumberOfUtilitiesPerCategory.Select(x => $"{x.Key}-{x.Value}"));
    var incompatibleUtilitiesDescriptor = incompatiblePairs.Count > 0 ? string.Join(',', incompatiblePairs) : "none";
    File.AppendAllText(pathToFile, $"{categoryDescriptor} {incompatibleUtilitiesDescriptor} {combinations.Length}{Environment.NewLine}");
}