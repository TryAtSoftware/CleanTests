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
        var incompatibilityFactorByUtilityId = this.DiscoverIncompatibleUtilities();
        var categories = this._utilities.Categories.OrderByDescending(x => this._utilities.Get(x).Sum(y => incompatibilityFactorByUtilityId[y.Id].Count)).ToArray();

        Dictionary<string, int> utilityIsForbiddenBy = new ();
        foreach (var utility in this._utilities.GetAllValues()) utilityIsForbiddenBy[utility.Id] = 0;
        
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

            foreach (var utility in this._utilities.Get(currentCategory))
            {
                if (utilityIsForbiddenBy[utility.Id] > 0) continue;

                foreach (var iu in incompatibilityFactorByUtilityId[utility.Id]) utilityIsForbiddenBy[iu]++;
                
                slots[currentCategory] = utility;
                Dfs(categoryIndex + 1);
                slots.Remove(currentCategory);

                foreach (var iu in incompatibilityFactorByUtilityId[utility.Id]) utilityIsForbiddenBy[iu]--;
            }
        }
    }

    private Dictionary<string, HashSet<string>> DiscoverIncompatibleUtilities()
    {
        Dictionary<string, Dictionary<string, HashSet<string>>> characteristicsRegister = new ();
        foreach (var category in this._utilities.Categories) characteristicsRegister[category] = new Dictionary<string, HashSet<string>>();

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
}