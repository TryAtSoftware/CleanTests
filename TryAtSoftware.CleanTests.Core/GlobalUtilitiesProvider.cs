namespace TryAtSoftware.CleanTests.Core;

using System;
using System.Collections.Generic;
using TryAtSoftware.CleanTests.Core.Interfaces;

public class GlobalUtilitiesProvider : IGlobalUtilitiesProvider
{
    private readonly Dictionary<string, object> _utilities = new ();

    public bool RegisterUtility(string uniqueId, object instance)
    {
        if (!this.IsRegistered(uniqueId)) this._utilities[uniqueId] = new Dictionary<Type, object>();

        this._utilities[uniqueId] = instance;
        return true;
    }

    public bool IsRegistered(string uniqueId) => this._utilities.ContainsKey(uniqueId);

    public object? GetUtility(string uniqueId) => this._utilities.GetValueOrDefault(uniqueId);

    public IEnumerable<T> GetUtilities<T>()
    {
        foreach (var (_, utility) in this._utilities)
            if (utility is T t) yield return t;
    }
}