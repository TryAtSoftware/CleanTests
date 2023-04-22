namespace TryAtSoftware.CleanTests.UnitTests.Parametrization;

using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.CleanTests.Core.Utilities;
using TryAtSoftware.Extensions.Collections;

public class CombinatorialMachineSetup
{
    private readonly Dictionary<string, int> _numberOfUtilitiesPerCategory = new ();
    private readonly Dictionary<string, Dictionary<string, List<string>>> _demandsPerUtility = new ();
    private readonly Dictionary<string, List<string>> _characteristics = new ();

    public CombinatorialMachineSetup(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
    }

    public string Name { get; }
    
    public CombinatorialMachineSetup WithCategory(string category, int utilitiesCount)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException(nameof(category));
        if (utilitiesCount <= 0) throw new ArgumentException("The number of utilities for each category must be at least 1", nameof(utilitiesCount));
        
        if (this._numberOfUtilitiesPerCategory.ContainsKey(category)) throw new InvalidOperationException("Category with that name has already been registered.");
        this._numberOfUtilitiesPerCategory[category] = utilitiesCount;
        return this;
    }

    public CombinatorialMachineSetup WithCharacteristics(string category, int utilityId, params string[] characteristics)
    {
        this.ValidateUtilityExists(category, utilityId);

        var universalId = ComposeUniversalUtilityId(category, utilityId);
        if (!this._characteristics.ContainsKey(universalId)) this._characteristics[universalId] = new List<string>();
        foreach (var characteristic in characteristics.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this._characteristics[universalId].Add(characteristic);

        return this;
    }

    public CombinatorialMachineSetup WithDemands(string utilityCategory, int utilityId, string demandsCategory, params string[] demands)
    {
        this.ValidateUtilityExists(utilityCategory, utilityId);
        this.ValidateCategoryExists(demandsCategory);
        
        var universalId = ComposeUniversalUtilityId(utilityCategory, utilityId);
        if (!this._demandsPerUtility.ContainsKey(universalId)) this._demandsPerUtility[universalId] = new Dictionary<string, List<string>>();
        if (!this._demandsPerUtility[universalId].ContainsKey(demandsCategory)) this._demandsPerUtility[universalId][demandsCategory] = new List<string>();
        foreach (var demand in demands.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this._demandsPerUtility[universalId][demandsCategory].Add(demand);

        return this;
    }

    public (CombinatorialMachine CombinatorialMachine, IDictionary<string, ICleanUtilityDescriptor> UtilitiesById) Materialize()
    {
        var utilitiesCollection = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();
        var utilitiesById = new Dictionary<string, ICleanUtilityDescriptor>();

        foreach (var (category, utilitiesCount) in this._numberOfUtilitiesPerCategory)
        {
            for (var j = 1; j <= utilitiesCount; j++)
            {
                var universalId = ComposeUniversalUtilityId(category, j);

                this._characteristics.TryGetValue(universalId, out var characteristics);
                ICleanUtilityDescriptor utility = new CleanUtilityDescriptor(category, typeof(int), universalId, isGlobal: false, characteristics);

                this._demandsPerUtility.TryGetValue(universalId, out var demandsByCategory);
                foreach (var (demandCategory, demands) in demandsByCategory.OrEmptyIfNull())
                {
                    foreach (var demand in demands) utility.ExternalDemands.Register(demandCategory, demand);
                }

                utilitiesCollection.Register(category, utility);
                utilitiesById[utility.Id] = utility;
            }
        }

        return (CombinatorialMachine: new CombinatorialMachine(utilitiesCollection), UtilitiesById: utilitiesById);
    }

    private void ValidateUtilityExists(string category, int utilityId)
    {
        this.ValidateCategoryExists(category);
        
        var upperBound = this._numberOfUtilitiesPerCategory[category];
        if (utilityId <= 0 || utilityId > upperBound) throw new ArgumentException($"Invalid utility id. It should be in the range [1, {upperBound}]", nameof(utilityId));
    }

    private void ValidateCategoryExists(string category)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException(nameof(category));
        if (!this._numberOfUtilitiesPerCategory.ContainsKey(category)) throw new InvalidOperationException("Category with that name has not been registered.");
    }

    private static string ComposeUniversalUtilityId(string category, int utilityId) => $"{category}{utilityId}";

    public override string ToString() => this.Name;
}