namespace TryAtSoftware.CleanTests.Core.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
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

        var virtuallyIndexedUtilities = this.VirtuallyIndexUtilities(utilityIndexById);
        var bitmaskByCategory = ExtractCategoryBitmasks(virtuallyIndexedUtilities);
        var bitmaskByCategoryAndCharacteristic = ExtractCharacteristicBitmasks(virtuallyIndexedUtilities);
        var bitmaskByUtility = ExtractUtilityBitmasks(virtuallyIndexedUtilities, bitmaskByCategory, bitmaskByCategoryAndCharacteristic);

        // Sort by set bits count and refresh the `utilityIndexById` dictionary values.
        var order = Enumerable.Range(0, utilitiesCount).OrderBy(x => bitmaskByUtility[x].CountSetBits()).ToArray();

        var orderedVirtuallyIndexedUtilities = new ICleanUtilityDescriptor[utilitiesCount];
        for (var i = 0; i < utilitiesCount; i++) orderedVirtuallyIndexedUtilities[i] = virtuallyIndexedUtilities[order[i]];

        bitmaskByCategory = ExtractCategoryBitmasks(orderedVirtuallyIndexedUtilities);
        bitmaskByCategoryAndCharacteristic = ExtractCharacteristicBitmasks(orderedVirtuallyIndexedUtilities);
        bitmaskByUtility = ExtractUtilityBitmasks(orderedVirtuallyIndexedUtilities, bitmaskByCategory, bitmaskByCategoryAndCharacteristic);

        var slots = new int[this._utilities.Categories.Count];
        return this.Dfs(slotIndex: 0, slots, new Bitmask(utilitiesCount, initializeWithZeros: false), bitmaskByUtility, BuildTransformationFunction(orderedVirtuallyIndexedUtilities), BuildContinuationCheck(orderedVirtuallyIndexedUtilities, bitmaskByCategory));
    }

    private ICleanUtilityDescriptor[] VirtuallyIndexUtilities(IDictionary<string, int> utilityIndexById)
    {
        var utilitiesCount = utilityIndexById.Count;
        var result = new ICleanUtilityDescriptor[utilitiesCount];
        foreach (var utility in this._utilities.GetAllValues())
        {
            var index = utilityIndexById[utility.Id];
            result[index] = utility;
        }

        return result;
    }

    private static IDictionary<string, Bitmask> ExtractCategoryBitmasks(ICleanUtilityDescriptor[] virtuallyIndexedUtilities)
    {
        Dictionary<string, Bitmask> bitmasksByCategory = new ();

        var utilitiesCount = virtuallyIndexedUtilities.Length;
        for (var i = 0; i < utilitiesCount; i++)
        {
            var category = virtuallyIndexedUtilities[i].Category;
            if (!bitmasksByCategory.ContainsKey(category)) bitmasksByCategory[category] = new Bitmask(utilitiesCount, initializeWithZeros: true);
            bitmasksByCategory[category].Set(i);
        }

        return bitmasksByCategory;
    }

    private static IDictionary<string, IDictionary<string, Bitmask>> ExtractCharacteristicBitmasks(ICleanUtilityDescriptor[] virtuallyIndexedUtilities)
    {
        Dictionary<string, IDictionary<string, Bitmask>> bitmasksByCategory = new ();

        var utilitiesCount = virtuallyIndexedUtilities.Length;
        for (var i = 0; i < utilitiesCount; i++)
        {
            var category = virtuallyIndexedUtilities[i].Category;
            if (!bitmasksByCategory.ContainsKey(category)) bitmasksByCategory[category] = new Dictionary<string, Bitmask>();

            foreach (var characteristic in virtuallyIndexedUtilities[i].Characteristics)
            {
                if (!bitmasksByCategory[category].ContainsKey(characteristic)) bitmasksByCategory[category][characteristic] = new Bitmask(utilitiesCount, initializeWithZeros: true);
                bitmasksByCategory[category][characteristic].Set(i);
            }
        }

        return bitmasksByCategory;
    }

    private static Bitmask[] ExtractUtilityBitmasks(ICleanUtilityDescriptor[] virtuallyIndexedUtilities, IDictionary<string, Bitmask> bitmaskByCategory, IDictionary<string, IDictionary<string, Bitmask>> bitmaskByCategoryAndCharacteristic)
    {
        var utilitiesCount = virtuallyIndexedUtilities.Length;
        var result = new Bitmask[utilitiesCount];

        for (var i = 0; i < virtuallyIndexedUtilities.Length; i++)
        {
            var category = virtuallyIndexedUtilities[i].Category;
            result[i] = ~bitmaskByCategory[category];

            foreach (var (demandCategory, demandsForCategory) in virtuallyIndexedUtilities[i].ExternalDemands)
            {
                if (!bitmaskByCategoryAndCharacteristic.ContainsKey(demandCategory)) continue;

                var baseCompatibilityBitmask = ~bitmaskByCategory[demandCategory];
                foreach (var demand in demandsForCategory)
                {
                    var compatibilityBitmask = baseCompatibilityBitmask;
                    if (bitmaskByCategoryAndCharacteristic[demandCategory].TryGetValue(demand, out var demandBitmask)) compatibilityBitmask |= demandBitmask;
                    result[i] &= compatibilityBitmask;
                }
            }
        }

        return result;
    }

    private static Func<ArraySegment<int>, Bitmask, bool> BuildContinuationCheck(ICleanUtilityDescriptor[] virtuallyIndexedUtilities, IDictionary<string, Bitmask> bitmaskByCategory) 
        => (usedUtilityIndices, accumulatedBitmask) =>
        {
            var categoriesToIgnore = new HashSet<string>();
            foreach (var utilityId in usedUtilityIndices) categoriesToIgnore.Add(virtuallyIndexedUtilities[utilityId].Category);

            return bitmaskByCategory.All(x => categoriesToIgnore.Contains(x.Key) || (accumulatedBitmask & x.Value).CountSetBits() > 0);
        };

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

    private IEnumerable<T> Dfs<T>(int slotIndex, int[] slots, Bitmask accumulatedBitmask, Bitmask[] bitmasks, Func<int[], T> transform, Func<ArraySegment<int>, Bitmask, bool>? continuationCheck = null)
    {
        if (slotIndex == slots.Length) yield return transform(slots);
        else if (continuationCheck is null || continuationCheck(new ArraySegment<int>(slots, 0, slotIndex), accumulatedBitmask))
        {
            var utilityIndex = accumulatedBitmask.FindMostSignificantSetBit();
            while (utilityIndex != -1)
            {
                slots[slotIndex] = utilityIndex;
                foreach (var combination in this.Dfs(slotIndex + 1, slots, accumulatedBitmask & bitmasks[utilityIndex], bitmasks, transform, continuationCheck))
                    yield return combination;

                accumulatedBitmask.Unset(utilityIndex);
                utilityIndex = accumulatedBitmask.FindMostSignificantSetBit();
            }
        }
    }
}