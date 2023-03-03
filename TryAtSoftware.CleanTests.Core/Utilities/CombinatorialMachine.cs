namespace TryAtSoftware.CleanTests.Core.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Extensions;
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
        var categories = this._utilities.Categories.ToArray();

        Dictionary<string, Dictionary<string, int>> activeDemands = new ();
        Dictionary<string, ICleanUtilityDescriptor> slots = new ();
        List<IDictionary<string, string>> resultBag = new ();

        Dfs(categoryIndex: 0);
        return resultBag;

        void Dfs(int categoryIndex)
        {
            if (categoryIndex == categories.Length)
            {
                resultBag.Add(slots.ToDictionary(x => x.Key, x => x.Value.Id));
                return;
            }

            var currentCategory = categories[categoryIndex];

            var requiredCharacteristics = activeDemands.EnsureValue(currentCategory).Keys.ToArray();
            foreach (var utility in this._utilities.Get(currentCategory))
            {
                var canBeUsed = utility.FulfillsAllDemands(requiredCharacteristics);
                if (!canBeUsed || !RegisterExternalDemands(utility, slots, activeDemands)) continue;

                slots[currentCategory] = utility;
                Dfs(categoryIndex + 1);
                slots.Remove(currentCategory);

                CleanupExternalDemands(utility, activeDemands);
            }
        }
    }

    private static bool RegisterExternalDemands(ICleanUtilityDescriptor cleanUtilityDescriptor, IDictionary<string, ICleanUtilityDescriptor> slots, IDictionary<string, Dictionary<string, int>> activeDemands)
    {
        HashSet<string> checkedCategories = new ();
        foreach (var (category, demands) in cleanUtilityDescriptor.ExternalDemands)
        {
            if (slots.ContainsKey(category) && !slots[category].FulfillsAllDemands(demands)) return false;
            checkedCategories.Add(category);
        }

        foreach (var (category, demands) in cleanUtilityDescriptor.ExternalDemands)
        {
            if (checkedCategories.Contains(category)) continue;
            
            var activeDemandsForCategory = activeDemands.EnsureValue(category);
            foreach (var demand in demands)
            {
                if (!activeDemandsForCategory.ContainsKey(demand)) activeDemandsForCategory[demand] = 0;
                activeDemandsForCategory[demand]++;
            }
        }

        return true;
    }

    private static void CleanupExternalDemands(ICleanUtilityDescriptor cleanUtilityDescriptor, IDictionary<string, Dictionary<string, int>> activeDemands)
    {
        foreach (var (category, demands) in cleanUtilityDescriptor.ExternalDemands)
        {
            if (!activeDemands.TryGetValue(category, out var activeDemandsForCategory)) continue;

            foreach (var demand in demands)
            {
                if (!activeDemandsForCategory.TryGetValue(demand, out var demandOccurrences)) continue;

                if (demandOccurrences == 1) activeDemandsForCategory.Remove(demand);
                else activeDemandsForCategory[demand] = demandOccurrences - 1;
            }
        }
    }
}