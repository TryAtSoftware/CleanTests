namespace TryAtSoftware.CleanTests.Core.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class CombinatorialMachine
{
    private readonly ICleanTestInitializationCollection<ICleanUtilityDescriptor> _utilities;

    public CombinatorialMachine(ICleanTestInitializationCollection<ICleanUtilityDescriptor> utilities)
    {
        this._utilities = utilities ?? throw new ArgumentNullException(nameof(utilities));
    }

    // TODO: Describe why dynamically changing the categories order (based on the external demands of already iterated utilities) does not work.
    public IEnumerable<IDictionary<string, string>> GenerateAllCombinations()
    {
        var (incompatibleUtilitiesMap, incompatibilityEnforcersByCategory, incompatibilityFactorByCategory) = this.DiscoverIncompatibleUtilities();
        var categories = this._utilities.Categories.OrderByDescending(x => incompatibilityEnforcersByCategory[x]).ThenByDescending(x => incompatibilityFactorByCategory[x]).ToArray();

        Dictionary<string, int> incompatibilityFactorByUtilityId = new ();
        foreach (var utility in this._utilities.GetAllValues()) incompatibilityFactorByUtilityId[utility.Id] = 0;
        
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
                if (incompatibilityFactorByUtilityId[utility.Id] > 0) continue;

                foreach (var iu in incompatibleUtilitiesMap[utility.Id]) incompatibilityFactorByUtilityId[iu]++;
                
                slots[currentCategory] = utility;
                Dfs(categoryIndex + 1);
                slots.Remove(currentCategory);

                foreach (var iu in incompatibleUtilitiesMap[utility.Id]) incompatibilityFactorByUtilityId[iu]--;
            }
        }
    }

    public BigInteger CalculateNumberOfCombinations()
    {
        BigInteger product = 1;
        foreach (var category in this._utilities.Categories) product = BigInteger.Multiply(product, this._utilities.Count(category));

        var incompatiblePairs = new List<(string, string)>();
        var visited = new HashSet<string>();

        var (incompatibleUtilitiesMap, _, _) = this.DiscoverIncompatibleUtilities();
        foreach (var (principalUtility, incompatibleUtilities) in incompatibleUtilitiesMap)
        {
            foreach (var incompatibleUtility in incompatibleUtilities.Where(x => !visited.Contains(x))) incompatiblePairs.Add((principalUtility, incompatibleUtility));
            visited.Add(principalUtility);
        }

        var categoryByUtilityId = new Dictionary<string, string>();
        foreach (var utility in this._utilities.GetAllValues()) categoryByUtilityId[utility.Id] = utility.Category;

        var used = new Dictionary<string, string>();
        BigInteger ans = 0;
        for (var i = 0; i < incompatiblePairs.Count; i++) Solve(i, add: true, product);

        return product - ans;
        
        void Solve(int index, bool add, BigInteger current)
        {
            if (index == incompatiblePairs.Count) return;

            var (first, second) = incompatiblePairs[index];
            var (firstCategory, secondCategory) = (categoryByUtilityId[first], categoryByUtilityId[second]);

            if (!used.ContainsKey(firstCategory)) current = BigInteger.Divide(current, this._utilities.Count(firstCategory));
            else if (used[firstCategory] != first) return;
            
            if (!used.ContainsKey(secondCategory)) current = BigInteger.Divide(current, this._utilities.Count(secondCategory));
            else if (used[secondCategory] != second) return;

            used[firstCategory] = first;
            used[secondCategory] = second;

            if (add) ans += current;
            else ans -= current;
            
            for (var i = index + 1; i < incompatiblePairs.Count; i++) Solve(i, !add, current);

            used.Remove(firstCategory);
            used.Remove(secondCategory);
        }
    }

    public (Dictionary<string, HashSet<string>> IncompatibleUtilitiesMap, Dictionary<string, int> IncompatibilityEnforcersByCategory, Dictionary<string, int> IncompatibilityFactorByCategory) DiscoverIncompatibleUtilities()
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

                    foreach (var otherUtility in this._utilities.Get(demandCategory).SafeWhere(predicate))
                    {
                        if (incompatibleUtilitiesMap[utility.Id].Add(otherUtility.Id))
                        {
                            incompatibilityEnforcersByCategory[utility.Category]++;
                            incompatibilityFactorByCategory[utility.Category]++;
                        }

                        if (incompatibleUtilitiesMap[otherUtility.Id].Add(utility.Id)) incompatibilityFactorByCategory[demandCategory]++;
                    }
                }
            }
        }

        return (incompatibleUtilitiesMap, incompatibilityEnforcersByCategory, incompatibilityFactorByCategory);
    }
}