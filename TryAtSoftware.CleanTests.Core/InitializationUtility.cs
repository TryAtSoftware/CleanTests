namespace TryAtSoftware.CleanTests.Core;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.XUnit.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class InitializationUtility : IInitializationUtility
{
    private readonly HashSet<string> _characteristics = new();

    /// <inheritdoc />
    public string Category { get; }
        
    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public Type Type { get; }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public bool IsGlobal { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<string> Characteristics => this._characteristics.AsReadonlySafe();

    /// <inheritdoc />
    public ICleanTestInitializationCollection<string> GlobalDemands { get; } = new CleanTestInitializationCollection<string>();

    /// <inheritdoc />
    public ICleanTestInitializationCollection<string> LocalDemands { get; } = new CleanTestInitializationCollection<string>();

    /// <inheritdoc />
    public HashSet<string> Requirements { get; } = new ();
    

    public InitializationUtility(string initializationCategory, Guid id, Type type, string displayName, bool isGlobal, IEnumerable<string>? characteristics, IEnumerable<string>? requirements)
    {
        this.Category = initializationCategory;
        this.Id = id;
        this.Type = type ?? throw new ArgumentNullException(nameof(type));
        this.DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        this.IsGlobal = isGlobal;
        foreach (var characteristic in characteristics.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this._characteristics.Add(characteristic);
        foreach (var requirement in requirements.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this.Requirements.Add(requirement);
    }

    public bool ContainsCharacteristic(string characteristic)
    {
        if (string.IsNullOrWhiteSpace(characteristic)) return false;
        return this._characteristics.Contains(characteristic);
    }
}