namespace TryAtSoftware.CleanTests.Core.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using TryAtSoftware.CleanTests.Core.Extensions;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class GraphIterator
{
    private readonly ICleanTestInitializationCollection<ICleanUtilityDescriptor> _utilities;

    public GraphIterator(ICleanTestInitializationCollection<ICleanUtilityDescriptor> utilities)
    {
        this._utilities = utilities ?? throw new ArgumentNullException(nameof(utilities));
    }

    public IEnumerable<IDictionary<string, string>> Iterate(IDictionary<string, HashSet<string>>? initialDemands = null)
    {
        Dictionary<string, Dictionary<string, int>> demands = new ();
        foreach (var (category, initialDemandsForCategory) in initialDemands.OrEmptyIfNull())
        {
            demands[category] = new Dictionary<string, int>();
            foreach (var d in initialDemandsForCategory.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) demands[category][d] = 1;
        }

        var categories = this._utilities.Categories.ToArray();

        Dictionary<string, ICleanUtilityDescriptor> currentVariation = new ();
        List<IDictionary<string, string>> resultBag = new ();

        Dfs(categoryIndex: 0);
        return resultBag;
        
        void Dfs(int categoryIndex)
        {
            if (categoryIndex == categories.Length)
            {
                resultBag.Add(currentVariation.ToDictionary(x => x.Key, x => x.Value.Id));
                return;
            }

            var currentCategory = categories[categoryIndex];

            var requiredCharacteristics = demands.EnsureValue(currentCategory).Keys.ToArray();
            foreach (var utility in this._utilities.Get(currentCategory))
            {
                var canBeUsed = true;
                for (var i = 0; canBeUsed && i < requiredCharacteristics.Length; i++) canBeUsed = utility.ContainsCharacteristic(requiredCharacteristics[i]);
            
                if (!canBeUsed) continue;

                foreach (var (externalDemandsCategory, externalDemandsValues) in utility.ExternalDemands)
                {
                    if (currentVariation.ContainsKey(externalDemandsCategory))
                    {
                        if (!currentVariation[externalDemandsCategory].FulfillsAllDemands(externalDemandsValues))
                        {
                            canBeUsed = false;
                            break;
                        }
                    }
                    else
                    {
                        demands.EnsureValue(externalDemandsCategory);

                        foreach (var v in externalDemandsValues)
                        {
                            if (!demands[externalDemandsCategory].ContainsKey(v)) demands[externalDemandsCategory][v] = 0;
                            demands[externalDemandsCategory][v]++;
                        }    
                    }
                }
                
                if (!canBeUsed) continue;

                currentVariation[currentCategory] = utility;
                Dfs(categoryIndex + 1);

                currentVariation.Remove(currentCategory);
                foreach (var (externalDemandsCategory, externalDemandsValues) in utility.ExternalDemands)
                {
                    if (currentVariation.ContainsKey(externalDemandsCategory)) continue;
                    
                    foreach (var v in externalDemandsValues)
                    {
                        if (demands[externalDemandsCategory][v] == 1) demands[externalDemandsCategory].Remove(v);
                        else demands[externalDemandsCategory][v]--;
                    }
                }
            }
        }
    }
}