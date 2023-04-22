namespace TryAtSoftware.CleanTests.Core;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Interfaces;
using TryAtSoftware.Extensions.Collections;

public class CleanUtilityDescriptor : ICleanUtilityDescriptor
{
    private readonly HashSet<string> _characteristics = new();
    private readonly HashSet<string> _internalRequirements = new ();

    /// <inheritdoc />
    public string Category { get; }

    /// <inheritdoc />
    public Type Type { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public bool IsGlobal { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<string> Characteristics => this._characteristics.AsReadOnlyCollection();

    /// <inheritdoc />
    public ICleanTestInitializationCollection<string> ExternalDemands { get; } = new CleanTestInitializationCollection<string>();

    /// <inheritdoc />
    public ICleanTestInitializationCollection<string> InternalDemands { get; } = new CleanTestInitializationCollection<string>();

    /// <inheritdoc />
    public IReadOnlyCollection<string> InternalRequirements => this._internalRequirements.AsReadOnlyCollection();

    public CleanUtilityDescriptor(string initializationCategory, Type type, string displayName, bool isGlobal, IEnumerable<string>? characteristics = null, IEnumerable<string>? requirements = null)
    {
        this.Category = initializationCategory;
        this.Type = type ?? throw new ArgumentNullException(nameof(type));
        this.Name = displayName ?? throw new ArgumentNullException(nameof(displayName));
        this.IsGlobal = isGlobal;
        foreach (var characteristic in characteristics.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this._characteristics.Add(characteristic);
        foreach (var requirement in requirements.OrEmptyIfNull().IgnoreNullOrWhitespaceValues()) this._internalRequirements.Add(requirement);
    }

    public bool ContainsCharacteristic(string characteristic)
    {
        if (string.IsNullOrWhiteSpace(characteristic)) return false;
        return this._characteristics.Contains(characteristic);
    }
}