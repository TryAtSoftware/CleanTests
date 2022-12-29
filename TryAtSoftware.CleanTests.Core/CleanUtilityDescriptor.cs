namespace TryAtSoftware.CleanTests.Core;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class CleanUtilityDescriptor : ICleanUtilityDescriptor
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
    public IReadOnlyCollection<string> Characteristics => this._characteristics.AsReadOnlyCollection();

    /// <inheritdoc />
    public ICleanTestInitializationCollection<string> ExternalDemands { get; } = new CleanTestInitializationCollection<string>();

    /// <inheritdoc />
    public ICleanTestInitializationCollection<string> InternalDemands { get; } = new CleanTestInitializationCollection<string>();

    /// <inheritdoc />
    public HashSet<string> InternalRequirements { get; } = new ();

    public CleanUtilityDescriptor(string initializationCategory, Guid id, Type type, string displayName, bool isGlobal, IEnumerable<string>? characteristics, IEnumerable<string>? requirements)
    {
        this.Category = initializationCategory;
        this.Id = id;
        this.Type = type ?? throw new ArgumentNullException(nameof(type));
        this.DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        this.IsGlobal = isGlobal;
        foreach (var characteristic in characteristics.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this._characteristics.Add(characteristic);
        foreach (var requirement in requirements.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this.InternalRequirements.Add(requirement);
    }

    public bool ContainsCharacteristic(string characteristic)
    {
        if (string.IsNullOrWhiteSpace(characteristic)) return false;
        return this._characteristics.Contains(characteristic);
    }
}