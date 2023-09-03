namespace TryAtSoftware.CleanTests.Core.Utilities;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class CombinatorialMachine
{
    private readonly ICleanTestInitializationCollection<ICleanUtilityDescriptor> _utilities;

    public CombinatorialMachine(ICleanTestInitializationCollection<ICleanUtilityDescriptor> utilities)
    {
        this._utilities = utilities ?? throw new ArgumentNullException(nameof(utilities));
    }

    public IEnumerable<IDictionary<string, string>> GenerateAllCombinations()
    {
        Dictionary<string, int> utilityIndexById = new ();
        foreach (var utility in this._utilities.GetAllValues()) utilityIndexById[utility.Id] = utilityIndexById.Count;

        var utilitiesCount = utilityIndexById.Count;

        Dictionary<string, Bitmask> bitmasksByCategory = new ();
        var bitmaskByUtilityIndex = new Bitmask[utilitiesCount];

        var characteristicsRegister = new Dictionary<string, Dictionary<string, Bitmask>>();

        foreach (var (category, utilitiesForCategory) in this._utilities)
        {
            var categoryBitmask = new Bitmask(utilitiesCount, initializeWithZeros: true);
            var categoryCharacteristicRegister = new Dictionary<string, Bitmask>();

            foreach (var utility in utilitiesForCategory)
            {
                var utilityIndex = utilityIndexById[utility.Id];
                categoryBitmask.Set(utilityIndex);
                bitmaskByUtilityIndex[utilityIndex] = new Bitmask(utilitiesCount, initializeWithZeros: false);

                foreach (var characteristic in utility.Characteristics)
                {
                    if (!categoryCharacteristicRegister.ContainsKey(characteristic)) categoryCharacteristicRegister[characteristic] = new Bitmask(utilitiesCount, initializeWithZeros: true);
                    categoryCharacteristicRegister[characteristic].Set(utilityIndex);
                }
            }

            bitmasksByCategory[category] = categoryBitmask;
            characteristicsRegister[category] = categoryCharacteristicRegister;
        }

        foreach (var (category, utilitiesForCategory) in this._utilities)
        {
            var u = ~bitmasksByCategory[category];
            
            foreach (var utility in utilitiesForCategory)
            {
                var utilityIndex = utilityIndexById[utility.Id];

                bitmaskByUtilityIndex[utilityIndex] &= u;
                foreach (var (demandCategory, demandsForCategory) in utility.ExternalDemands)
                {
                    if (!characteristicsRegister.ContainsKey(demandCategory)) continue;
                    
                    var v = ~bitmasksByCategory[demandCategory];
                    foreach (var demand in demandsForCategory)
                    {
                        var complement = v;
                        if (characteristicsRegister[demandCategory].TryGetValue(demand, out var demandBitmask)) complement |= demandBitmask;
                        bitmaskByUtilityIndex[utilityIndex] &= complement;
                    }

                    var test = bitmaskByUtilityIndex[utilityIndex] & v;

                    // if (test.SetBitsCount() == 0)
                    if (test.FindMostSignificantSetBit() == -1)
                    {
                        // bitmaskByUtilityIndex[utilityIndex].Clear();
                        bitmaskByUtilityIndex[utilityIndex] = new Bitmask(utilitiesCount, initializeWithZeros: true);
                    }
                }
            }
        }

        // Sort by set bits count and refresh the `utilityIndexById` dictionary values.
        var virtuallyIndexedUtilities = new ICleanUtilityDescriptor[utilitiesCount];
        foreach (var utility in this._utilities.GetAllValues()) virtuallyIndexedUtilities[utilityIndexById[utility.Id]] = utility;

        var slots = new int[this._utilities.Categories.Count];
        return this.Dfs(slotIndex: 0, slots, new Bitmask(utilitiesCount, initializeWithZeros: false), bitmaskByUtilityIndex, BuildTransformationFunction(virtuallyIndexedUtilities));
    }

    // TODO: Extract an array where each index is mapped to the number of unique categories that are succeeding
    private IEnumerable<T> Dfs<T>(int slotIndex, int[] slots, Bitmask accumulatedBitmask, Bitmask[] bitmasks, Func<int[], T> transform)
    {
        if (slotIndex == slots.Length) yield return transform(slots);
        else
        {
            var utilityIndex = accumulatedBitmask.FindMostSignificantSetBit();
            while (utilityIndex != -1)
            {
                slots[slotIndex] = utilityIndex;
                foreach (var combination in this.Dfs(slotIndex + 1, slots, accumulatedBitmask & bitmasks[utilityIndex], bitmasks, transform))
                    yield return combination;

                accumulatedBitmask.Unset(utilityIndex);
                utilityIndex = accumulatedBitmask.FindMostSignificantSetBit();
            }
        }
    }

    private static Func<int[], IDictionary<string, string>> BuildTransformationFunction(ICleanUtilityDescriptor[] virtuallyIndexedUtilities)
        => slots =>
        {
            Dictionary<string, string> ans = new ();
            foreach (var utilityIndex in slots)
            {
                var utility = virtuallyIndexedUtilities[utilityIndex];
                ans[utility.Category] = utility.Id;
            }

            return ans;
        };
}