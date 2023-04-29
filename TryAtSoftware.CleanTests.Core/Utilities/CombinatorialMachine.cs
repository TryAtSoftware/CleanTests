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
        var (incompatibleUtilitiesMap, incompatibilityEnforcersByCategory, incompatibilityFactorByCategory) = this.DiscoverIncompatibleUtilities();
        var categories = this._utilities.Categories.OrderByDescending(x => incompatibilityEnforcersByCategory[x]).ThenByDescending(x => incompatibilityFactorByCategory[x]).ToArray();

        Dictionary<string, int> incompatibilityFactorByUtilityId = new ();
        foreach (var utility in this._utilities.GetAllValues()) incompatibilityFactorByUtilityId[utility.Id] = 0;

        var slots = new string[categories.Length];
        List<IDictionary<string, string>> resultBag = new ();

        Dfs(slotIndex: 0);
        return resultBag;

        void Dfs(int slotIndex)
        {
            if (slotIndex == slots.Length)
            {
                var result = new Dictionary<string, string>();
                for (var i = 0; i < slots.Length; i++) result[categories[i]] = slots[i];

                resultBag.Add(result);
                return;
            }

            var currentCategory = categories[slotIndex];

            foreach (var utility in this._utilities.Get(currentCategory))
            {
                if (incompatibilityFactorByUtilityId[utility.Id] > 0) continue;

                foreach (var iu in incompatibleUtilitiesMap[utility.Id]) incompatibilityFactorByUtilityId[iu]++;
                
                slots[slotIndex] = utility.Id;
                Dfs(slotIndex + 1);

                foreach (var iu in incompatibleUtilitiesMap[utility.Id]) incompatibilityFactorByUtilityId[iu]--;
            }
        }
    }

    private (Dictionary<string, HashSet<string>> IncompatibleUtilitiesMap, Dictionary<string, int> IncompatibilityEnforcersByCategory, Dictionary<string, int> incompatibilityFactorByCategory) DiscoverIncompatibleUtilities()
    {
        Dictionary<string, int> incompatibilityEnforcersByCategory = new (), incompatibilityFactorByCategory = new ();
        Dictionary<string, Dictionary<string, HashSet<string>>> characteristicsRegister = new ();
        foreach (var category in this._utilities.Categories)
        {
            incompatibilityEnforcersByCategory[category] = 0;
            incompatibilityFactorByCategory[category] = 0;
            characteristicsRegister[category] = new Dictionary<string, HashSet<string>>();
        }

        Dictionary<string, HashSet<string>> incompatibleUtilitiesMap = new ();
        foreach (var utility in this._utilities.GetAllValues())
        {
            incompatibleUtilitiesMap[utility.Id] = new HashSet<string>();

            foreach (var characteristic in utility.Characteristics)
            {
                if (!characteristicsRegister[utility.Category].ContainsKey(characteristic)) characteristicsRegister[utility.Category][characteristic] = new HashSet<string>();
                characteristicsRegister[utility.Category][characteristic].Add(utility.Id);
            }
        }

        foreach (var utility in this._utilities.GetAllValues())
        {
            foreach (var (demandCategory, demandsForCategory) in utility.ExternalDemands)
            {
                if (!this._utilities.ContainsCategory(demandCategory)) continue;
                
                foreach (var demand in demandsForCategory)
                {
                    Func<ICleanUtilityDescriptor, bool>? predicate = null;
                    if (characteristicsRegister[demandCategory].ContainsKey(demand)) predicate = x => !characteristicsRegister[demandCategory][demand].Contains(x.Id);

                    foreach (var incompatibleUtilityId in this._utilities.Get(demandCategory).SafeWhere(predicate).Select(x => x.Id))
                    {
                        if (incompatibleUtilitiesMap[utility.Id].Add(incompatibleUtilityId))
                        {
                            incompatibilityEnforcersByCategory[utility.Category]++;
                            incompatibilityFactorByCategory[utility.Category]++;
                        }

                        if (incompatibleUtilitiesMap[incompatibleUtilityId].Add(utility.Id)) incompatibilityFactorByCategory[demandCategory]++;
                    }
                }
            }
        }

        return (incompatibleUtilitiesMap, incompatibilityEnforcersByCategory, incompatibilityFactorByCategory);
    }
}