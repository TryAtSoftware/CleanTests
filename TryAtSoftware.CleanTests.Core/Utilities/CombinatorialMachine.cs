namespace TryAtSoftware.CleanTests.Core.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

    private Dictionary<string, HashSet<string>> DiscoverIncompatibleUtilities()
    {
        Dictionary<string, Dictionary<string, HashSet<string>>> characteristicsRegister = new ();
        foreach (var category in this._utilities.Categories) characteristicsRegister[category] = new Dictionary<string, HashSet<int>>();

        Dictionary<string, HashSet<string>> ans = new ();
        foreach (var utility in this._utilities.GetAllValues())
        {
            ans[utility.Id] = new HashSet<string>();

            foreach (var characteristic in utility.Characteristics)
            {
                if (!characteristicsRegister[utility.Category].ContainsKey(characteristic)) characteristicsRegister[utility.Category][characteristic] = new HashSet<string>();
                characteristicsRegister[utility.Category][characteristic].Add(utility.Id);
            }
        }

        foreach (var utility in this._utilities.GetAllValues())
        {
            foreach (var (category, demandsForCategory) in utility.ExternalDemands)
            {
                if (!this._utilities.ContainsCategory(category)) continue;
                
                foreach (var demand in demandsForCategory)
                {
                    Func<ICleanUtilityDescriptor, bool>? predicate = null;
                    if (characteristicsRegister[category].ContainsKey(demand)) predicate = x => !characteristicsRegister[category][demand].Contains(x.Id);

                    foreach (var otherUtility in this._utilities.Get(category).SafeWhere(predicate))
                    {
                        ans[utility.Id].Add(otherUtility.Id);
                        ans[otherUtility.Id].Add(utility.Id);
                    }
                }
            }
        }

        return ans;
    }

    private static bool RegisterExternalDemands(ICleanUtilityDescriptor cleanUtilityDescriptor, IDictionary<string, ICleanUtilityDescriptor> slots, IDictionary<string, Dictionary<string, int>> activeDemands)
    {
        HashSet<string> checkedCategories = new ();
        foreach (var (category, demands) in cleanUtilityDescriptor.ExternalDemands)
        {
            if (!slots.TryGetValue(category, out var otherUtilityToCheck)) continue;

            if (!otherUtilityToCheck.FulfillsAllDemands(demands)) return false;
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