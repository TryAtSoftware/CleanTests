namespace TryAtSoftware.CleanTests.UnitTests.Parametrization;

using TryAtSoftware.CleanTests.Core;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class EnvironmentSetup
{
    private readonly Dictionary<string, int> _numberOfUtilitiesPerCategory = new ();
    private readonly Dictionary<string, Dictionary<string, List<string>>> _externalDemandsPerUtility = new ();
    private readonly Dictionary<string, Dictionary<string, List<string>>> _outerDemandsPerUtility = new ();
    private readonly Dictionary<string, List<string>> _characteristics = new ();
    private readonly Dictionary<string, List<string>> _requirements = new ();

    public EnvironmentSetup(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        this.Name = name;
    }

    public string Name { get; }
    public int CategoriesCount => this._numberOfUtilitiesPerCategory.Count;
    
    public EnvironmentSetup WithCategory(string category, int utilitiesCount)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException(nameof(category));
        if (utilitiesCount <= 0) throw new ArgumentException("The number of utilities for each category must be at least 1", nameof(utilitiesCount));
        
        if (this._numberOfUtilitiesPerCategory.ContainsKey(category)) throw new InvalidOperationException("Category with that name has already been registered.");
        this._numberOfUtilitiesPerCategory[category] = utilitiesCount;
        return this;
    }

    public EnvironmentSetup WithCharacteristics(string category, int utilityId, params string[] characteristics)
    {
        this.ValidateUtilityExists(category, utilityId);

        var universalId = ComposeUniversalUtilityId(category, utilityId);
        if (!this._characteristics.ContainsKey(universalId)) this._characteristics[universalId] = new List<string>();
        foreach (var characteristic in characteristics.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this._characteristics[universalId].Add(characteristic);

        return this;
    }

    public EnvironmentSetup WithRequirements(string category, int utilityId, params string[] requirements)
    {
        this.ValidateUtilityExists(category, utilityId);

        var universalId = ComposeUniversalUtilityId(category, utilityId);
        if (!this._requirements.ContainsKey(universalId)) this._requirements[universalId] = new List<string>();
        foreach (var characteristic in requirements.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this._requirements[universalId].Add(characteristic);

        return this;
    }

    public EnvironmentSetup WithExternalDemands(string utilityCategory, int utilityId, string demandsCategory, params string[] demands)
    {
        this.ValidateUtilityExists(utilityCategory, utilityId);
        
        var universalId = ComposeUniversalUtilityId(utilityCategory, utilityId);
        if (!this._externalDemandsPerUtility.ContainsKey(universalId)) this._externalDemandsPerUtility[universalId] = new Dictionary<string, List<string>>();
        if (!this._externalDemandsPerUtility[universalId].ContainsKey(demandsCategory)) this._externalDemandsPerUtility[universalId][demandsCategory] = new List<string>();
        foreach (var demand in demands.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this._externalDemandsPerUtility[universalId][demandsCategory].Add(demand);

        return this;
    }

    public EnvironmentSetup WithOuterDemands(string utilityCategory, int utilityId, string demandsCategory, params string[] demands)
    {
        this.ValidateUtilityExists(utilityCategory, utilityId);
        
        var universalId = ComposeUniversalUtilityId(utilityCategory, utilityId);
        if (!this._outerDemandsPerUtility.ContainsKey(universalId)) this._outerDemandsPerUtility[universalId] = new Dictionary<string, List<string>>();
        if (!this._outerDemandsPerUtility[universalId].ContainsKey(demandsCategory)) this._outerDemandsPerUtility[universalId][demandsCategory] = new List<string>();
        foreach (var demand in demands.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this._outerDemandsPerUtility[universalId][demandsCategory].Add(demand);

        return this;
    }

    public ICleanTestInitializationCollection<ICleanUtilityDescriptor> Materialize()
    {
        var utilitiesCollection = new CleanTestInitializationCollection<ICleanUtilityDescriptor>();

        foreach (var (category, utilitiesCount) in this._numberOfUtilitiesPerCategory)
        {
            for (var j = 1; j <= utilitiesCount; j++)
            {
                var universalId = ComposeUniversalUtilityId(category, j);

                this._characteristics.TryGetValue(universalId, out var characteristics);
                this._requirements.TryGetValue(universalId, out var requirements);
                ICleanUtilityDescriptor utility = new CleanUtilityDescriptor(category, typeof(int), universalId, isGlobal: false, characteristics, requirements);

                this._externalDemandsPerUtility.TryGetValue(universalId, out var externalDemandsByCategory);
                foreach (var (demandCategory, demands) in externalDemandsByCategory.OrEmptyIfNull())
                {
                    foreach (var demand in demands) utility.ExternalDemands.Register(demandCategory, demand);
                }
                
                this._outerDemandsPerUtility.TryGetValue(universalId, out var outerDemandsByCategory);
                foreach (var (demandCategory, demands) in outerDemandsByCategory.OrEmptyIfNull())
                {
                    foreach (var demand in demands) utility.OuterDemands.Register(demandCategory, demand);
                }

                utilitiesCollection.Register(category, utility);
            }
        }

        return utilitiesCollection;
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